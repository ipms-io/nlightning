using System;
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
            migrationBuilder.AlterColumn<byte>(
                name: "State",
                table: "Htlcs",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<byte[]>(
                name: "Signature",
                table: "Htlcs",
                type: "BLOB",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<byte[]>(
                name: "PaymentPreimage",
                table: "Htlcs",
                type: "BLOB",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(32)",
                oldNullable: true);

            migrationBuilder.AlterColumn<byte[]>(
                name: "PaymentHash",
                table: "Htlcs",
                type: "BLOB",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(32)");

            migrationBuilder.AlterColumn<ulong>(
                name: "ObscuredCommitmentNumber",
                table: "Htlcs",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(20,0)");

            migrationBuilder.AlterColumn<uint>(
                name: "CltvExpiry",
                table: "Htlcs",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "AmountMsat",
                table: "Htlcs",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(20,0)");

            migrationBuilder.AlterColumn<byte[]>(
                name: "AddMessageBytes",
                table: "Htlcs",
                type: "BLOB",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(max)");

            migrationBuilder.AlterColumn<byte>(
                name: "Direction",
                table: "Htlcs",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<ulong>(
                name: "HtlcId",
                table: "Htlcs",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(20,0)");

            migrationBuilder.AlterColumn<byte[]>(
                name: "ChannelId",
                table: "Htlcs",
                type: "BLOB",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(32)");

            migrationBuilder.AlterColumn<byte>(
                name: "Version",
                table: "Channels",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<byte>(
                name: "State",
                table: "Channels",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<ulong>(
                name: "RemoteRevocationNumber",
                table: "Channels",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(20,0)");

            migrationBuilder.AlterColumn<byte[]>(
                name: "RemoteNodeId",
                table: "Channels",
                type: "BLOB",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(32)");

            migrationBuilder.AlterColumn<ulong>(
                name: "RemoteNextHtlcId",
                table: "Channels",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(20,0)");

            migrationBuilder.AlterColumn<decimal>(
                name: "RemoteBalanceSatoshis",
                table: "Channels",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "LocalRevocationNumber",
                table: "Channels",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(20,0)");

            migrationBuilder.AlterColumn<ulong>(
                name: "LocalNextHtlcId",
                table: "Channels",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(20,0)");

            migrationBuilder.AlterColumn<decimal>(
                name: "LocalBalanceSatoshis",
                table: "Channels",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<byte[]>(
                name: "LastSentSignature",
                table: "Channels",
                type: "BLOB",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(64)",
                oldNullable: true);

            migrationBuilder.AlterColumn<byte[]>(
                name: "LastReceivedSignature",
                table: "Channels",
                type: "BLOB",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(64)",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsInitiator",
                table: "Channels",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<byte[]>(
                name: "FundingTxId",
                table: "Channels",
                type: "BLOB",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(32)");

            migrationBuilder.AlterColumn<ushort>(
                name: "FundingOutputIndex",
                table: "Channels",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<uint>(
                name: "FundingCreatedAtBlockHeight",
                table: "Channels",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<long>(
                name: "FundingAmountSatoshis",
                table: "Channels",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<byte[]>(
                name: "ChannelId",
                table: "Channels",
                type: "BLOB",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(32)");

            migrationBuilder.AlterColumn<byte[]>(
                name: "RevocationBasepoint",
                table: "ChannelKeySets",
                type: "BLOB",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(33)");

            migrationBuilder.AlterColumn<byte[]>(
                name: "PaymentBasepoint",
                table: "ChannelKeySets",
                type: "BLOB",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(33)");

            migrationBuilder.AlterColumn<byte[]>(
                name: "LastRevealedPerCommitmentSecret",
                table: "ChannelKeySets",
                type: "BLOB",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<uint>(
                name: "KeyIndex",
                table: "ChannelKeySets",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<byte[]>(
                name: "HtlcBasepoint",
                table: "ChannelKeySets",
                type: "BLOB",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(33)");

            migrationBuilder.AlterColumn<byte[]>(
                name: "FundingPubKey",
                table: "ChannelKeySets",
                type: "BLOB",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(33)");

            migrationBuilder.AlterColumn<byte[]>(
                name: "DelayedPaymentBasepoint",
                table: "ChannelKeySets",
                type: "BLOB",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(33)");

            migrationBuilder.AlterColumn<byte[]>(
                name: "CurrentPerCommitmentPoint",
                table: "ChannelKeySets",
                type: "BLOB",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(33)");

            migrationBuilder.AlterColumn<ulong>(
                name: "CurrentPerCommitmentIndex",
                table: "ChannelKeySets",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(20,0)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsLocal",
                table: "ChannelKeySets",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<byte[]>(
                name: "ChannelId",
                table: "ChannelKeySets",
                type: "BLOB",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(32)");

            migrationBuilder.AlterColumn<byte>(
                name: "UseScidAlias",
                table: "ChannelConfigs",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<ushort>(
                name: "ToSelfDelay",
                table: "ChannelConfigs",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<byte[]>(
                name: "RemoteUpfrontShutdownScript",
                table: "ChannelConfigs",
                type: "BLOB",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "RemoteDustLimitAmountSats",
                table: "ChannelConfigs",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<bool>(
                name: "OptionAnchorOutputs",
                table: "ChannelConfigs",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<uint>(
                name: "MinimumDepth",
                table: "ChannelConfigs",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "MaxHtlcAmountInFlight",
                table: "ChannelConfigs",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(20,0)");

            migrationBuilder.AlterColumn<ushort>(
                name: "MaxAcceptedHtlcs",
                table: "ChannelConfigs",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<byte[]>(
                name: "LocalUpfrontShutdownScript",
                table: "ChannelConfigs",
                type: "BLOB",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "LocalDustLimitAmountSats",
                table: "ChannelConfigs",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<ulong>(
                name: "HtlcMinimumMsat",
                table: "ChannelConfigs",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "FeeRatePerKwSatoshis",
                table: "ChannelConfigs",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<long>(
                name: "ChannelReserveAmountSats",
                table: "ChannelConfigs",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<byte[]>(
                name: "ChannelId",
                table: "ChannelConfigs",
                type: "BLOB",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(32)");

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
                    TransactionIndex = table.Column<ushort>(type: "INTEGER", nullable: true),
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

            migrationBuilder.AlterColumn<byte>(
                name: "State",
                table: "Htlcs",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<byte[]>(
                name: "Signature",
                table: "Htlcs",
                type: "varbinary(max)",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "BLOB",
                oldNullable: true);

            migrationBuilder.AlterColumn<byte[]>(
                name: "PaymentPreimage",
                table: "Htlcs",
                type: "varbinary(32)",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "BLOB",
                oldNullable: true);

            migrationBuilder.AlterColumn<byte[]>(
                name: "PaymentHash",
                table: "Htlcs",
                type: "varbinary(32)",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "BLOB");

            migrationBuilder.AlterColumn<decimal>(
                name: "ObscuredCommitmentNumber",
                table: "Htlcs",
                type: "decimal(20,0)",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<long>(
                name: "CltvExpiry",
                table: "Htlcs",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(uint),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<decimal>(
                name: "AmountMsat",
                table: "Htlcs",
                type: "decimal(20,0)",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<byte[]>(
                name: "AddMessageBytes",
                table: "Htlcs",
                type: "varbinary(max)",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "BLOB");

            migrationBuilder.AlterColumn<byte>(
                name: "Direction",
                table: "Htlcs",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<decimal>(
                name: "HtlcId",
                table: "Htlcs",
                type: "decimal(20,0)",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<byte[]>(
                name: "ChannelId",
                table: "Htlcs",
                type: "varbinary(32)",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "BLOB");

            migrationBuilder.AlterColumn<byte>(
                name: "Version",
                table: "Channels",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<byte>(
                name: "State",
                table: "Channels",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<decimal>(
                name: "RemoteRevocationNumber",
                table: "Channels",
                type: "decimal(20,0)",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<byte[]>(
                name: "RemoteNodeId",
                table: "Channels",
                type: "varbinary(32)",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "BLOB");

            migrationBuilder.AlterColumn<decimal>(
                name: "RemoteNextHtlcId",
                table: "Channels",
                type: "decimal(20,0)",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<long>(
                name: "RemoteBalanceSatoshis",
                table: "Channels",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<decimal>(
                name: "LocalRevocationNumber",
                table: "Channels",
                type: "decimal(20,0)",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<decimal>(
                name: "LocalNextHtlcId",
                table: "Channels",
                type: "decimal(20,0)",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<long>(
                name: "LocalBalanceSatoshis",
                table: "Channels",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<byte[]>(
                name: "LastSentSignature",
                table: "Channels",
                type: "varbinary(64)",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "BLOB",
                oldNullable: true);

            migrationBuilder.AlterColumn<byte[]>(
                name: "LastReceivedSignature",
                table: "Channels",
                type: "varbinary(64)",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "BLOB",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsInitiator",
                table: "Channels",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<byte[]>(
                name: "FundingTxId",
                table: "Channels",
                type: "varbinary(32)",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "BLOB");

            migrationBuilder.AlterColumn<long>(
                name: "FundingOutputIndex",
                table: "Channels",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(ushort),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<long>(
                name: "FundingCreatedAtBlockHeight",
                table: "Channels",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(uint),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<long>(
                name: "FundingAmountSatoshis",
                table: "Channels",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<byte[]>(
                name: "ChannelId",
                table: "Channels",
                type: "varbinary(32)",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "BLOB");

            migrationBuilder.AlterColumn<byte[]>(
                name: "RevocationBasepoint",
                table: "ChannelKeySets",
                type: "varbinary(33)",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "BLOB");

            migrationBuilder.AlterColumn<byte[]>(
                name: "PaymentBasepoint",
                table: "ChannelKeySets",
                type: "varbinary(33)",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "BLOB");

            migrationBuilder.AlterColumn<byte[]>(
                name: "LastRevealedPerCommitmentSecret",
                table: "ChannelKeySets",
                type: "varbinary(max)",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "BLOB",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "KeyIndex",
                table: "ChannelKeySets",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(uint),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<byte[]>(
                name: "HtlcBasepoint",
                table: "ChannelKeySets",
                type: "varbinary(33)",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "BLOB");

            migrationBuilder.AlterColumn<byte[]>(
                name: "FundingPubKey",
                table: "ChannelKeySets",
                type: "varbinary(33)",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "BLOB");

            migrationBuilder.AlterColumn<byte[]>(
                name: "DelayedPaymentBasepoint",
                table: "ChannelKeySets",
                type: "varbinary(33)",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "BLOB");

            migrationBuilder.AlterColumn<byte[]>(
                name: "CurrentPerCommitmentPoint",
                table: "ChannelKeySets",
                type: "varbinary(33)",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "BLOB");

            migrationBuilder.AlterColumn<decimal>(
                name: "CurrentPerCommitmentIndex",
                table: "ChannelKeySets",
                type: "decimal(20,0)",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<bool>(
                name: "IsLocal",
                table: "ChannelKeySets",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<byte[]>(
                name: "ChannelId",
                table: "ChannelKeySets",
                type: "varbinary(32)",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "BLOB");

            migrationBuilder.AlterColumn<byte>(
                name: "UseScidAlias",
                table: "ChannelConfigs",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<int>(
                name: "ToSelfDelay",
                table: "ChannelConfigs",
                type: "int",
                nullable: false,
                oldClrType: typeof(ushort),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<byte[]>(
                name: "RemoteUpfrontShutdownScript",
                table: "ChannelConfigs",
                type: "varbinary(max)",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "BLOB",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "RemoteDustLimitAmountSats",
                table: "ChannelConfigs",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<bool>(
                name: "OptionAnchorOutputs",
                table: "ChannelConfigs",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<long>(
                name: "MinimumDepth",
                table: "ChannelConfigs",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(uint),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<decimal>(
                name: "MaxHtlcAmountInFlight",
                table: "ChannelConfigs",
                type: "decimal(20,0)",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<int>(
                name: "MaxAcceptedHtlcs",
                table: "ChannelConfigs",
                type: "int",
                nullable: false,
                oldClrType: typeof(ushort),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<byte[]>(
                name: "LocalUpfrontShutdownScript",
                table: "ChannelConfigs",
                type: "varbinary(max)",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "BLOB",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "LocalDustLimitAmountSats",
                table: "ChannelConfigs",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<decimal>(
                name: "HtlcMinimumMsat",
                table: "ChannelConfigs",
                type: "decimal(20,0)",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<long>(
                name: "FeeRatePerKwSatoshis",
                table: "ChannelConfigs",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<long>(
                name: "ChannelReserveAmountSats",
                table: "ChannelConfigs",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<byte[]>(
                name: "ChannelId",
                table: "ChannelConfigs",
                type: "varbinary(32)",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "BLOB");
        }
    }
}
