using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NLightning.Infrastructure.Persistence.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class AddBlockchaisStateAndWatchedTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BlockchainStates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    LastProcessedHeight = table.Column<uint>(type: "INTEGER", nullable: false),
                    LastProcessedBlockHash = table.Column<byte[]>(type: "BLOB", nullable: false),
                    LastProcessedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlockchainStates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WatchedTransactions",
                columns: table => new
                {
                    ChannelId = table.Column<byte[]>(type: "BLOB", nullable: false),
                    TransactionId = table.Column<byte[]>(type: "BLOB", nullable: false),
                    RequiredDepth = table.Column<uint>(type: "INTEGER", nullable: false),
                    FirstSeenAtHeight = table.Column<uint>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WatchedTransactions", x => new { x.ChannelId, x.TransactionId });
                    table.ForeignKey(
                        name: "FK_WatchedTransactions_Channels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "Channels",
                        principalColumn: "ChannelId",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlockchainStates");

            migrationBuilder.DropTable(
                name: "WatchedTransactions");
        }
    }
}