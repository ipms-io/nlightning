using NBitcoin;
using NLightning.Domain.Channels.ValueObjects;
using NLightning.Tests.Utils.Vectors;

namespace NLightning.Bolt11.Tests.Models;

using Domain.Models;
using Domain.ValueObjects;

public class RoutingInfoCollectionTests
{
    private readonly RoutingInfo _defaultRoutingInfo = new(
        new PubKey(InitiatorValidKeysVector.RemoteStaticPublicKey),
        new ShortChannelId(870127, 1237, 1), 1, 1, 1
    );

    [Fact]
    public void Given_EmptyCollection_When_AddWithinCapacity_Then_ItemIsAddedAndChangedEventRaised()
    {
        // Given
        var collection = new RoutingInfoCollection();
        var eventRaised = false;
        collection.Changed += (_, _) => eventRaised = true;

        // When
        collection.Add(_defaultRoutingInfo);

        // Then
        Assert.Single(collection);
        Assert.Same(_defaultRoutingInfo, collection[0]);
        Assert.True(eventRaised, "Changed event should be raised when an item is added.");
    }

    [Fact]
    public void Given_FullCollection_When_AddItem_Then_InvalidOperationExceptionIsThrown()
    {
        // Given
        var collection = new RoutingInfoCollection();
        // Fill up the collection with 12 items (max capacity)
        for (var i = 0; i < 12; i++)
        {
            collection.Add(_defaultRoutingInfo);
        }

        // When / Then
        var ex = Assert.Throws<InvalidOperationException>(() =>
            collection.Add(_defaultRoutingInfo)
        );
        Assert.Contains("maximum capacity of 12 has been reached", ex.Message);
    }

    [Fact]
    public void Given_EmptyCollection_When_AddRangeWithinCapacity_Then_ItemsAreAddedNoException()
    {
        // Given
        var collection = new RoutingInfoCollection();
        var routingInfos = Enumerable.Range(1, 3)
            .Select(_ => _defaultRoutingInfo);

        // When
        collection.AddRange(routingInfos);

        // Then
        Assert.Equal(3, collection.Count);
    }

    [Fact]
    public void Given_Collection_When_AddRangeExceedsCapacity_Then_InvalidOperationExceptionIsThrown()
    {
        // Given
        var collection = new RoutingInfoCollection();
        // 11 items already added
        for (var i = 0; i < 11; i++)
        {
            collection.Add(_defaultRoutingInfo);
        }

        // nextAddRange tries to add 2 items => total 13 > capacity
        var newItems = new List<RoutingInfo>
        {
            _defaultRoutingInfo,
            _defaultRoutingInfo
        };

        // When / Then
        var ex = Assert.Throws<InvalidOperationException>(() => collection.AddRange(newItems));
        Assert.Contains("maximum capacity of 12 has been reached", ex.Message);
    }

    [Fact]
    public void Given_ExistingItem_When_Remove_Then_ItemIsRemovedAndChangedEventRaised()
    {
        // Given
        var collection = new RoutingInfoCollection();
        var eventRaised = false;
        collection.Changed += (_, _) => eventRaised = true;
        collection.Add(_defaultRoutingInfo);

        // When
        var removed = collection.Remove(_defaultRoutingInfo);

        // Then
        Assert.True(removed, "Remove should return true for an existing item.");
        Assert.Empty(collection);
        Assert.True(eventRaised, "Changed event should be raised when an existing item is removed.");
    }

    [Fact]
    public void Given_NonExistingItem_When_Remove_Then_ReturnsFalseAndNoEventRaised()
    {
        // Given
        var collection = new RoutingInfoCollection();
        var eventCount = 0;
        collection.Changed += (_, _) => eventCount++;

        // When
        var removed = collection.Remove(_defaultRoutingInfo);

        // Then
        Assert.False(removed);
        Assert.Equal(0, eventCount);
    }

    [Fact]
    public void Given_CollectionWithItems_When_RemoveAt_Then_ItemIsRemovedAndEventRaised()
    {
        // Given
        var collection = new RoutingInfoCollection();
        var eventRaised = false;
        collection.Changed += (_, _) => eventRaised = true;

        collection.Add(_defaultRoutingInfo);
        collection.Add(_defaultRoutingInfo);

        // When
        collection.RemoveAt(0);

        // Then
        Assert.Single(collection);
        Assert.True(eventRaised);
    }

    [Fact]
    public void Given_CollectionWithItems_When_RemoveAll_Then_MatchingItemsRemovedAndEventRaised()
    {
        // Given
        var collection = new RoutingInfoCollection();
        var eventCount = 0;
        collection.Changed += (_, _) => eventCount++;

        collection.Add(_defaultRoutingInfo);
        collection.Add(_defaultRoutingInfo);
        collection.Add(new RoutingInfo(new PubKey(InitiatorValidKeysVector.RemoteStaticPublicKey),
                                       new ShortChannelId(870127, 1237, 1), 2, 1, 1));

        // When
        var removedCount = collection.RemoveAll(r => r.FeeBaseMsat == 1);

        // Then
        Assert.Equal(2, removedCount);
        Assert.Single(collection);
        Assert.Equal(4, eventCount);
    }

    [Fact]
    public void Given_CollectionWithItems_When_RemoveRange_Then_ItemsRemovedAndEventRaised()
    {
        // Given
        var collection = new RoutingInfoCollection();
        var eventRaised = false;
        collection.Changed += (_, _) => eventRaised = true;

        for (var i = 0; i < 5; i++)
        {
            collection.Add(_defaultRoutingInfo);
        }

        // When
        collection.RemoveRange(1, 3); // remove Node1, Node2, Node3

        // Then
        Assert.Equal(2, collection.Count); // Node0, Node4 remain
        Assert.True(eventRaised);
    }
}