using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NLightning.Infrastructure.Persistence.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Channels",
                columns: table => new
                {
                    ChannelId = table.Column<byte[]>(type: "varbinary(32)", nullable: false),
                    FundingCreatedAtBlockHeight = table.Column<long>(type: "bigint", nullable: false),
                    FundingTxId = table.Column<byte[]>(type: "varbinary(32)", nullable: false),
                    FundingOutputIndex = table.Column<long>(type: "bigint", nullable: false),
                    FundingAmountSatoshis = table.Column<long>(type: "bigint", nullable: false),
                    IsInitiator = table.Column<bool>(type: "bit", nullable: false),
                    RemoteNodeId = table.Column<byte[]>(type: "varbinary(32)", nullable: false),
                    LocalNextHtlcId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    RemoteNextHtlcId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    LocalRevocationNumber = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    RemoteRevocationNumber = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    LastSentSignature = table.Column<byte[]>(type: "varbinary(64)", nullable: true),
                    LastReceivedSignature = table.Column<byte[]>(type: "varbinary(64)", nullable: true),
                    State = table.Column<byte>(type: "tinyint", nullable: false),
                    Version = table.Column<byte>(type: "tinyint", nullable: false),
                    LocalBalanceSatoshis = table.Column<long>(type: "bigint", nullable: false),
                    RemoteBalanceSatoshis = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Channels", x => x.ChannelId);
                });

            migrationBuilder.CreateTable(
                name: "ChannelConfigs",
                columns: table => new
                {
                    ChannelId = table.Column<byte[]>(type: "varbinary(32)", nullable: false),
                    MinimumDepth = table.Column<long>(type: "bigint", nullable: false),
                    ToSelfDelay = table.Column<int>(type: "int", nullable: false),
                    MaxAcceptedHtlcs = table.Column<int>(type: "int", nullable: false),
                    LocalDustLimitAmountSats = table.Column<long>(type: "bigint", nullable: false),
                    RemoteDustLimitAmountSats = table.Column<long>(type: "bigint", nullable: false),
                    HtlcMinimumMsat = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    ChannelReserveAmountSats = table.Column<long>(type: "bigint", nullable: true),
                    MaxHtlcAmountInFlight = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    FeeRatePerKwSatoshis = table.Column<long>(type: "bigint", nullable: false),
                    OptionAnchorOutputs = table.Column<bool>(type: "bit", nullable: false),
                    LocalUpfrontShutdownScript = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    RemoteUpfrontShutdownScript = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    UseScidAlias = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelConfigs", x => x.ChannelId);
                    table.ForeignKey(
                        name: "FK_ChannelConfigs_Channels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "Channels",
                        principalColumn: "ChannelId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChannelKeySets",
                columns: table => new
                {
                    ChannelId = table.Column<byte[]>(type: "varbinary(32)", nullable: false),
                    IsLocal = table.Column<bool>(type: "bit", nullable: false),
                    FundingPubKey = table.Column<byte[]>(type: "varbinary(33)", nullable: false),
                    RevocationBasepoint = table.Column<byte[]>(type: "varbinary(33)", nullable: false),
                    PaymentBasepoint = table.Column<byte[]>(type: "varbinary(33)", nullable: false),
                    DelayedPaymentBasepoint = table.Column<byte[]>(type: "varbinary(33)", nullable: false),
                    HtlcBasepoint = table.Column<byte[]>(type: "varbinary(33)", nullable: false),
                    CurrentPerCommitmentIndex = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    CurrentPerCommitmentPoint = table.Column<byte[]>(type: "varbinary(33)", nullable: false),
                    LastRevealedPerCommitmentSecret = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    KeyIndex = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelKeySets", x => new { x.ChannelId, x.IsLocal });
                    table.ForeignKey(
                        name: "FK_ChannelKeySets_Channels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "Channels",
                        principalColumn: "ChannelId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Htlcs",
                columns: table => new
                {
                    ChannelId = table.Column<byte[]>(type: "varbinary(32)", nullable: false),
                    HtlcId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    Direction = table.Column<byte>(type: "tinyint", nullable: false),
                    AmountMsat = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    PaymentHash = table.Column<byte[]>(type: "varbinary(32)", nullable: false),
                    PaymentPreimage = table.Column<byte[]>(type: "varbinary(32)", nullable: true),
                    CltvExpiry = table.Column<long>(type: "bigint", nullable: false),
                    State = table.Column<byte>(type: "tinyint", nullable: false),
                    ObscuredCommitmentNumber = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    AddMessageBytes = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    Signature = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Htlcs", x => new { x.ChannelId, x.HtlcId, x.Direction });
                    table.ForeignKey(
                        name: "FK_Htlcs_Channels_ChannelId",
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
                name: "ChannelConfigs");

            migrationBuilder.DropTable(
                name: "ChannelKeySets");

            migrationBuilder.DropTable(
                name: "Htlcs");

            migrationBuilder.DropTable(
                name: "Channels");
        }
    }
}