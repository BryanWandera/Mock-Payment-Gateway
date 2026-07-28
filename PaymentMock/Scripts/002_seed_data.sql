USE paymentmock;

INSERT INTO gateway_account (Id, Currency, CurrentBalance, AvailableBalance, TotalTransactions, TotalPayouts, UpdatedAt)
VALUES
    ('acc-kes-001', 'KES', 500000.00, 450000.00, 0, 0, UTC_TIMESTAMP()),
    ('acc-ugx-001', 'UGX', 10000000.00, 9500000.00, 0, 0, UTC_TIMESTAMP()),
    ('acc-tzs-001', 'TZS', 8000000.00, 7500000.00, 0, 0, UTC_TIMESTAMP()),
    ('acc-usd-001', 'USD', 50000.00, 48000.00, 0, 0, UTC_TIMESTAMP())
ON DUPLICATE KEY UPDATE UpdatedAt = UTC_TIMESTAMP();
