namespace NLightning.Blazor.Tests.Infrastructure.Crypto.Hashes;

using TestCollections;

[Collection(BlazorTestCollection.Name)]
public class Sha256Tests : BlazorTestBase
{
    [Fact]
    public async Task Given_NistVectorInputs_When_DataIsHashedInApp_Then_ResultIsKnownForAll()
    {
        // Arrange
        Assert.NotNull(Page);
        ClearOutput();
        await Page.GotoAsync(RootUri, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 10000
        });

        var input = Page.GetByTestId("sha256MsgHex");
        var button = Page.GetByTestId("sha256Hash");
        var resultElement = Page.GetByTestId("sha256HashResult");
        var errorElement = Page.GetByTestId("sha256HashError");

        Assert.NotNull(input);
        Assert.NotNull(button);
        Assert.NotNull(resultElement);

        var vectors = ReadTestVectors(Path.Combine(AppContext.BaseDirectory, "SHA256LongMsg.rsp"));
        Assert.NotEmpty(vectors);

        // Act & Assert: iterate all vectors
        foreach (var v in vectors)
        {
            // Fill the message hex
            await input.FillAsync(v.MsgHex!);

            // Click to compute
            await button.ClickAsync();

            // Wait for the result element to be visible (first time) and then to match the expected value
            await Page.WaitForSelectorAsync("[data-testid='sha256HashResult']",
                                            new PageWaitForSelectorOptions
                                            {
                                                State = WaitForSelectorState.Visible,
                                                Timeout = 10000
                                            });

            var expectedHex = Convert.ToHexString(v.Md!);

            // Wait until the content equals the expected hex
            await Page.WaitForFunctionAsync(
                @"(expected) => document.querySelector('[data-testid=\'sha256HashResult\']').textContent === expected",
                expectedHex);

            var hex = await resultElement.TextContentAsync();
            Assert.Equal(expectedHex, hex);

            // Also ensure no error is displayed
            var errVisible = await errorElement.IsVisibleAsync();
            if (!errVisible)
                continue;

            var err = await errorElement.TextContentAsync();
            Assert.True(string.IsNullOrWhiteSpace(err), $"Unexpected error displayed: {err}");
        }
    }

    private sealed class TestVector(int len)
    {
        public int Len { get; } = len;
        public string? MsgHex { get; set; }
        public byte[]? Md { get; set; }
    }

    private static List<TestVector> ReadTestVectors(string filePath)
    {
        var testVectors = new List<TestVector>();
        TestVector? currentVector = null;

        foreach (var line in File.ReadLines(filePath))
        {
            if (line.StartsWith("Len = "))
            {
                currentVector = new TestVector(int.Parse(line[6..]));
            }
            else if (line.StartsWith("Msg = "))
            {
                if (currentVector == null)
                    throw new InvalidOperationException("Msg line without Len line");

                var hex = line[6..];
                currentVector.MsgHex = hex;

                // Optional consistency check
                if (hex.Length != currentVector.Len / 4)
                    throw new InvalidOperationException("Msg length does not match Len");
            }
            else if (line.StartsWith("MD = "))
            {
                if (currentVector == null)
                    throw new InvalidOperationException("MD line without Len line");

                if (currentVector.MsgHex == null)
                    throw new InvalidOperationException("MD line without Msg line");

                currentVector.Md = Convert.FromHexString(line[5..]);
                testVectors.Add(currentVector);
            }
        }

        return testVectors;
    }
}