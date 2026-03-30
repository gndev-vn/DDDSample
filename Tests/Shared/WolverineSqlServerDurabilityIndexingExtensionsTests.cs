using Shared.Hosting;

namespace DDDSample.Tests.Hosting;

public sealed class WolverineSqlServerDurabilityIndexingExtensionsTests
{
    [Fact]
    public void BuildIndexDefinitions_CoversDurabilityBatchPredicates()
    {
        var definitions = WolverineSqlServerDurabilityIndexingExtensions.BuildIndexDefinitions();

        Assert.Contains(definitions, x =>
            x.TableName == "wolverine_incoming_envelopes" &&
            x.IndexName == "IX_wolverine_incoming_status_owner_received" &&
            x.Columns.SequenceEqual(["status", "owner_id", "received_at"]));

        Assert.Contains(definitions, x =>
            x.TableName == "wolverine_incoming_envelopes" &&
            x.IndexName == "IX_wolverine_incoming_status_keep_until" &&
            x.Columns.SequenceEqual(["status", "keep_until"]));

        Assert.Contains(definitions, x =>
            x.TableName == "wolverine_outgoing_envelopes" &&
            x.IndexName == "IX_wolverine_outgoing_owner_destination" &&
            x.Columns.SequenceEqual(["owner_id", "destination"]));

        Assert.Contains(definitions, x =>
            x.TableName == "wolverine_dead_letters" &&
            x.IndexName == "IX_wolverine_dead_letters_replayable" &&
            x.Columns.SequenceEqual(["replayable"]));

        Assert.Contains(definitions, x =>
            x.TableName == "wolverine_node_records" &&
            x.IndexName == "IX_wolverine_node_records_timestamp" &&
            x.Columns.SequenceEqual(["timestamp"]));

        Assert.Contains(definitions, x =>
            x.TableName == "wolverine_nodes" &&
            x.IndexName == "IX_wolverine_nodes_node_number" &&
            x.Columns.SequenceEqual(["node_number"]));
    }

    [Fact]
    public void BuildEnsureIndexesSql_UsesIdempotentCreateStatements()
    {
        var sql = WolverineSqlServerDurabilityIndexingExtensions.BuildEnsureIndexesSql("wolverine");

        Assert.Contains("IF OBJECT_ID(N'[wolverine].[wolverine_incoming_envelopes]', N'U') IS NOT NULL", sql, StringComparison.Ordinal);
        Assert.Contains("CREATE INDEX [IX_wolverine_incoming_status_owner_received]", sql, StringComparison.Ordinal);
        Assert.Contains("CREATE INDEX [IX_wolverine_outgoing_owner_destination]", sql, StringComparison.Ordinal);
        Assert.Contains("CREATE INDEX [IX_wolverine_dead_letters_replayable]", sql, StringComparison.Ordinal);
        Assert.Contains("CREATE INDEX [IX_wolverine_node_records_timestamp]", sql, StringComparison.Ordinal);
        Assert.Contains("CREATE INDEX [IX_wolverine_nodes_node_number]", sql, StringComparison.Ordinal);
    }
}
