using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Shared.Extensions;

public static class SqlServerDatabaseMigrationExtensions
{
    private const int StartupRetryCount = 10;
    private static readonly TimeSpan StartupRetryDelay = TimeSpan.FromSeconds(3);

    public static async Task MigrateSqlServerDbContextAsync<TContext>(this IServiceProvider services,
        CancellationToken cancellationToken = default)
        where TContext : DbContext
    {
        await using var scope = services.CreateAsyncScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<TContext>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();
        var connectionString = dbContext.Database.GetConnectionString() ?? throw new InvalidOperationException(
            $"Missing connection string for {typeof(TContext).Name}.");

        var builder = new SqlConnectionStringBuilder(connectionString);
        var databaseName = builder.InitialCatalog;
        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new InvalidOperationException($"Connection string for {typeof(TContext).Name} must specify a database name.");
        }

        await EnsureDatabaseExistsAsync(builder, databaseName, logger, cancellationToken);

        var pendingMigrations = (await dbContext.Database.GetPendingMigrationsAsync(cancellationToken)).ToArray();
        if (pendingMigrations.Length == 0)
        {
            logger.LogInformation("No pending migrations for {DbContext} on database {Database}",
                typeof(TContext).Name, databaseName);
            return;
        }

        logger.LogInformation("Applying {MigrationCount} migrations for {DbContext} on database {Database}: {Migrations}",
            pendingMigrations.Length,
            typeof(TContext).Name,
            databaseName,
            pendingMigrations);

        await dbContext.Database.MigrateAsync(cancellationToken);

        logger.LogInformation("Applied database migrations for {DbContext} on database {Database}",
            typeof(TContext).Name, databaseName);
    }

    private static async Task EnsureDatabaseExistsAsync(SqlConnectionStringBuilder builder, string databaseName,
        ILogger logger, CancellationToken cancellationToken)
    {
        var masterBuilder = new SqlConnectionStringBuilder(builder.ConnectionString)
        {
            InitialCatalog = "master"
        };

        for (var attempt = 1; attempt <= StartupRetryCount; attempt++)
        {
            try
            {
                await using var connection = new SqlConnection(masterBuilder.ConnectionString);
                await connection.OpenAsync(cancellationToken);

                await using var command = connection.CreateCommand();
                command.CommandText = """
BEGIN TRY
    IF DB_ID(@databaseName) IS NULL
    BEGIN
        DECLARE @sql nvarchar(max) = N'CREATE DATABASE ' + QUOTENAME(@databaseName);
        EXEC(@sql);
    END
END TRY
BEGIN CATCH
    IF ERROR_NUMBER() <> 1801
        THROW;
END CATCH;
""";
                command.Parameters.AddWithValue("@databaseName", databaseName);

                await command.ExecuteNonQueryAsync(cancellationToken);
                logger.LogInformation("Ensured SQL Server database {Database} exists before migration for startup orchestration",
                    databaseName);
                return;
            }
            catch (SqlException ex) when (attempt < StartupRetryCount && IsStartupRetryable(ex))
            {
                logger.LogWarning(ex,
                    "SQL Server was reachable but not ready for authenticated startup work on database {Database}. Retrying in {DelaySeconds} seconds ({Attempt}/{MaxAttempts})",
                    databaseName,
                    StartupRetryDelay.TotalSeconds,
                    attempt,
                    StartupRetryCount);

                await Task.Delay(StartupRetryDelay, cancellationToken);
            }
        }

        throw new InvalidOperationException($"SQL Server database {databaseName} could not be prepared for startup migrations.");
    }

    private static bool IsStartupRetryable(SqlException exception)
    {
        return exception.Number is 18456 or 4060 or 233 or -2 or 53;
    }
}
