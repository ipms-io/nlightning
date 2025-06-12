using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NLightning.Infrastructure.Persistence.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddBlockchaisStateAndWatchedTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "funding_output_index",
                table: "channels",
                type: "integer",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.CreateTable(
                name: "blockchain_states",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_processed_height = table.Column<long>(type: "bigint", nullable: false),
                    last_processed_block_hash = table.Column<byte[]>(type: "bytea", nullable: false),
                    last_processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_blockchain_states", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "watched_transactions",
                columns: table => new
                {
                    channel_id = table.Column<byte[]>(type: "bytea", nullable: false),
                    transaction_id = table.Column<byte[]>(type: "bytea", nullable: false),
                    required_depth = table.Column<long>(type: "bigint", nullable: false),
                    first_seen_at_height = table.Column<long>(type: "bigint", nullable: true),
                    transaction_index = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_watched_transactions", x => new { x.channel_id, x.transaction_id });
                    table.ForeignKey(
                        name: "fk_watched_transactions_channels_channel_id",
                        column: x => x.channel_id,
                        principalTable: "channels",
                        principalColumn: "channel_id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "blockchain_states");

            migrationBuilder.DropTable(
                name: "watched_transactions");

            migrationBuilder.AlterColumn<long>(
                name: "funding_output_index",
                table: "channels",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
