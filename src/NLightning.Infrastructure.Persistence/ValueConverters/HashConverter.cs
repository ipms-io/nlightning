using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace NLightning.Infrastructure.Persistence.ValueConverters;

using Domain.Crypto.ValueObjects;

/// <summary>
/// EF Core value converter for TxIdId value object
/// </summary>
public class HashConverter() : ValueConverter<Hash, byte[]>(hash => hash, bytes => new Hash(bytes));