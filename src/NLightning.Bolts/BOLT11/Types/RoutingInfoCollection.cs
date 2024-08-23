using System.Collections.ObjectModel;

namespace NLightning.Bolts.BOLT11.Types;

/// <summary>
/// A collection of routing information
/// </summary>
public sealed class RoutingInfoCollection : ObservableCollection<RoutingInfo>
{
    /// <summary>
    /// Te maximum amount of routing information that can be stored in a single routing information list
    /// </summary>
    /// <remarks>
    /// The maximum length of a tagged field is 1023 * 5 bits. The routing information is 408 bits long.
    /// 1023 * 5 bits = 5115 bits / 408 bits = 12.5 => round down to 12
    /// </remarks>
    private const int MAX_CAPACITY = 12;

    /// <summary>
    /// Adds a routing information to the collection
    /// </summary>
    /// <param name="routingInfo">The routing information to add</param>
    /// <exception cref="InvalidOperationException">Thrown when the maximum capacity has been reached</exception>
    /// <remarks>
    /// The maximum capacity of the collection is 12
    /// </remarks>
    public new void Add(RoutingInfo routingInfo)
    {
        if (Count >= MAX_CAPACITY)
        {
            throw new InvalidOperationException($"The maximum capacity of {MAX_CAPACITY} has been reached");
        }

        base.Add(routingInfo);
    }
}