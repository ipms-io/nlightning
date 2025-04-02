using NLightning.Bolts.BOLT11;
using NLightning.Bolts.Exceptions;
using SharpFuzz;

namespace InvoiceRoundTripFuzzer;

internal abstract class InvoiceRoundTripFuzzer
{
    private static void Main()
    {
        Fuzzer.OutOfProcess.Run(stream =>
        {
            //pwsh ../../fuzz.ps1 InvoiceRoundTripFuzzer.csproj -i Testcases
            try
            {
                using var reader = new StreamReader(stream);

                var invoiceData = reader.ReadToEnd();

                var invoice = Invoice.Decode(invoiceData);

                var encodedInvoice = invoice.Encode();

                if (!invoiceData.Equals(encodedInvoice, StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception("Invoice mismatch");
                }

            }
            catch (InvoiceSerializationException) { }
        });
    }
}