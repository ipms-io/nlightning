// ReSharper disable PropertyCanBeMadeInitOnly.Global
namespace NLightning.Infrastructure.Persistence.Entities;

public class HtlcEntity
{
    /// <summary>
    /// Represents the unique identifier for a channel associated with the HTLC entity.
    /// This property is used to establish a relationship between HTLCs and their respective channels.
    /// </summary>
    /// <remarks>This is part of the composite-key identifier</remarks>
    public required byte[] ChannelId { get; set; }

    /// <summary>
    /// Represents the unique identifier for a specific HTLC (Hashed Time-Locked Contract) instance.
    /// This property helps to distinguish and track individual HTLCs associated with a channel.
    /// </summary>
    /// <remarks>This is part of the composite-key identifier</remarks>
    public required ulong HtlcId { get; set; }

    /// <summary>
    /// Specifies the direction of the HTLC in relation to the channel, indicating whether it is incoming or outgoing.
    /// This property aids in identifying the flow of funds and the associated logic within the channel operations.
    /// </summary>
    /// <remarks>This is part of the composite-key identifier</remarks>
    public required byte Direction { get; set; }

    /// <summary>
    /// Represents the amount associated with the HTLC in milli-satoshis (msat).
    /// This property is used to define the precise value of the HTLC for transactions and operations
    /// within the Lightning Network.
    /// </summary>
    public required ulong AmountMsat { get; set; }

    /// <summary>
    /// Represents the hash identifier used for payment routing in Lightning Network.
    /// This property ensures that payments are routed securely without exposing the preimage.
    /// </summary>
    /// <remarks>
    /// The PaymentHash is a fundamental part of HTLC (Hashed Time Lock Contract) mechanics,
    /// allowing validation of payment claims through hash preimage revelation.
    /// </remarks>
    public required byte[] PaymentHash { get; set; } // uint256 as string

    /// <summary>
    /// Represents the raw preimage of a cryptographic hash used in the context of
    /// HTLC (Hashed Time Lock Contract) payments.
    /// This property is utilized to verify payment authenticity by reconstructing the hash associated with the HTLC.
    /// </summary>
    /// <remarks>This property may be null if the preimage is not available at the given
    /// state of the HTLC lifecycle.</remarks>
    public byte[]? PaymentPreimage { get; set; } // uint256 as string, nullable

    /// <summary>
    /// Represents the expiration time of the HTLC (Hashed Timelock Contract).
    /// This property indicates the deadline by which the HTLC must be resolved, or it becomes invalid.
    /// </summary>
    /// <remarks>This property is critical in ensuring the timely processing of payment channels to avoid stale or
    /// unresolved contracts.</remarks>
    public required uint CltvExpiry { get; set; } // Block height

    /// <summary>
    /// Represents the current state of the HTLC (Hashed Time-Locked Contract).
    /// This property captures the lifecycle stage of the HTLC, such as being offered,
    /// settled, failed, fulfilled, or revoked.
    /// </summary>
    /// <remarks>
    /// The state is stored as an enumeration of type <see cref="NLightning.Domain.Channels.Enums.HtlcState"/>.
    /// </remarks>
    public required byte State { get; set; }

    /// <summary>
    /// Represents the obscured commitment number associated with the HTLC entity.
    /// This property is used in creating a commitment transaction's unique identifier
    /// and ensures privacy and obfuscation in Lightning Network transactions.
    /// </summary>
    /// <remarks>
    /// The obscured commitment number is derived using the negotiated channel information
    /// and serves as a mechanism to mitigate channel state leakage.
    /// </remarks>
    public required ulong ObscuredCommitmentNumber { get; set; }

    /// <summary>
    /// Stores the serialized byte representation of the HTLC's "Add" message.
    /// This property is used for persisting and reconstructing the HTLC's initial state for
    /// communication purposes.
    /// </summary>
    /// <remarks>
    /// This property facilitates the serialization and deserialization of the UpdateAddHtlcMessage
    /// to ensure accurate data storage and retrieval in the repository layers.
    /// </remarks>
    public required byte[] AddMessageBytes { get; set; }

    /// <summary>
    /// Represents the cryptographic signature associated with the HTLC entity.
    /// This property is used to validate and ensure the authenticity of messages or transactions
    /// within the HTLC protocol, using the corresponding cryptographic keys.
    /// </summary>
    /// <remarks>This property is optional, as it may not be present in all scenarios.</remarks>
    public byte[]?  Signature { get; set; }

    // Default constructor for EF Core
    internal HtlcEntity()
    { }
}