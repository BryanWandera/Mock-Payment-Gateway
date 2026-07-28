CREATE DATABASE IF NOT EXISTS paymentmock;
USE paymentmock;

CREATE TABLE IF NOT EXISTS transactions (
    Id VARCHAR(36) PRIMARY KEY,
    GatewayTransactionId VARCHAR(64) NOT NULL,
    MerchantReference VARCHAR(100) NOT NULL,
    ExternalReference VARCHAR(100) NULL,
    ReceiptNumber VARCHAR(50) NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Currency VARCHAR(3) NOT NULL,
    Provider VARCHAR(20) NOT NULL,
    PaymentMethod VARCHAR(20) NOT NULL,
    Status VARCHAR(20) NOT NULL,
    FailureReason VARCHAR(500) NULL,
    Scenario VARCHAR(50) NULL,
    IdempotencyKey VARCHAR(100) NULL,
    CustomerName VARCHAR(200) NULL,
    CustomerEmail VARCHAR(200) NULL,
    PhoneNumber VARCHAR(20) NULL,
    CardLastFour VARCHAR(4) NULL,
    CallbackUrl VARCHAR(500) NULL,
    NotificationId VARCHAR(36) NULL,
    Description VARCHAR(500) NULL,
    CheckoutToken VARCHAR(64) NULL,
    CheckoutCompletedAt DATETIME NULL,
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME NOT NULL,
    CompletedAt DATETIME NULL,
    INDEX idx_transactions_gateway_id (GatewayTransactionId),
    INDEX idx_transactions_merchant_ref (MerchantReference),
    INDEX idx_transactions_status (Status),
    INDEX idx_transactions_created (CreatedAt),
    INDEX idx_transactions_idempotency (IdempotencyKey),
    INDEX idx_transactions_checkout_token (CheckoutToken)
);

CREATE TABLE IF NOT EXISTS transaction_state_history (
    Id VARCHAR(36) PRIMARY KEY,
    TransactionId VARCHAR(36) NOT NULL,
    FromStatus VARCHAR(20) NOT NULL,
    ToStatus VARCHAR(20) NOT NULL,
    TransitionedAt DATETIME NOT NULL,
    Notes VARCHAR(500) NULL,
    INDEX idx_tx_history_transaction (TransactionId)
);

CREATE TABLE IF NOT EXISTS payouts (
    Id VARCHAR(36) PRIMARY KEY,
    GatewayPayoutId VARCHAR(64) NOT NULL,
    MerchantReference VARCHAR(100) NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Currency VARCHAR(3) NOT NULL,
    PaymentMethod VARCHAR(20) NOT NULL,
    Provider VARCHAR(20) NOT NULL,
    Status VARCHAR(20) NOT NULL,
    FailureReason VARCHAR(500) NULL,
    Scenario VARCHAR(50) NULL,
    RecipientName VARCHAR(200) NULL,
    PhoneNumber VARCHAR(20) NULL,
    BankAccountNumber VARCHAR(50) NULL,
    BankName VARCHAR(100) NULL,
    BankCode VARCHAR(20) NULL,
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME NOT NULL,
    CompletedAt DATETIME NULL,
    INDEX idx_payouts_gateway_id (GatewayPayoutId),
    INDEX idx_payouts_merchant_ref (MerchantReference),
    INDEX idx_payouts_status (Status)
);

CREATE TABLE IF NOT EXISTS payout_state_history (
    Id VARCHAR(36) PRIMARY KEY,
    PayoutId VARCHAR(36) NOT NULL,
    FromStatus VARCHAR(20) NOT NULL,
    ToStatus VARCHAR(20) NOT NULL,
    TransitionedAt DATETIME NOT NULL,
    Notes VARCHAR(500) NULL,
    INDEX idx_payout_history_payout (PayoutId)
);

CREATE TABLE IF NOT EXISTS webhook_deliveries (
    Id VARCHAR(36) PRIMARY KEY,
    TransactionId VARCHAR(36) NULL,
    PayoutId VARCHAR(36) NULL,
    TargetUrl VARCHAR(500) NOT NULL,
    Payload TEXT NOT NULL,
    Signature VARCHAR(200) NULL,
    Status VARCHAR(20) NOT NULL,
    AttemptCount INT NOT NULL DEFAULT 0,
    HttpStatusCode INT NULL,
    ResponseBody TEXT NULL,
    ErrorMessage VARCHAR(500) NULL,
    CreatedAt DATETIME NOT NULL,
    DeliveredAt DATETIME NULL,
    ScheduledAt DATETIME NULL,
    INDEX idx_webhook_status (Status),
    INDEX idx_webhook_scheduled (ScheduledAt)
);

CREATE TABLE IF NOT EXISTS webhook_registrations (
    Id VARCHAR(36) PRIMARY KEY,
    Url VARCHAR(500) NOT NULL,
    NotificationType VARCHAR(10) NOT NULL DEFAULT 'POST',
    IpnId VARCHAR(36) NULL,
    IsActive TINYINT(1) NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL,
    INDEX idx_webhook_reg_ipn (IpnId)
);

CREATE TABLE IF NOT EXISTS idempotency_keys (
    Id VARCHAR(36) PRIMARY KEY,
    IdempotencyKey VARCHAR(100) NOT NULL,
    RequestPath VARCHAR(200) NOT NULL,
    RequestHash VARCHAR(64) NOT NULL,
    StatusCode INT NOT NULL,
    ResponseBody TEXT NOT NULL,
    CreatedAt DATETIME NOT NULL,
    ExpiresAt DATETIME NOT NULL,
    UNIQUE INDEX idx_idempotency_key (IdempotencyKey, RequestPath)
);

CREATE TABLE IF NOT EXISTS scenario_executions (
    Id VARCHAR(36) PRIMARY KEY,
    ScenarioName VARCHAR(50) NOT NULL,
    TransactionId VARCHAR(36) NULL,
    PayoutId VARCHAR(36) NULL,
    Details TEXT NULL,
    ExecutedAt DATETIME NOT NULL,
    INDEX idx_scenario_name (ScenarioName)
);

CREATE TABLE IF NOT EXISTS gateway_account (
    Id VARCHAR(36) PRIMARY KEY,
    Currency VARCHAR(3) NOT NULL,
    CurrentBalance DECIMAL(18,2) NOT NULL DEFAULT 0,
    AvailableBalance DECIMAL(18,2) NOT NULL DEFAULT 0,
    TotalTransactions INT NOT NULL DEFAULT 0,
    TotalPayouts INT NOT NULL DEFAULT 0,
    UpdatedAt DATETIME NOT NULL,
    UNIQUE INDEX idx_account_currency (Currency)
);

CREATE TABLE IF NOT EXISTS audit_logs (
    Id VARCHAR(36) PRIMARY KEY,
    LogType VARCHAR(50) NOT NULL,
    Message VARCHAR(500) NOT NULL,
    Details TEXT NULL,
    CorrelationId VARCHAR(36) NULL,
    CreatedAt DATETIME NOT NULL,
    INDEX idx_audit_type (LogType),
    INDEX idx_audit_created (CreatedAt)
);

CREATE TABLE IF NOT EXISTS config_history (
    Id VARCHAR(36) PRIMARY KEY,
    ConfigSection VARCHAR(100) NOT NULL,
    Snapshot TEXT NOT NULL,
    ChangedBy VARCHAR(100) NULL,
    CreatedAt DATETIME NOT NULL
);
