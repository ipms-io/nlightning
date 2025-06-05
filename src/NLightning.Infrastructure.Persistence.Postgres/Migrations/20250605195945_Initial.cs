using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NLightning.Infrastructure.Persistence.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "channels",
                columns: table => new
                {
                    channel_id = table.Column<byte[]>(type: "bytea", nullable: false),
                    funding_tx_id = table.Column<byte[]>(type: "bytea", nullable: false),
                    funding_output_index = table.Column<long>(type: "bigint", nullable: false),
                    funding_amount_satoshis = table.Column<long>(type: "bigint", nullable: false),
                    is_initiator = table.Column<bool>(type: "boolean", nullable: false),
                    remote_node_id = table.Column<byte[]>(type: "bytea", nullable: false),
                    local_next_htlc_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    remote_next_htlc_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    local_revocation_number = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    remote_revocation_number = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    last_sent_signature = table.Column<byte[]>(type: "bytea", nullable: true),
                    last_received_signature = table.Column<byte[]>(type: "bytea", nullable: true),
                    state = table.Column<byte>(type: "smallint", nullable: false),
                    version = table.Column<byte>(type: "smallint", nullable: false),
                    local_balance_satoshis = table.Column<decimal>(type: "numeric", nullable: false),
                    remote_balance_satoshis = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_channels", x => x.channel_id);
                });

            migrationBuilder.CreateTable(
                name: "channel_configs",
                columns: table => new
                {
                    channel_id = table.Column<byte[]>(type: "bytea", nullable: false),
                    minimum_depth = table.Column<long>(type: "bigint", nullable: false),
                    to_self_delay = table.Column<int>(type: "integer", nullable: false),
                    max_accepted_htlcs = table.Column<int>(type: "integer", nullable: false),
                    local_dust_limit_amount_sats = table.Column<long>(type: "bigint", nullable: false),
                    remote_dust_limit_amount_sats = table.Column<long>(type: "bigint", nullable: false),
                    htlc_minimum_msat = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    channel_reserve_amount_sats = table.Column<long>(type: "bigint", nullable: true),
                    max_htlc_amount_in_flight = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    fee_rate_per_kw_satoshis = table.Column<long>(type: "bigint", nullable: false),
                    option_anchor_outputs = table.Column<bool>(type: "boolean", nullable: false),
                    local_upfront_shutdown_script = table.Column<byte[]>(type: "bytea", nullable: true),
                    remote_upfront_shutdown_script = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_channel_configs", x => x.channel_id);
                    table.ForeignKey(
                        name: "fk_channel_configs_channels_channel_id",
                        column: x => x.channel_id,
                        principalTable: "channels",
                        principalColumn: "channel_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "channel_key_sets",
                columns: table => new
                {
                    channel_id = table.Column<byte[]>(type: "bytea", nullable: false),
                    is_local = table.Column<bool>(type: "boolean", nullable: false),
                    funding_pub_key = table.Column<byte[]>(type: "bytea", nullable: false),
                    revocation_basepoint = table.Column<byte[]>(type: "bytea", nullable: false),
                    payment_basepoint = table.Column<byte[]>(type: "bytea", nullable: false),
                    delayed_payment_basepoint = table.Column<byte[]>(type: "bytea", nullable: false),
                    htlc_basepoint = table.Column<byte[]>(type: "bytea", nullable: false),
                    current_per_commitment_index = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    current_per_commitment_point = table.Column<byte[]>(type: "bytea", nullable: false),
                    last_revealed_per_commitment_secret = table.Column<byte[]>(type: "bytea", nullable: true),
                    key_index = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_channel_key_sets", x => new { x.channel_id, x.is_local });
                    table.ForeignKey(
                        name: "fk_channel_key_sets_channels_channel_id",
                        column: x => x.channel_id,
                        principalTable: "channels",
                        principalColumn: "channel_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "htlcs",
                columns: table => new
                {
                    channel_id = table.Column<byte[]>(type: "bytea", nullable: false),
                    htlc_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    direction = table.Column<byte>(type: "smallint", nullable: false),
                    amount_msat = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    payment_hash = table.Column<byte[]>(type: "bytea", nullable: false),
                    payment_preimage = table.Column<byte[]>(type: "bytea", nullable: true),
                    cltv_expiry = table.Column<long>(type: "bigint", nullable: false),
                    state = table.Column<byte>(type: "smallint", nullable: false),
                    obscured_commitment_number = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    add_message_bytes = table.Column<byte[]>(type: "bytea", nullable: false),
                    signature = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_htlcs", x => new { x.channel_id, x.htlc_id, x.direction });
                    table.ForeignKey(
                        name: "fk_htlcs_channels_channel_id",
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
                name: "channel_configs");

            migrationBuilder.DropTable(
                name: "channel_key_sets");

            migrationBuilder.DropTable(
                name: "htlcs");

            migrationBuilder.DropTable(
                name: "channels");
        }
    }
}