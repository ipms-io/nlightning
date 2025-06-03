using NLightning.Domain.Protocol.Messages;

namespace NLightning.Domain.Channels.ValueObjects;

using Crypto.ValueObjects;
using Enums;
using Money;
using Protocol.Messages.Interfaces;

public readonly record struct Htlc
{
    public ulong Id { get; }
    public LightningMoney Amount { get; }
    public Hash PaymentHash { get; }
    public Hash? PaymentPreimage { get; }
    public uint CltvExpiry { get; }
    public HtlcState State { get; }
    public HtlcDirection Direction { get; }
    public UpdateAddHtlcMessage AddMessage { get; }
    public ulong ObscuredCommitmentNumber { get; }
    public DerSignature? Signature { get; }

    public Htlc(LightningMoney amount, UpdateAddHtlcMessage addMessage, HtlcDirection direction, uint cltvExpiry,
                ulong id, ulong obscuredCommitmentNumber, Hash paymentHash, HtlcState state,
                Hash? paymentPreimage = null, DerSignature? signature = null)
    {
        Id = id;
        Amount = amount;
        PaymentHash = paymentHash;
        PaymentPreimage = paymentPreimage;
        CltvExpiry = cltvExpiry;
        State = state;
        Direction = direction;
        AddMessage = addMessage;
        ObscuredCommitmentNumber = obscuredCommitmentNumber;
        Signature = signature;
    }
}