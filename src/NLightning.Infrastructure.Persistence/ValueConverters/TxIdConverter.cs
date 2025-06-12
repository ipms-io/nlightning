using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace NLightning.Infrastructure.Persistence.ValueConverters;

using Domain.Bitcoin.ValueObjects;

/// <summary>
/// EF Core value converter for TxIdId value object
/// </summary>
public class TxIdConverter() : ValueConverter<TxId, byte[]>(txId => txId, bytes => new TxId(bytes));