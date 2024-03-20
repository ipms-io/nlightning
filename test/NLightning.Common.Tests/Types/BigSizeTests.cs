namespace NLightning.Common.Tests.Types;

using Common.Types;

using static Utils.TestUtils;

public partial class BigSizeTests
{
    [Fact]
    public void Given_VectorInputs_When_DeserializeBigSize_Then_ResultIsKnown()
    {
        // Arrange
        var testVectors = ReadTestVectors("Vectors/BigSize.txt").Where(x => x.Error == null);

        foreach (var testVector in testVectors)
        {
            // Arrange
            using var memoryStream = new MemoryStream(testVector.Bytes);
            using var reader = new BinaryReader(memoryStream);

            // Act
            var bigSizeValue = BigSize.Deserialize(reader);

            // Assert
            Assert.Equal(testVector.Value, bigSizeValue.Value);
        }
    }

    [Fact]
    public void Given_VectorInputs_When_DeserializeBigSize_Then_ErrorIsThrown()
    {
        // Arrange
        var testVectors = ReadTestVectors("Vectors/BigSize.txt").Where(x => x.Error != null);

        foreach (var testVector in testVectors)
        {
            // Arrange
            using var memoryStream = new MemoryStream(testVector.Bytes);
            using var reader = new BinaryReader(memoryStream);

            // Act
            void Deserialize() => BigSize.Deserialize(reader);

            // Assert
            var exception = Assert.ThrowsAny<Exception>(Deserialize);
        }
    }

    [Fact]
    public void Given_VectorInputs_When_SerializeBigSize_Then_ResultIsKnown()
    {
        // Arrange
        var testVectors = ReadTestVectors("Vectors/BigSize.txt").Where(x => x.Error == null);

        foreach (var testVector in testVectors)
        {
            // Arrange
            using var memoryStream = new MemoryStream();
            using var writer = new BinaryWriter(memoryStream);

            // Act
            new BigSize(testVector.Value).Serialize(writer);

            // Assert
            Assert.Equal(testVector.Bytes, memoryStream.ToArray());
        }
    }

    [Theory]
    [InlineData(ulong.MaxValue, typeof(long))]
    [InlineData(ulong.MaxValue, typeof(uint))]
    [InlineData(ulong.MaxValue, typeof(int))]
    [InlineData(ulong.MaxValue, typeof(ushort))]
    [InlineData(ulong.MaxValue, typeof(short))]
    [InlineData(ulong.MaxValue, typeof(byte))]
    [InlineData(long.MaxValue, typeof(uint))]
    [InlineData(long.MaxValue, typeof(int))]
    [InlineData(long.MaxValue, typeof(ushort))]
    [InlineData(long.MaxValue, typeof(short))]
    [InlineData(long.MaxValue, typeof(byte))]
    [InlineData(uint.MaxValue, typeof(int))]
    [InlineData(uint.MaxValue, typeof(ushort))]
    [InlineData(uint.MaxValue, typeof(short))]
    [InlineData(uint.MaxValue, typeof(byte))]
    [InlineData(int.MaxValue, typeof(ushort))]
    [InlineData(int.MaxValue, typeof(short))]
    [InlineData(int.MaxValue, typeof(byte))]
    [InlineData(ushort.MaxValue, typeof(short))]
    [InlineData(ushort.MaxValue, typeof(byte))]
    [InlineData(short.MaxValue, typeof(byte))]
    public void Given_ImplicitConversion_When_CastToSmallerSize_Then_ThrowsException(ulong value, Type type)
    {
        // Arrange
        var bigSize = new BigSize(value);

        // Assert
        switch (type)
        {
            case Type longType when longType == typeof(long):
                var longException = Assert.Throws<OverflowException>(() => (long)bigSize);
                Assert.Equal($"Cannot convert {bigSize.Value} to long because it's too large.", longException.Message);
                break;
            case Type uintType when uintType == typeof(uint):
                var uintException = Assert.Throws<OverflowException>(() => (uint)bigSize);
                Assert.Equal($"Cannot convert {bigSize.Value} to uint because it's too large.", uintException.Message);
                break;
            case Type intType when intType == typeof(int):
                var intException = Assert.Throws<OverflowException>(() => (int)bigSize);
                Assert.Equal($"Cannot convert {bigSize.Value} to int because it's too large.", intException.Message);
                break;
            case Type ushortType when ushortType == typeof(ushort):
                var ushortException = Assert.Throws<OverflowException>(() => (ushort)bigSize);
                Assert.Equal($"Cannot convert {bigSize.Value} to ushort because it's too large.", ushortException.Message);
                break;
            case Type shortType when shortType == typeof(short):
                var shortException = Assert.Throws<OverflowException>(() => (short)bigSize);
                Assert.Equal($"Cannot convert {bigSize.Value} to short because it's too large.", shortException.Message);
                break;
            case Type byteType when byteType == typeof(byte):
                var byteException = Assert.Throws<OverflowException>(() => (byte)bigSize);
                Assert.Equal($"Cannot convert {bigSize.Value} to byte because it's too large.", byteException.Message);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    [Theory]
    [InlineData(byte.MaxValue, typeof(byte))]
    [InlineData(byte.MaxValue, typeof(short))]
    [InlineData(byte.MaxValue, typeof(ushort))]
    [InlineData(byte.MaxValue, typeof(int))]
    [InlineData(byte.MaxValue, typeof(uint))]
    [InlineData(byte.MaxValue, typeof(long))]
    [InlineData(byte.MaxValue, typeof(ulong))]
    [InlineData(short.MaxValue, typeof(short))]
    [InlineData(short.MaxValue, typeof(ushort))]
    [InlineData(short.MaxValue, typeof(int))]
    [InlineData(short.MaxValue, typeof(uint))]
    [InlineData(short.MaxValue, typeof(long))]
    [InlineData(short.MaxValue, typeof(ulong))]
    [InlineData(ushort.MaxValue, typeof(ushort))]
    [InlineData(ushort.MaxValue, typeof(int))]
    [InlineData(ushort.MaxValue, typeof(uint))]
    [InlineData(ushort.MaxValue, typeof(long))]
    [InlineData(ushort.MaxValue, typeof(ulong))]
    [InlineData(int.MaxValue, typeof(int))]
    [InlineData(int.MaxValue, typeof(uint))]
    [InlineData(int.MaxValue, typeof(long))]
    [InlineData(int.MaxValue, typeof(ulong))]
    [InlineData(uint.MaxValue, typeof(uint))]
    [InlineData(uint.MaxValue, typeof(long))]
    [InlineData(uint.MaxValue, typeof(ulong))]
    [InlineData(long.MaxValue, typeof(long))]
    [InlineData(long.MaxValue, typeof(ulong))]
    [InlineData(ulong.MaxValue, typeof(ulong))]
    public void Given_ImplicitConversion_When_CastToBiggerSize_Then_CastIsValid(ulong value, Type type)
    {
        // Arrange
        var bigSize = new BigSize(value);

        // Assert
        switch (type)
        {
            case Type ulongType when ulongType == typeof(ulong):
                Assert.Equal(value, bigSize);
                break;
            case Type longType when longType == typeof(long):
                Assert.Equal((long)value, bigSize);
                break;
            case Type uintType when uintType == typeof(uint):
                Assert.Equal((uint)value, bigSize);
                break;
            case Type intType when intType == typeof(int):
                Assert.Equal((int)value, bigSize);
                break;
            case Type ushortType when ushortType == typeof(ushort):
                Assert.Equal((ushort)value, bigSize);
                break;
            case Type shortType when shortType == typeof(short):
                Assert.Equal((short)value, bigSize);
                break;
            case Type byteType when byteType == typeof(byte):
                Assert.Equal((byte)value, bigSize);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    [Theory]
    [InlineData(0, 0, true)]
    [InlineData(0, 1, false)]
    [InlineData(1, 0, false)]
    [InlineData(1, 1, true)]
    public void Given_BigSize_When_CompareEqual_Then_ResultIsKnown(ulong value1, ulong value2, bool expected)
    {
        // Arrange
        var bigSize1 = new BigSize(value1);
        var bigSize2 = new BigSize(value2);

        // Act
        var result = bigSize1 == bigSize2;

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(0, 0, false)]
    [InlineData(0, 1, true)]
    [InlineData(1, 0, true)]
    [InlineData(1, 1, false)]
    public void Given_BigSize_When_CompareNotEqual_Then_ResultIsKnown(ulong value1, ulong value2, bool expected)
    {
        // Arrange
        var bigSize1 = new BigSize(value1);
        var bigSize2 = new BigSize(value2);

        // Act
        var result = bigSize1 != bigSize2;

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(0, 0, false)]
    [InlineData(0, 1, true)]
    [InlineData(1, 0, false)]
    [InlineData(1, 1, false)]
    public void Given_BigSize_When_CompareLessThan_Then_ResultIsKnown(ulong value1, ulong value2, bool expected)
    {
        // Arrange
        var bigSize1 = new BigSize(value1);
        var bigSize2 = new BigSize(value2);

        // Act
        var result = bigSize1 < bigSize2;

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(0, 0, true)]
    [InlineData(0, 1, true)]
    [InlineData(1, 0, false)]
    [InlineData(1, 1, true)]
    public void Given_BigSize_When_CompareLessThanOrEqual_Then_ResultIsKnown(ulong value1, ulong value2, bool expected)
    {
        // Arrange
        var bigSize1 = new BigSize(value1);
        var bigSize2 = new BigSize(value2);

        // Act
        var result = bigSize1 <= bigSize2;

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(0, 0, false)]
    [InlineData(0, 1, false)]
    [InlineData(1, 0, true)]
    [InlineData(1, 1, false)]
    public void Given_BigSize_When_CompareGreaterThan_Then_ResultIsKnown(ulong value1, ulong value2, bool expected)
    {
        // Arrange
        var bigSize1 = new BigSize(value1);
        var bigSize2 = new BigSize(value2);

        // Act
        var result = bigSize1 > bigSize2;

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(0, 0, true)]
    [InlineData(0, 1, false)]
    [InlineData(1, 0, true)]
    [InlineData(1, 1, true)]
    public void Given_BigSize_When_CompareGreaterThanOrEqual_Then_ResultIsKnown(ulong value1, ulong value2, bool expected)
    {
        // Arrange
        var bigSize1 = new BigSize(value1);
        var bigSize2 = new BigSize(value2);

        // Act
        var result = bigSize1 >= bigSize2;

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Given_BigSize_When_CompareEqualWithObject_Then_ResultIsKnown()
    {
        // Arrange
        var bigSize = new BigSize(0);
        var obj = new object();

        // Act
        var result = bigSize.Equals(obj);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Given_BigSize_When_CompareEqualWithNull_Then_ResultIsKnown()
    {
        // Arrange
        var bigSize = new BigSize(0);

        // Act
        var result = bigSize.Equals(null);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Given_BigSize_When_CompareEqualWithBigSize_Then_ResultIsKnown()
    {
        // Arrange
        var bigSize = new BigSize(0);
        var bigSize2 = new BigSize(0);

        // Act
        var result = bigSize.Equals(bigSize2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Given_BigSize_When_GetHashCode_Then_ResultIsKnown()
    {
        // Arrange
        var bigSize = new BigSize(0);

        // Act
        var result = bigSize.GetHashCode();

        // Assert
        Assert.Equal(0.GetHashCode(), result);
    }

    private class TestVector
    {
        public ulong Value { get; set; }
        public byte[] Bytes { get; set; } = [];
        public string? Error { get; set; }
    }

    private static List<TestVector> ReadTestVectors(string filePath)
    {
        var testVectors = new List<TestVector>();
        TestVector? currentVector = null;

        foreach (var line in File.ReadLines(filePath))
        {
            if (line.StartsWith("Value: "))
            {
                currentVector = new TestVector
                {
                    Value = ulong.Parse(line[7..])
                };
            }
            else if (line.StartsWith("Bytes: "))
            {
                if (currentVector == null)
                {
                    throw new InvalidOperationException("Bytes line without Value line");
                }

                currentVector.Bytes = GetBytes(line[7..]);
            }
            else if (line.StartsWith("Error: "))
            {
                if (currentVector == null)
                {
                    throw new InvalidOperationException("Bytes line without Value line");
                }

                if (currentVector.Bytes == null)
                {
                    throw new InvalidOperationException("Error line without Bytes line");
                }

                var error = line[7..];
                if (!string.IsNullOrEmpty(error))
                {
                    currentVector.Error = error;
                }

                testVectors.Add(currentVector);
            }
        }

        return testVectors;
    }
}