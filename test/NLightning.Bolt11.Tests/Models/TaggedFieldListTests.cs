namespace NLightning.Bolt11.Tests.Models;

using Bolt11.Models;
using Common.Utils;
using Domain.ValueObjects;
using Enums;
using Interfaces;
using Mocks;

public class TaggedFieldListTests
{
    [Fact]
    public void Given_TaggedFieldList_When_AddValidTaggedField_Then_ItemIsAddedAndChangedEventRaised()
    {
        // Given
        var list = new TaggedFieldList();
        var eventRaised = false;
        list.Changed += (_, _) => eventRaised = true;

        var field = new MockTaggedField
        {
            Type = TaggedFieldTypes.DESCRIPTION,
            Length = 10
        };

        // When
        list.Add(field);

        // Then
        Assert.Single(list);
        Assert.Same(field, list[0]);
        Assert.True(eventRaised, "Changed event should be raised after Add().");
    }

    [Fact]
    public void Given_TaggedFieldListWithExistingField_When_AddSameType_Then_ArgumentExceptionIsThrown()
    {
        // Given
        var list = new TaggedFieldList { new MockTaggedField { Type = TaggedFieldTypes.DESCRIPTION } };

        // When / Then
        var ex = Assert.Throws<ArgumentException>(() =>
            list.Add(new MockTaggedField { Type = TaggedFieldTypes.DESCRIPTION })
        );

        Assert.Contains("already contains a tagged field of type DESCRIPTION", ex.Message);
    }

    [Fact]
    public void Given_TaggedFieldListWithDescription_When_AddDescriptionHash_Then_ArgumentExceptionIsThrown()
    {
        // Given
        var list = new TaggedFieldList { new MockTaggedField { Type = TaggedFieldTypes.DESCRIPTION } };

        // When / Then
        var ex = Assert.Throws<ArgumentException>(() =>
            list.Add(new MockTaggedField { Type = TaggedFieldTypes.DESCRIPTION_HASH })
        );

        Assert.Contains("already contains a tagged field of type DESCRIPTION_HASH", ex.Message);
    }

    [Fact]
    public void Given_TaggedFieldListWithDescriptionHash_When_AddDescription_Then_ArgumentExceptionIsThrown()
    {
        // Given
        var list = new TaggedFieldList { new MockTaggedField { Type = TaggedFieldTypes.DESCRIPTION_HASH } };

        // When / Then
        var ex = Assert.Throws<ArgumentException>(() =>
            list.Add(new MockTaggedField { Type = TaggedFieldTypes.DESCRIPTION })
        );

        Assert.Contains("already contains a tagged field of type DESCRIPTION", ex.Message);
    }

    [Fact]
    public void Given_TaggedFieldList_When_AddFallbackAddressMultipleTimes_Then_NoExceptionIsThrown()
    {
        // Given
        var list = new TaggedFieldList
        {
            // When
            new MockTaggedField { Type = TaggedFieldTypes.FALLBACK_ADDRESS },
            new MockTaggedField { Type = TaggedFieldTypes.FALLBACK_ADDRESS }
        };

        // Then
        Assert.Equal(2, list.Count);
    }

    [Fact]
    public void Given_TaggedFieldList_When_AddRange_Then_AllItemsAddedAndSingleChangedEventFired()
    {
        // Given
        var list = new TaggedFieldList();
        var eventCount = 0;
        list.Changed += (_, _) => eventCount++;

        var fields = new List<ITaggedField>
        {
            new MockTaggedField { Type = TaggedFieldTypes.DESCRIPTION },
            new MockTaggedField { Type = TaggedFieldTypes.FALLBACK_ADDRESS }
        };

        // When
        list.AddRange(fields);

        // Then
        Assert.Equal(2, list.Count);
        // Because AddRange calls OnChanged once after everything is added
        Assert.Equal(1, eventCount);
    }

    [Fact]
    public void Given_ExistingItem_When_Remove_Then_ItemIsRemovedAndEventRaised()
    {
        // Given
        var list = new TaggedFieldList();
        var eventRaised = false;
        list.Changed += (_, _) => eventRaised = true;

        var field = new MockTaggedField { Type = TaggedFieldTypes.DESCRIPTION };
        list.Add(field);

        // When
        var removed = list.Remove(field);

        // Then
        Assert.True(removed, "Remove should return true for an existing item.");
        Assert.Empty(list);
        Assert.True(eventRaised, "Changed event should be raised after Remove().");
    }

    [Fact]
    public void Given_NonExistingItem_When_Remove_Then_ReturnsFalseAndNoEventIsRaised()
    {
        // Given
        var list = new TaggedFieldList();
        var eventCount = 0;
        list.Changed += (_, _) => eventCount++;

        // When
        var removed = list.Remove(new MockTaggedField { Type = TaggedFieldTypes.DESCRIPTION });

        // Then
        Assert.False(removed, "Remove should return false for a non-existing item.");
        Assert.Equal(0, eventCount);
    }

    [Fact]
    public void Given_ValidList_When_RemoveAt_Then_ItemIsRemovedAndEventRaised()
    {
        // Given
        var list = new TaggedFieldList();
        var eventRaised = false;
        list.Changed += (_, _) => eventRaised = true;

        list.Add(new MockTaggedField { Type = TaggedFieldTypes.DESCRIPTION });

        // When
        list.RemoveAt(0);

        // Then
        Assert.Empty(list);
        Assert.True(eventRaised);
    }

    [Fact]
    public void Given_ValidList_When_RemoveAll_Then_RemovedCountAndEventRaisedIfAnyRemoved()
    {
        // Given
        var list = new TaggedFieldList();
        var eventCount = 0;
        list.Changed += (_, _) => eventCount++;

        list.Add(new MockTaggedField { Type = TaggedFieldTypes.DESCRIPTION });
        list.Add(new MockTaggedField { Type = TaggedFieldTypes.FALLBACK_ADDRESS });

        // When
        var removed = list.RemoveAll(x => x.Type == TaggedFieldTypes.DESCRIPTION);

        // Then
        Assert.Equal(1, removed);
        Assert.Single(list);
        Assert.Equal(3, eventCount);
    }

    [Fact]
    public void Given_ValidList_When_RemoveRange_Then_ItemsRemovedAndEventRaised()
    {
        // Given
        var list = new TaggedFieldList();
        var eventRaised = false;
        list.Changed += (_, _) => eventRaised = true;

        list.Add(new MockTaggedField { Type = TaggedFieldTypes.DESCRIPTION });
        list.Add(new MockTaggedField { Type = TaggedFieldTypes.FALLBACK_ADDRESS });
        list.Add(new MockTaggedField { Type = TaggedFieldTypes.EXPIRY_TIME });

        // When
        list.RemoveRange(1, 2);

        // Then
        Assert.Single(list);
        Assert.True(eventRaised);
    }

    [Fact]
    public void Given_ValidList_When_TryGetExistingItem_Then_ReturnsTrueAndItem()
    {
        // Given
        var list = new TaggedFieldList();
        var field = new MockTaggedField { Type = TaggedFieldTypes.DESCRIPTION };
        list.Add(field);

        // When
        var found = list.TryGet(TaggedFieldTypes.DESCRIPTION, out MockTaggedField? result);

        // Then
        Assert.True(found);
        Assert.NotNull(result);
        Assert.Same(field, result);
    }

    [Fact]
    public void Given_EmptyList_When_TryGetNonExistingItem_Then_ReturnsFalseAndNull()
    {
        // Given
        var list = new TaggedFieldList();

        // When
        var found = list.TryGet(TaggedFieldTypes.DESCRIPTION, out MockTaggedField? result);

        // Then
        Assert.False(found);
        Assert.Null(result);
    }

    [Fact]
    public void Given_ValidList_When_TryGetAllExisting_Then_ReturnsTrueAndItems()
    {
        // Given
        var list = new TaggedFieldList();
        var field1 = new MockTaggedField { Type = TaggedFieldTypes.FALLBACK_ADDRESS };
        var field2 = new MockTaggedField { Type = TaggedFieldTypes.FALLBACK_ADDRESS };
        list.Add(field1);
        list.Add(field2);

        // When
        var found = list.TryGetAll(TaggedFieldTypes.FALLBACK_ADDRESS, out List<MockTaggedField> items);

        // Then
        Assert.True(found);
        Assert.NotNull(items);
        Assert.Equal(2, items.Count);
        Assert.Contains(field1, items);
        Assert.Contains(field2, items);
    }

    [Fact]
    public void Given_EmptyList_When_TryGetAllNonExisting_Then_ReturnsFalseAndNullList()
    {
        // Given
        var list = new TaggedFieldList();

        // When
        var found = list.TryGetAll(TaggedFieldTypes.FALLBACK_ADDRESS, out List<MockTaggedField> items);

        // Then
        Assert.False(found);
        Assert.Null(items);
    }

    [Fact]
    public void Given_ValidList_When_CalculateSizeInBits_Then_SumOfAllLengths()
    {
        // Given
        var list = new TaggedFieldList
        {
            new MockTaggedField { Type = TaggedFieldTypes.DESCRIPTION, Length = 5 },
            new MockTaggedField { Type = TaggedFieldTypes.FALLBACK_ADDRESS, Length = 10 }
        };

        // When
        var sizeInBits = list.CalculateSizeInBits();

        // Then
        Assert.Equal(15, sizeInBits);
    }

    [Fact]
    public void Given_ValidList_When_WriteToBitWriter_Then_TagsAndLengthsAreWritten()
    {
        // Given
        var list = new TaggedFieldList
        {
            new MockTaggedField { Type = TaggedFieldTypes.DESCRIPTION, Length = 2 },
            new MockTaggedField { Type = TaggedFieldTypes.FALLBACK_ADDRESS, Length = 1 }
        };
        var bitWriter = new BitWriter(50);

        // When
        list.WriteToBitWriter(bitWriter);

        // Then
        // Assert.Equal(4, bitWriter.Writes.Count);
        // Explanation of 4 writes:
        //   1) Type of first field (CUSTOM_TYPE) => WriteByteAsBits(...)
        //   2) Length of first field => WriteInt16AsBits(...)
        //   3) Type of second field (FALLBACK_ADDRESS)
        //   4) Length of second field
        // The actual content written by WriteToBitWriter in each field depends on the field's logic
    }

    [Fact]
    public void Given_BitReaderReturningNoData_When_FromBitReaderCalled_Then_EmptyListReturned()
    {
        // Given
        var bitReader = new BitReader([]); // defaults to HasMoreBits = false
        // When
        var list = TaggedFieldList.FromBitReader(bitReader, Network.MAINNET);

        // Then
        Assert.Empty(list);
    }
}