﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NLightning.Infrastructure.Persistence.Contexts;

#nullable disable

namespace NLightning.Infrastructure.Persistence.Sqlite.Migrations
{
    [DbContext(typeof(NLightningDbContext))]
    partial class NLightningDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.12");

            modelBuilder.Entity("NLightning.Infrastructure.Persistence.Entities.Bitcoin.BlockchainStateEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("LastProcessedAt")
                        .HasColumnType("TEXT");

                    b.Property<byte[]>("LastProcessedBlockHash")
                        .IsRequired()
                        .HasColumnType("BLOB");

                    b.Property<uint>("LastProcessedHeight")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("BlockchainStates");
                });

            modelBuilder.Entity("NLightning.Infrastructure.Persistence.Entities.Bitcoin.WatchedTransactionEntity", b =>
                {
                    b.Property<byte[]>("TransactionId")
                        .HasColumnType("BLOB");

                    b.Property<byte[]>("ChannelId")
                        .IsRequired()
                        .HasColumnType("BLOB");

                    b.Property<DateTime?>("CompletedAt")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<uint?>("FirstSeenAtHeight")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("RequiredDepth")
                        .HasColumnType("INTEGER");

                    b.Property<ushort?>("TransactionIndex")
                        .HasColumnType("INTEGER");

                    b.HasKey("TransactionId");

                    b.HasIndex("ChannelId");

                    b.ToTable("WatchedTransactions");
                });

            modelBuilder.Entity("NLightning.Infrastructure.Persistence.Entities.Channel.ChannelConfigEntity", b =>
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

            modelBuilder.Entity("NLightning.Infrastructure.Persistence.Entities.Channel.ChannelEntity", b =>
                {
                    b.Property<byte[]>("ChannelId")
                        .HasColumnType("BLOB");

                    b.Property<long>("FundingAmountSatoshis")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("FundingCreatedAtBlockHeight")
                        .HasColumnType("INTEGER");

                    b.Property<ushort>("FundingOutputIndex")
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

                    b.Property<byte[]>("PeerEntityNodeId")
                        .HasColumnType("BLOB");

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

                    b.HasIndex("PeerEntityNodeId");

                    b.ToTable("Channels");
                });

            modelBuilder.Entity("NLightning.Infrastructure.Persistence.Entities.Channel.ChannelKeySetEntity", b =>
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

            modelBuilder.Entity("NLightning.Infrastructure.Persistence.Entities.Channel.HtlcEntity", b =>
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

            modelBuilder.Entity("NLightning.Infrastructure.Persistence.Entities.Node.PeerEntity", b =>
                {
                    b.Property<byte[]>("NodeId")
                        .HasColumnType("BLOB");

                    b.Property<string>("Host")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("LastSeenAt")
                        .HasColumnType("TEXT");

                    b.Property<uint>("Port")
                        .HasColumnType("INTEGER");

                    b.HasKey("NodeId");

                    b.ToTable("Peers");
                });

            modelBuilder.Entity("NLightning.Infrastructure.Persistence.Entities.Bitcoin.WatchedTransactionEntity", b =>
                {
                    b.HasOne("NLightning.Infrastructure.Persistence.Entities.Channel.ChannelEntity", null)
                        .WithMany("WatchedTransactions")
                        .HasForeignKey("ChannelId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("NLightning.Infrastructure.Persistence.Entities.Channel.ChannelConfigEntity", b =>
                {
                    b.HasOne("NLightning.Infrastructure.Persistence.Entities.Channel.ChannelEntity", null)
                        .WithOne("Config")
                        .HasForeignKey("NLightning.Infrastructure.Persistence.Entities.Channel.ChannelConfigEntity", "ChannelId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("NLightning.Infrastructure.Persistence.Entities.Channel.ChannelEntity", b =>
                {
                    b.HasOne("NLightning.Infrastructure.Persistence.Entities.Node.PeerEntity", null)
                        .WithMany("Channels")
                        .HasForeignKey("PeerEntityNodeId");
                });

            modelBuilder.Entity("NLightning.Infrastructure.Persistence.Entities.Channel.ChannelKeySetEntity", b =>
                {
                    b.HasOne("NLightning.Infrastructure.Persistence.Entities.Channel.ChannelEntity", null)
                        .WithMany("KeySets")
                        .HasForeignKey("ChannelId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("NLightning.Infrastructure.Persistence.Entities.Channel.HtlcEntity", b =>
                {
                    b.HasOne("NLightning.Infrastructure.Persistence.Entities.Channel.ChannelEntity", null)
                        .WithMany("Htlcs")
                        .HasForeignKey("ChannelId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("NLightning.Infrastructure.Persistence.Entities.Channel.ChannelEntity", b =>
                {
                    b.Navigation("Config");

                    b.Navigation("Htlcs");

                    b.Navigation("KeySets");

                    b.Navigation("WatchedTransactions");
                });

            modelBuilder.Entity("NLightning.Infrastructure.Persistence.Entities.Node.PeerEntity", b =>
                {
                    b.Navigation("Channels");
                });
#pragma warning restore 612, 618
        }
    }
}
