using System.Net;

namespace PollyHttpClientFactory
{
    /// <summary>
    /// Class PollySettings.
    /// </summary>
    public class PollySettings
    {
        /// <summary>
        /// Number of seconds before failing
        /// </summary>
        /// <value>The duration before failing.</value>
        public int? DurationBeforeFailing { get; set; }

        /// <summary>
        /// The duration the circuit will stay open before resetting in seconds
        /// </summary>
        /// <value>The duration of break.</value>
        public double? DurationOfBreak { get; set; }

        /// <summary>
        /// The maximum number of concurrent actions that may be executing through the policy.
        /// Default value set to 20 based on maximum through put for SCIO
        /// </summary>
        public int MaxParallelization { get; set; } = 20;

        /// <summary>
        /// The maximum number of actions that may be queuing, waiting for an execution slot.
        /// Default set to 30 to ensure that work can be queued for SCIO calls
        /// </summary>
        public int MaxQueuingActions { get; set; } = 30;

        public readonly HttpStatusCode[] HttpStatusCodesWorthRetrying =
            {
                HttpStatusCode.InternalServerError,
                HttpStatusCode.BadGateway,
                HttpStatusCode.GatewayTimeout,
                HttpStatusCode.Forbidden,
                HttpStatusCode.PaymentRequired,
                HttpStatusCode.TooManyRequests
            };
    }
}