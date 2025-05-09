using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace NLightning.Application.NLTG.Signers;

using Domain.Channels;
using Domain.Protocol.Managers;
using Domain.Protocol.Services;
using Domain.Protocol.Signers;

public class LocalLightningSigner : ILightningSigner
{
    private readonly ISecureKeyManager _secureKeyManager;
    private readonly IKeyDerivationService _keyDerivationService;
    private readonly ConcurrentDictionary<string, ChannelKeyData> _channelKeyData = new();
    private readonly ILogger<LocalLightningSigner> _logger;

    public LocalLightningSigner(IKeyDerivationService keyDerivationService, ILogger<LocalLightningSigner> logger,
                                ISecureKeyManager secureKeyManager)
    {
        _keyDerivationService = keyDerivationService;
        _logger = logger;
        _secureKeyManager = secureKeyManager;
        
        // TODO: Load channel key data from database
    }
    
    public void Dispose()
    {
        // TODO release managed resources here
    }
}