namespace NLightning.Domain.Bitcoin.ValueObjects;

public readonly record struct BitcoinKeyPath
{
    public byte[] Value { get; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="BitcoinKeyPath"/> struct.
    /// </summary>
    /// <param name="value">The key path value.</param>
    public BitcoinKeyPath(byte[] value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value is null || value.Length == 0)
            throw new ArgumentException("Key path must not be null or empty.", nameof(value));
        
        Value = value;
    }

    public static implicit operator BitcoinKeyPath(byte[] bytes) => new(bytes);
    public static implicit operator byte[](BitcoinKeyPath script) => script.Value;
}