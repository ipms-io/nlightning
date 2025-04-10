using System.Security.Cryptography;
using System.Text;
using NLightning.Bolts.BOLT8.Constants;
using NLightning.Bolts.BOLT8.States;
using NLightning.Bolts.Tests.BOLT8.Mock;
using NLightning.Bolts.Tests.BOLT8.Utils;
using SharpFuzz;

namespace ActTwoFuzzer;

internal abstract class ActTwoFuzzer
{
    private static void Main()
    {
        Fuzzer.OutOfProcess.Run(stream =>
        {
            //pwsh ../../fuzz.ps1 ActTwoFuzzer.csproj -i Testcases
            try
            {
                using var memory = new MemoryStream();
                stream.CopyTo(memory);
                var data = memory.ToArray().AsSpan();

                var initiator = new HandshakeState(
                    true,
                    InitiatorValidKeysUtil.LocalStaticPrivateKey,
                    InitiatorValidKeysUtil.RemoteStaticPublicKey,
                    new FakeFixedKeyDh(InitiatorValidKeysUtil.EphemeralPrivateKey)
                );

                var buffer = new byte[ProtocolConstants.MAX_MESSAGE_LENGTH];

                initiator.WriteMessage(Encoding.ASCII.GetBytes(string.Empty), buffer);

                initiator.ReadMessage(data, buffer);
            }
            catch (ObjectDisposedException) { }
            catch (InvalidOperationException) { }
            catch (ArgumentException) { }
            catch (CryptographicException) { }
            catch (FormatException) { } //Crash found: System.FormatException: Invalid public key at NBitcoin.PubKey..ctor(ReadOnlySpan`1 bytes)

        });
    }
}