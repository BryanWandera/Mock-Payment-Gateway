using MySql.Data.MySqlClient;

namespace PaymentMock.Database;

public static class DbInitializer
{
    private const int MaxConnectionAttempts = 15;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(3);

    public static async Task InitializeAsync(string connectionString, ILogger logger)
    {
        var builder = new MySqlConnectionStringBuilder(connectionString) { Database = string.Empty };
        var serverConnectionString = builder.ConnectionString;

        await using var connection = await OpenWithRetryAsync(serverConnectionString, logger);

        var scriptsDirectory = Path.Combine(AppContext.BaseDirectory, "Scripts");
        if (!Directory.Exists(scriptsDirectory))
        {
            logger.LogWarning("Scripts directory not found at {Path}; skipping schema initialization", scriptsDirectory);
            return;
        }

        foreach (var scriptFile in Directory.GetFiles(scriptsDirectory, "*.sql").OrderBy(f => f))
        {
            logger.LogInformation("Applying database script {Script}", Path.GetFileName(scriptFile));
            var sql = await File.ReadAllTextAsync(scriptFile);
            var script = new MySqlScript(connection, sql);
            await script.ExecuteAsync();
        }

        logger.LogInformation("Database initialization complete");
    }

    private static async Task<MySqlConnection> OpenWithRetryAsync(string serverConnectionString, ILogger logger)
    {
        for (var attempt = 1; attempt <= MaxConnectionAttempts; attempt++)
        {
            try
            {
                var connection = new MySqlConnection(serverConnectionString);
                await connection.OpenAsync();
                return connection;
            }
            catch (Exception ex) when (attempt < MaxConnectionAttempts)
            {
                logger.LogWarning("MySQL not yet available (attempt {Attempt}/{MaxAttempts}): {Message}", attempt, MaxConnectionAttempts, ex.Message);
                await Task.Delay(RetryDelay);
            }
        }

        throw new InvalidOperationException("Could not connect to MySQL after multiple attempts");
    }
}
