namespace NLightning.Common.Interfaces;

using Types;

public interface IFeeService
{
    /// <summary>
    /// Gets the current fee rate in satoshis per kiloweight (sat/kW)
    /// </summary>
    Task<LightningMoney> GetFeeRatePerKwAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current cached fee rate without checking API
    /// </summary>
    LightningMoney GetCachedFeeRatePerKw();

    /// <summary>
    /// Forces a refresh of the fee rate from the API
    /// </summary>
    Task RefreshFeeRateAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts a background task to periodically refresh the fee rate
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops the background task and cancels any ongoing operations within the service.
    /// </summary>
    /// <returns>A task representing the asynchronous stop operation.</returns>
    Task StopAsync();
}