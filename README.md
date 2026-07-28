# Mock Payment Gateway

A provider-agnostic **mock payment gateway** that behaves like a real third-party payment
provider (in the spirit of Pesapal, Flutterwave, and other East African payment gateways) so
applications can perform complete end-to-end payment integration testing without touching a real
provider.

Built with **.NET 8**, **MySQL** (via Dapper), Serilog structured logging, and Swagger. Fully
Dockerized.

## Features

- Mobile Money payments (MTN, Airtel) with a simulated STK push → PIN entry → processing flow
- Card payments (Visa, Mastercard) with a simplified processing flow
- A realistic transaction state machine (`Created → Pending → Processing → Completed/Failed/Cancelled/Expired`)
  with timestamped state history
- Payouts (bank transfer and mobile money transfer) with their own lifecycle
- Configurable webhooks: normal, delayed, duplicate, lost, disabled, out-of-order, and
  invalid-signature delivery — all HMAC-signed
- A **Scenario Engine** with 16 built-in scenarios (happy path, declines, timeouts, random success
  rates, stuck transactions, webhook mischief, etc.) — fully configurable in `appsettings.json`
- Two provider profiles — **Generic** and **Pesapal** — that change response shape, status naming,
  and auth mechanism via a single config switch
- API-key authentication (Generic profile) and bearer-token authentication (Pesapal profile)
- Idempotency support via an `Idempotency-Key` header
- Admin/logging APIs for viewing transactions, payouts, webhook deliveries, errors, and scenario
  executions
- Health checks, Swagger UI, structured logging, and Docker Compose for one-command startup

## Quickstart

```bash
docker compose up --build
```

This starts MySQL and the gateway. On first boot the app creates the `paymentmock` database,
applies the schema in `PaymentMock/Scripts/001_create_schema.sql`, and seeds gateway account
balances from `002_seed_data.sql` — no manual migration step required, and it's safe to restart at
any time (scripts are idempotent).

- Swagger UI: http://localhost:8080/swagger
- Health check: http://localhost:8080/health
- API base URL: `http://localhost:8080/api/v1`

Import [`postman/PaymentMock.postman_collection.json`](postman/PaymentMock.postman_collection.json)
into Postman to try every endpoint — it ships with a collection-level `X-API-Key` and example
request bodies for each scenario category.

### Running locally without Docker

You need a MySQL 8 instance reachable from your machine.

```bash
cd PaymentMock
dotnet restore
dotnet run
```

Update `Database:ConnectionString` in `PaymentMock/appsettings.json` (or set the
`Database__ConnectionString` environment variable) to point at your MySQL instance.

## Authentication

Every request (other than `/health`, `/swagger`, and `/api/v1/auth/token`) must be authenticated.
Which mechanism is required depends on the active `GatewayProfile`:

| Profile | Mechanism | How |
|---|---|---|
| `Generic` (default) | API key | Header `X-API-Key: <key>` — keys are configured under `Authentication.ApiKeys` in `appsettings.json` |
| `Pesapal` | Bearer token | `POST /api/v1/auth/token` with a `consumerKey`/`consumerSecret` pair (configured under `Authentication.PesapalConsumerKeys`) returns a token valid for 5 hours; send it as `Authorization: Bearer <token>` |

## Idempotency

Send an `Idempotency-Key` header on `POST /api/v1/payments/*` or `POST /api/v1/payouts` requests.
A repeated key on the same endpoint replays the original response instead of creating a duplicate
payment. Reusing a key with a **different** request body returns `409 Conflict`.

## Provider Profiles

Set `GatewayProfile` in `appsettings.json` to `"Generic"` or `"Pesapal"`. Routes stay identical —
only the response shape, status naming, and auth mechanism change, so a consuming application can
switch profiles by config alone (the guiding principle behind this gateway).

- **Generic**: clean, provider-neutral JSON (`TransactionDto`, `PayoutDto`, etc. — see
  `PaymentMock/DTOs/Generic`).
- **Pesapal**: Pesapal-flavored shapes (`OrderTrackingId`, `PaymentStatusDescription` of
  `COMPLETED`/`FAILED`/`PENDING`/`INVALID`, IPN-style webhook body — see
  `PaymentMock/DTOs/Pesapal`) and bearer-token auth via `/api/v1/auth/token`.

`Flutterwave`, `Eversend`, and `Paystack` are reserved as future profiles (per the design spec) but
are not yet implemented.

## API Reference

Full interactive documentation is in Swagger UI at `/swagger`. Summary:

| Method | Path | Description |
|---|---|---|
| POST | `/api/v1/payments/mobile-money` | Initiate a Mobile Money payment |
| POST | `/api/v1/payments/card` | Initiate a Card payment |
| GET | `/api/v1/transactions/{id}` | Get a transaction |
| GET | `/api/v1/transactions/{id}/status` | Get just the status |
| GET | `/api/v1/transactions` | Search/filter transactions (status, provider, method, merchant reference, date range) |
| POST | `/api/v1/payouts` | Initiate a payout |
| GET | `/api/v1/payouts/{id}` | Get a payout |
| GET | `/api/v1/payouts` | List/filter payouts |
| GET | `/api/v1/account` | Gateway account balances |
| POST | `/api/v1/webhooks/register` | Register a callback URL |
| GET | `/api/v1/webhooks` | List registered callback URLs |
| POST | `/api/v1/auth/token` | Issue a bearer token (Pesapal profile) |
| GET | `/api/v1/admin/transactions` \| `/payouts` \| `/webhooks` \| `/errors` \| `/scenarios` | Admin log viewers |
| GET | `/api/v1/admin/config` \| `/config-history` | Effective configuration and its history |
| GET | `/health` | Health check |

## How a payment flows

1. `POST /api/v1/payments/mobile-money` (or `/card`) validates the request, creates a `Transaction`
   row in `Created` state, and returns immediately — just like a real gateway acknowledging an STK
   push.
2. A background worker (`TransactionProcessingBackgroundService`) picks it up and advances it
   through `Pending` (STK push sent) → `Processing` (PIN entered, bank processing) →
   `Completed`/`Failed`/`Expired`, with realistic, jittered delays at each step (card payments skip
   straight to `Processing`.
3. Every transition is timestamped in `transaction_state_history`.
4. Webhooks fire at `Processing` and at the final state, shaped and signed per the active scenario
   and provider profile, and delivered asynchronously by `WebhookDeliveryBackgroundService` (with
   retries).
5. Poll `GET /api/v1/transactions/{id}` at any time, or rely on the webhook.

Payouts follow the same pattern (`PayoutProcessingBackgroundService`), minus the STK/PIN steps.

Because the gateway persists to MySQL and re-seeds its processing queues from any transactions
still in-flight on startup, an app restart resumes the simulation instead of losing state.

## The Scenario Engine

Pass `"scenario": "<Name>"` in a payment or payout request to force specific behavior (omit it to
use `Gateway.DefaultScenario`, `HappyPath` by default). All scenarios are defined — and fully
tunable — under `Scenarios` in `appsettings.json`.

| Scenario | What happens |
|---|---|
| `HappyPath` | Normal success, standard delays, one webhook |
| `Declined` | Fails after the full processing delay — "Declined by provider" |
| `WrongPin` | Mobile Money — fails right after PIN entry, skipping the bank-processing delay |
| `InsufficientFunds` | Fails after the full processing delay — "Insufficient funds" |
| `InvalidCard` | Card — fails fast, "Card declined by issuer" |
| `InvalidPhoneNumber` | Mobile Money — fails fast, "Phone number not registered with provider" |
| `Timeout` | Customer never responds to the STK push; transitions to `Expired` |
| `SlowProcessing` | Happy-path outcome with all delays multiplied (default 6x) |
| `ProviderUnavailable` | Fails fast — "Payment provider temporarily unavailable" |
| `GatewayError` | Fails fast — "Internal gateway processing error" |
| `DuplicateRequest` | A repeat of an in-flight `merchantReference` returns the **original** transaction instead of creating a new one |
| `RandomSuccessRate` | Each run independently succeeds/fails per `SuccessRatePercent` |
| `NeverCompletes` | Gets stuck at `StuckAtStatus` (default `Processing`) forever — no webhook, ever |
| `WebhookLost` | Happy-path outcome, final webhook is never sent |
| `WebhookDelayed` | Happy-path outcome, final webhook delivery is delayed (`WebhookDelayMs`) |
| `DuplicateWebhook` | Happy-path outcome, final webhook delivered `WebhookDuplicateCount` times |

`SimulateOutOfOrderWebhook` (delays the intermediate "processing" webhook so it arrives after the
final one) and `SimulateInvalidSignature` (signs with a deliberately wrong secret) can be layered
onto any scenario. Format validation (malformed phone numbers, invalid card numbers, unsupported
currencies) is **always** rejected at request time with `400 Bad Request`, regardless of scenario —
only provider-side rejection is scenario-driven.

Every scenario execution is recorded and viewable at `GET /api/v1/admin/scenarios`.

## Webhooks

Webhooks are generic JSON, HMAC-SHA256-signed with `Webhooks.SignatureSecret` and delivered with an
`X-Webhook-Signature` header (hex-encoded). Verify it in your receiver:

```
signature = hex(HMAC_SHA256(secret, raw_request_body))
```

Set a per-payment `callbackUrl`, or register a default one via `POST /api/v1/webhooks/register`.
`Webhooks.Enabled: false` disables delivery globally. Delivery attempts, retries, and failures are
visible at `GET /api/v1/admin/webhooks`.

## Configuration reference (`appsettings.json`)

| Key | Purpose |
|---|---|
| `GatewayProfile` | `Generic` or `Pesapal` |
| `Database.ConnectionString` | MySQL connection string |
| `Authentication.ApiKeys` | Valid `X-API-Key` values for the Generic profile |
| `Authentication.PesapalConsumerKeys` | Valid consumer key/secret pairs for the Pesapal profile |
| `Gateway.SupportedCurrencies` / `SupportedProviders` | Whitelist enforced by validation |
| `Gateway.DefaultScenario` | Scenario used when a request doesn't specify one |
| `Gateway.ProcessingDelays` | Base delay (ms) for each simulated step |
| `Gateway.TimingVariancePercent` | ± jitter applied to every delay |
| `Scenarios.<Name>` | Per-scenario outcome/timing/webhook overrides (see table above) |
| `Webhooks.*` | Global enable flag, default callback URL, signing secret, retry policy |

Every value can be overridden via environment variables using the standard ASP.NET Core
double-underscore convention, e.g. `Gateway__ProcessingDelays__ProcessingMs=500`.

## Project structure

```
PaymentMock/
  Controllers/{Interfaces,Implementations}   Thin HTTP endpoints
  Services/{Interfaces,Implementations}      Business logic, scenario engine, provider profiles
  Services/Background                        Hosted services driving the state machines & webhook delivery
  Repositories/{Interfaces,Implementations}  Dapper/MySQL data access
  Models/                                    Persistence entities
  DTOs/{Generic,Pesapal}                     Profile-specific request/response shapes
  Enums/                                     Status, provider, and method enumerations
  Configuration/                             Strongly-typed appsettings.json bindings
  Middleware/                                Exception handling, request logging, auth, idempotency
  Exceptions/                                Typed exceptions mapped to HTTP responses
  Extensions/                                DI/middleware wiring, Dapper enum handler, ModelState helper
  Database/                                  Startup schema/seed initializer
  Scripts/                                   SQL schema + seed data (idempotent, MySQL)
postman/                                     Postman collection
Dockerfile, docker-compose.yml               Container build & local orchestration
```

Layering follows `Controllers → Services → Repositories → MySQL`: controllers only bind
requests/responses, business logic lives in services, and only repositories talk to MySQL.

## Verification checklist

- `dotnet build` from `PaymentMock/` — should be clean.
- `docker compose up --build` — MySQL becomes healthy, the app connects and initializes the
  schema, `/health` returns healthy.
- Swagger UI loads at `/swagger`; calling any endpoint without `X-API-Key` returns `401`.
- `POST /api/v1/payments/mobile-money` with `"scenario": "HappyPath"`, then poll
  `GET /api/v1/transactions/{id}` — status should move `Created → Pending → Processing →
  Completed` over a few seconds, and a signed webhook should arrive at your `callbackUrl`.
- Repeat the same request with an `Idempotency-Key` header — the second call returns the original
  transaction, no duplicate row is created.
- Try `WrongPin`, `NeverCompletes`, and `WebhookDelayed` and confirm behavior matches the scenario
  table above.
- Switch `GatewayProfile` to `Pesapal`, restart, and confirm the response shape changes to
  Pesapal-style fields and `X-API-Key` no longer works — you must call `/api/v1/auth/token` first.
