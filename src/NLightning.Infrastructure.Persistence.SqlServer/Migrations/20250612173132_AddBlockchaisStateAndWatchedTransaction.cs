using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NLightning.Infrastructure.Persistence.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddBlockchaisStateAndWatchedTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "FundingOutputIndex",
                table: "Channels",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<byte[]>(
                name: "PeerEntityNodeId",
                table: "Channels",
                type: "varbinary(33)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BlockchainStates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastProcessedHeight = table.Column<long>(type: "bigint", nullable: false),
                    LastProcessedBlockHash = table.Column<byte[]>(type: "varbinary(32)", nullable: false),
                    LastProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlockchainStates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Peers",
                columns: table => new
                {
                    NodeId = table.Column<byte[]>(type: "varbinary(33)", nullable: false),
                    Host = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Port = table.Column<long>(type: "bigint", nullable: false),
                    LastSeenAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Peers", x => x.NodeId);
                });

            migrationBuilder.CreateTable(
                name: "WatchedTransactions",
                columns: table => new
                {
                    TransactionId = table.Column<byte[]>(type: "varbinary(32)", nullable: false),
                    ChannelId = table.Column<byte[]>(type: "varbinary(32)", nullable: false),
                    RequiredDepth = table.Column<long>(type: "bigint", nullable: false),
                    FirstSeenAtHeight = table.Column<long>(type: "bigint", nullable: true),
                    TransactionIndex = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WatchedTransactions", x => x.TransactionId);
                    table.ForeignKey(
                        name: "FK_WatchedTransactions_Channels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "Channels",
                        principalColumn: "ChannelId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Channels_PeerEntityNodeId",
                table: "Channels",
                column: "PeerEntityNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_WatchedTransactions_ChannelId",
                table: "WatchedTransactions",
                column: "ChannelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Channels_Peers_PeerEntityNodeId",
                table: "Channels",
                column: "PeerEntityNodeId",
                principalTable: "Peers",
                principalColumn: "NodeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Channels_Peers_PeerEntityNodeId",
                table: "Channels");

            migrationBuilder.DropTable(
                name: "BlockchainStates");

            migrationBuilder.DropTable(
                name: "Peers");

            migrationBuilder.DropTable(
                name: "WatchedTransactions");

            migrationBuilder.DropIndex(
                name: "IX_Channels_PeerEntityNodeId",
                table: "Channels");

            migrationBuilder.DropColumn(
                name: "PeerEntityNodeId",
                table: "Channels");

            migrationBuilder.AlterColumn<long>(
                name: "FundingOutputIndex",
                table: "Channels",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}