USE paymentmock;

-- Adds the columns needed for the hosted checkout flow (Card / Bank Transfer redirect page).
-- DbInitializer re-runs every script in this folder on every application startup, so these
-- ALTER TABLEs must be idempotent — MySQL has no portable "ADD COLUMN IF NOT EXISTS" prior to
-- 8.0.29, so we guard with an information_schema check + prepared statement instead.

SET @dbname = DATABASE();
SET @tablename = 'transactions';

SET @columnname = 'CheckoutToken';
SET @preparedStatement = (SELECT IF(
    (SELECT COUNT(*) FROM information_schema.COLUMNS
     WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tablename AND COLUMN_NAME = @columnname) > 0,
    'SELECT 1',
    'ALTER TABLE transactions ADD COLUMN CheckoutToken VARCHAR(64) NULL'
));
PREPARE alterIfNotExists FROM @preparedStatement;
EXECUTE alterIfNotExists;
DEALLOCATE PREPARE alterIfNotExists;

SET @columnname = 'CheckoutCompletedAt';
SET @preparedStatement = (SELECT IF(
    (SELECT COUNT(*) FROM information_schema.COLUMNS
     WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tablename AND COLUMN_NAME = @columnname) > 0,
    'SELECT 1',
    'ALTER TABLE transactions ADD COLUMN CheckoutCompletedAt DATETIME NULL'
));
PREPARE alterIfNotExists FROM @preparedStatement;
EXECUTE alterIfNotExists;
DEALLOCATE PREPARE alterIfNotExists;

SET @indexname = 'idx_transactions_checkout_token';
SET @preparedStatement = (SELECT IF(
    (SELECT COUNT(*) FROM information_schema.STATISTICS
     WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tablename AND INDEX_NAME = @indexname) > 0,
    'SELECT 1',
    'CREATE INDEX idx_transactions_checkout_token ON transactions (CheckoutToken)'
));
PREPARE alterIfNotExists FROM @preparedStatement;
EXECUTE alterIfNotExists;
DEALLOCATE PREPARE alterIfNotExists;
