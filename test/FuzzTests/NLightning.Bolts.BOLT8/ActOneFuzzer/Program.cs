using System.Security.Cryptography;
using NLightning.Bolts.BOLT8.Constants;
using NLightning.Bolts.BOLT8.States;
using NLightning.Bolts.Tests.BOLT8.Mock;
using NLightning.Bolts.Tests.BOLT8.Utils;
using SharpFuzz;

namespace ActOneFuzzer;

internal abstract class ActOneFuzzer
{
    private static void Main()
    {
        Fuzzer.OutOfProcess.Run(stream =>
        {
            //pwsh ../../fuzz.ps1 ActOneFuzzer.csproj -i Testcases
            try
            {
                using var memory = new MemoryStream();
                stream.CopyTo(memory);
                var data = memory.ToArray().AsSpan();
            
                var responder = new HandshakeState(
                    false,
                    ResponderValidKeysUtil.LocalStaticPrivateKey,
                    ResponderValidKeysUtil.LocalStaticPublicKey,
                    new FakeFixedKeyDh(ResponderValidKeysUtil.EphemeralPrivateKey)
                );
            
                var buffer = new byte[ProtocolConstants.MAX_MESSAGE_LENGTH];
                responder.ReadMessage(data, buffer);
            }
            catch (ObjectDisposedException) { }
            catch (InvalidOperationException) { }
            catch (ArgumentException) { }
            catch (CryptographicException) { }
            catch (FormatException) { } //Crash found: System.FormatException: Invalid public key at NBitcoin.PubKey..ctor(ReadOnlySpan`1 bytes)
        });
    }
}