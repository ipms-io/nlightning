using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NLightning.Infrastructure.Persistence.Sqlite.Migrations
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
                    ChannelId = table.Column<byte[]>(type: "BLOB", nullable: false),
                    FundingTxId = table.Column<byte[]>(type: "BLOB", nullable: false),
                    FundingOutputIndex = table.Column<uint>(type: "INTEGER", nullable: false),
                    FundingAmountSatoshis = table.Column<long>(type: "INTEGER", nullable: false),
                    IsInitiator = table.Column<bool>(type: "INTEGER", nullable: false),
                    RemoteNodeId = table.Column<byte[]>(type: "BLOB", nullable: false),
                    LocalNextHtlcId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    RemoteNextHtlcId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    LocalRevocationNumber = table.Column<ulong>(type: "INTEGER", nullable: false),
                    RemoteRevocationNumber = table.Column<ulong>(type: "INTEGER", nullable: false),
                    LastSentSignature = table.Column<byte[]>(type: "BLOB", nullable: true),
                    LastReceivedSignature = table.Column<byte[]>(type: "BLOB", nullable: true),
                    State = table.Column<byte>(type: "INTEGER", nullable: false),
                    Version = table.Column<byte>(type: "INTEGER", nullable: false),
                    LocalBalanceSatoshis = table.Column<decimal>(type: "TEXT", nullable: false),
                    RemoteBalanceSatoshis = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Channels", x => x.ChannelId);
                });

            migrationBuilder.CreateTable(
                name: "ChannelConfigs",
                columns: table => new
                {
                    ChannelId = table.Column<byte[]>(type: "BLOB", nullable: false),
                    MinimumDepth = table.Column<uint>(type: "INTEGER", nullable: false),
                    ToSelfDelay = table.Column<ushort>(type: "INTEGER", nullable: false),
                    MaxAcceptedHtlcs = table.Column<ushort>(type: "INTEGER", nullable: false),
                    LocalDustLimitAmountSats = table.Column<long>(type: "INTEGER", nullable: false),
                    RemoteDustLimitAmountSats = table.Column<long>(type: "INTEGER", nullable: false),
                    HtlcMinimumMsat = table.Column<ulong>(type: "INTEGER", nullable: false),
                    ChannelReserveAmountSats = table.Column<long>(type: "INTEGER", nullable: true),
                    MaxHtlcAmountInFlight = table.Column<ulong>(type: "INTEGER", nullable: false),
                    FeeRatePerKwSatoshis = table.Column<long>(type: "INTEGER", nullable: false),
                    OptionAnchorOutputs = table.Column<bool>(type: "INTEGER", nullable: false),
                    LocalUpfrontShutdownScript = table.Column<byte[]>(type: "BLOB", nullable: true),
                    RemoteUpfrontShutdownScript = table.Column<byte[]>(type: "BLOB", nullable: true)
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
                    ChannelId = table.Column<byte[]>(type: "BLOB", nullable: false),
                    IsLocal = table.Column<bool>(type: "INTEGER", nullable: false),
                    FundingPubKey = table.Column<byte[]>(type: "BLOB", nullable: false),
                    RevocationBasepoint = table.Column<byte[]>(type: "BLOB", nullable: false),
                    PaymentBasepoint = table.Column<byte[]>(type: "BLOB", nullable: false),
                    DelayedPaymentBasepoint = table.Column<byte[]>(type: "BLOB", nullable: false),
                    HtlcBasepoint = table.Column<byte[]>(type: "BLOB", nullable: false),
                    CurrentPerCommitmentIndex = table.Column<ulong>(type: "INTEGER", nullable: false),
                    CurrentPerCommitmentPoint = table.Column<byte[]>(type: "BLOB", nullable: false),
                    LastRevealedPerCommitmentSecret = table.Column<byte[]>(type: "BLOB", nullable: true),
                    KeyIndex = table.Column<uint>(type: "INTEGER", nullable: false)
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
                    ChannelId = table.Column<byte[]>(type: "BLOB", nullable: false),
                    HtlcId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    Direction = table.Column<byte>(type: "INTEGER", nullable: false),
                    AmountMsat = table.Column<ulong>(type: "INTEGER", nullable: false),
                    PaymentHash = table.Column<byte[]>(type: "BLOB", nullable: false),
                    PaymentPreimage = table.Column<byte[]>(type: "BLOB", nullable: true),
                    CltvExpiry = table.Column<uint>(type: "INTEGER", nullable: false),
                    State = table.Column<byte>(type: "INTEGER", nullable: false),
                    ObscuredCommitmentNumber = table.Column<ulong>(type: "INTEGER", nullable: false),
                    AddMessageBytes = table.Column<byte[]>(type: "BLOB", nullable: false),
                    Signature = table.Column<byte[]>(type: "BLOB", nullable: true)
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