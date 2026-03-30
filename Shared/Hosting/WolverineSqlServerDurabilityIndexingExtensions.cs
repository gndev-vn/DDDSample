using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Shared.Hosting;

public static class WolverineSqlServerDurabilityIndexingExtensions
{
    private const int MaxAttempts = 10;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(3);
    private static readonly string[] RequiredTables =
    [
        "wolverine_incoming_envelopes",
        "wolverine_outgoing_envelopes",
        "wolverine_dead_letters",
        "wolverine_node_records",
        "wolverine_nodes"
    ];

    internal static IReadOnlyList<WolverineSqlServerIndexDefinition> BuildIndexDefinitions() =>
    [
        new("wolverine_incoming_envelopes", "IX_wolverine_incoming_status_owner_received", ["status", "owner_id", "received_at"]),
        new("wolverine_incoming_envelopes", "IX_wolverine_incoming_status_keep_until", ["status", "keep_until"]),
        new("wolverine_incoming_envelopes", "IX_wolverine_incoming_owner_id", ["owner_id"]),
        new("wolverine_outgoing_envelopes", "IX_wolverine_outgoing_owner_destination", ["owner_id", "destination"]),
        new("wolverine_dead_letters", "IX_wolverine_dead_letters_replayable", ["replayable"]),
        new("wolverine_node_records", "IX_wolverine_node_records_timestamp", ["timestamp"]),
        new("wolverine_nodes", "IX_wolverine_nodes_node_number", ["node_number"])
    ];

    public static async Task EnsureWolverineSqlServerDurabilityIndexesAsync(string connectionString, ILogger logger,
        string schema = "wolverine", CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ArgumentException.ThrowIfNullOrWhiteSpace(schema);

        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            try
            {
                await using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync(cancellationToken);

                if (!await RequiredTablesExistAsync(connection, schema, cancellationToken))
                {
                    if (attempt == MaxAttempts)
                    {
                        throw new InvalidOperationException(
                            $"Wolverine durability tables were not available in schema '{schema}' after startup.");
                    }

                    logger.LogInformation(
                        "Wolverine durability tables are not ready in schema {Schema}. Retrying index provisioning in {DelaySeconds} seconds ({Attempt}/{MaxAttempts})",
                        schema,
                        RetryDelay.TotalSeconds,
                        attempt,
                        MaxAttempts);

                    await Task.Delay(RetryDelay, cancellationToken);
                    continue;
                }

                await using var command = connection.CreateCommand();
                command.CommandTimeout = 60;
                command.CommandText = BuildEnsureIndexesSql(schema);
                await command.ExecuteNonQueryAsync(cancellationToken);

                logger.LogInformation("Ensured Wolverine SQL Server durability indexes in schema {Schema}", schema);
                return;
            }
            catch (SqlException ex) when (attempt < MaxAttempts && IsRetryable(ex))
            {
                logger.LogWarning(ex,
                    "Failed to provision Wolverine SQL Server durability indexes in schema {Schema}. Retrying in {DelaySeconds} seconds ({Attempt}/{MaxAttempts})",
                    schema,
                    RetryDelay.TotalSeconds,
                    attempt,
                    MaxAttempts);

                await Task.Delay(RetryDelay, cancellationToken);
            }
        }
    }

    internal static string BuildEnsureIndexesSql(string schema)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(schema);

        return string.Join(Environment.NewLine + Environment.NewLine,
            BuildIndexDefinitions().Select(definition => BuildCreateIndexStatement(schema, definition)));
    }

    private static string BuildCreateIndexStatement(string schema, WolverineSqlServerIndexDefinition definition)
    {
        var qualifiedTableName = $"[{EscapeIdentifier(schema)}].[{EscapeIdentifier(definition.TableName)}]";
        var columnList = string.Join(", ", definition.Columns.Select(column => $"[{EscapeIdentifier(column)}]"));

        return $$"""
IF OBJECT_ID(N'{{qualifiedTableName}}', N'U') IS NOT NULL
    AND NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE name = N'{{EscapeLiteral(definition.IndexName)}}'
          AND object_id = OBJECT_ID(N'{{qualifiedTableName}}', N'U'))
BEGIN
    CREATE INDEX [{{EscapeIdentifier(definition.IndexName)}}]
        ON {{qualifiedTableName}} ({{columnList}});
END;
""";
    }

    private static async Task<bool> RequiredTablesExistAsync(SqlConnection connection, string schema,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
SELECT COUNT(*)
FROM sys.tables t
JOIN sys.schemas s ON s.schema_id = t.schema_id
WHERE s.name = @schema
  AND t.name IN (@incoming, @outgoing, @deadLetters, @nodeRecords, @nodes);
""";
        command.Parameters.AddWithValue("@schema", schema);
        command.Parameters.AddWithValue("@incoming", RequiredTables[0]);
        command.Parameters.AddWithValue("@outgoing", RequiredTables[1]);
        command.Parameters.AddWithValue("@deadLetters", RequiredTables[2]);
        command.Parameters.AddWithValue("@nodeRecords", RequiredTables[3]);
        command.Parameters.AddWithValue("@nodes", RequiredTables[4]);

        var count = (int)(await command.ExecuteScalarAsync(cancellationToken) ?? 0);
        return count == RequiredTables.Length;
    }

    private static bool IsRetryable(SqlException exception) => exception.Number is -2 or 53 or 208 or 4060 or 18456 or 1205;

    private static string EscapeIdentifier(string identifier) => identifier.Replace("]", "]]", StringComparison.Ordinal);

    private static string EscapeLiteral(string value) => value.Replace("'", "''", StringComparison.Ordinal);
}

internal sealed record WolverineSqlServerIndexDefinition(string TableName, string IndexName, IReadOnlyList<string> Columns);
