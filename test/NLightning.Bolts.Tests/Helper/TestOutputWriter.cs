using System.Text;
using Xunit.Abstractions;

namespace NLightning.Bolts.Tests.Helper;

public class TestOutputWriter : TextWriter
{
    private readonly ITestOutputHelper _output;

    public TestOutputWriter(ITestOutputHelper output)
    {
        _output = output;
    }

    public override Encoding Encoding => Encoding.UTF8;

    public override void Write(char[] buffer, int index, int count)
    {
        _output.WriteLine(new string(buffer, index, count));
    }
}