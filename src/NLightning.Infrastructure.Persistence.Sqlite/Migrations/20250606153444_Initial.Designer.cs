﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NLightning.Infrastructure.Persistence.Contexts;

#nullable disable

namespace NLightning.Infrastructure.Persistence.Sqlite.Migrations
{
    [DbContext(typeof(NLightningDbContext))]
    [Migration("20250606153444_Initial")]
    partial class Initial
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.12");

            modelBuilder.Entity("NLightning.Infrastructure.Persistence.Entities.ChannelConfigEntity", b =>
                {
                    b.Property<byte[]>("ChannelId")
                        .HasColumnType("BLOB");

                    b.Property<long?>("ChannelReserveAmountSats")
                        .HasColumnType("INTEGER");

                    b.Property<long>("FeeRatePerKwSatoshis")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("HtlcMinimumMsat")
                        .HasColumnType("INTEGER");

                    b.Property<long>("LocalDustLimitAmountSats")
                        .HasColumnType("INTEGER");

                    b.Property<byte[]>("LocalUpfrontShutdownScript")
                        .HasColumnType("BLOB");

                    b.Property<ushort>("MaxAcceptedHtlcs")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("MaxHtlcAmountInFlight")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("MinimumDepth")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("OptionAnchorOutputs")
                        .HasColumnType("INTEGER");

                    b.Property<long>("RemoteDustLimitAmountSats")
                        .HasColumnType("INTEGER");

                    b.Property<byte[]>("RemoteUpfrontShutdownScript")
                        .HasColumnType("BLOB");

                    b.Property<ushort>("ToSelfDelay")
                        .HasColumnType("INTEGER");

                    b.Property<byte>("UseScidAlias")
                        .HasColumnType("INTEGER");

                    b.HasKey("ChannelId");

                    b.ToTable("ChannelConfigs");
                });

            modelBuilder.Entity("NLightning.Infrastructure.Persistence.Entities.ChannelEntity", b =>
                {
                    b.Property<byte[]>("ChannelId")
                        .HasColumnType("BLOB");

                    b.Property<long>("FundingAmountSatoshis")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("FundingCreatedAtBlockHeight")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("FundingOutputIndex")
                        .HasColumnType("INTEGER");

                    b.Property<byte[]>("FundingTxId")
                        .IsRequired()
                        .HasColumnType("BLOB");

                    b.Property<bool>("IsInitiator")
                        .HasColumnType("INTEGER");

                    b.Property<byte[]>("LastReceivedSignature")
                        .HasColumnType("BLOB");

                    b.Property<byte[]>("LastSentSignature")
                        .HasColumnType("BLOB");

                    b.Property<decimal>("LocalBalanceSatoshis")
                        .HasColumnType("TEXT");

                    b.Property<ulong>("LocalNextHtlcId")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("LocalRevocationNumber")
                        .HasColumnType("INTEGER");

                    b.Property<decimal>("RemoteBalanceSatoshis")
                        .HasColumnType("TEXT");

                    b.Property<ulong>("RemoteNextHtlcId")
                        .HasColumnType("INTEGER");

                    b.Property<byte[]>("RemoteNodeId")
                        .IsRequired()
                        .HasColumnType("BLOB");

                    b.Property<ulong>("RemoteRevocationNumber")
                        .HasColumnType("INTEGER");

                    b.Property<byte>("State")
                        .HasColumnType("INTEGER");

                    b.Property<byte>("Version")
                        .HasColumnType("INTEGER");

                    b.HasKey("ChannelId");

                    b.ToTable("Channels");
                });

            modelBuilder.Entity("NLightning.Infrastructure.Persistence.Entities.ChannelKeySetEntity", b =>
                {
                    b.Property<byte[]>("ChannelId")
                        .HasColumnType("BLOB");

                    b.Property<bool>("IsLocal")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("CurrentPerCommitmentIndex")
                        .HasColumnType("INTEGER");

                    b.Property<byte[]>("CurrentPerCommitmentPoint")
                        .IsRequired()
                        .HasColumnType("BLOB");

                    b.Property<byte[]>("DelayedPaymentBasepoint")
                        .IsRequired()
                        .HasColumnType("BLOB");

                    b.Property<byte[]>("FundingPubKey")
                        .IsRequired()
                        .HasColumnType("BLOB");

                    b.Property<byte[]>("HtlcBasepoint")
                        .IsRequired()
                        .HasColumnType("BLOB");

                    b.Property<uint>("KeyIndex")
                        .HasColumnType("INTEGER");

                    b.Property<byte[]>("LastRevealedPerCommitmentSecret")
                        .HasColumnType("BLOB");

                    b.Property<byte[]>("PaymentBasepoint")
                        .IsRequired()
                        .HasColumnType("BLOB");

                    b.Property<byte[]>("RevocationBasepoint")
                        .IsRequired()
                        .HasColumnType("BLOB");

                    b.HasKey("ChannelId", "IsLocal");

                    b.ToTable("ChannelKeySets");
                });

            modelBuilder.Entity("NLightning.Infrastructure.Persistence.Entities.HtlcEntity", b =>
                {
                    b.Property<byte[]>("ChannelId")
                        .HasColumnType("BLOB");

                    b.Property<ulong>("HtlcId")
                        .HasColumnType("INTEGER");

                    b.Property<byte>("Direction")
                        .HasColumnType("INTEGER");

                    b.Property<byte[]>("AddMessageBytes")
                        .IsRequired()
                        .HasColumnType("BLOB");

                    b.Property<ulong>("AmountMsat")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("CltvExpiry")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("ObscuredCommitmentNumber")
                        .HasColumnType("INTEGER");

                    b.Property<byte[]>("PaymentHash")
                        .IsRequired()
                        .HasColumnType("BLOB");

                    b.Property<byte[]>("PaymentPreimage")
                        .HasColumnType("BLOB");

                    b.Property<byte[]>("Signature")
                        .HasColumnType("BLOB");

                    b.Property<byte>("State")
                        .HasColumnType("INTEGER");

                    b.HasKey("ChannelId", "HtlcId", "Direction");

                    b.ToTable("Htlcs");
                });

            modelBuilder.Entity("NLightning.Infrastructure.Persistence.Entities.ChannelConfigEntity", b =>
                {
                    b.HasOne("NLightning.Infrastructure.Persistence.Entities.ChannelEntity", null)
                        .WithOne("Config")
                        .HasForeignKey("NLightning.Infrastructure.Persistence.Entities.ChannelConfigEntity", "ChannelId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("NLightning.Infrastructure.Persistence.Entities.ChannelKeySetEntity", b =>
                {
                    b.HasOne("NLightning.Infrastructure.Persistence.Entities.ChannelEntity", null)
                        .WithMany("KeySets")
                        .HasForeignKey("ChannelId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("NLightning.Infrastructure.Persistence.Entities.HtlcEntity", b =>
                {
                    b.HasOne("NLightning.Infrastructure.Persistence.Entities.ChannelEntity", null)
                        .WithMany("Htlcs")
                        .HasForeignKey("ChannelId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("NLightning.Infrastructure.Persistence.Entities.ChannelEntity", b =>
                {
                    b.Navigation("Config");

                    b.Navigation("Htlcs");

                    b.Navigation("KeySets");
                });
#pragma warning restore 612, 618
        }
    }
}