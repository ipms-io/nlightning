using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace NLightning.Infrastructure.Persistence.ValueConverters;

using Domain.Crypto.ValueObjects;

/// <summary>
/// EF Core value converter for CompactPubKey value object
/// </summary>
public class CompactPubKeyConverter()
    : ValueConverter<CompactPubKey, byte[]>(compactPubKey => compactPubKey, bytes => new CompactPubKey(bytes));