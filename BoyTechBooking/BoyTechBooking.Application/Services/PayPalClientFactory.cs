using Microsoft.Extensions.Configuration;
using PayPalCheckoutSdk.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoyTechBooking.Application.Services
{
    public class PayPalClientFactory
    {
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly bool _isSandbox;

        public PayPalClientFactory(IConfiguration config)
        {
        }

        public PayPalClientFactory(string clientId, string clientSecret, bool isSandbox)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
            _isSandbox = isSandbox;
        }

        public PayPalHttpClient CreateClient()
        {

            var mode = Environment.GetEnvironmentVariable("PAYPAL_MODE") ?? "Sandbox";
            var clientId = Environment.GetEnvironmentVariable("PAYPAL_CLIENT_ID");
            var clientSecret = Environment.GetEnvironmentVariable("PAYPAL_CLIENT_SECRET");

            if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
                throw new InvalidOperationException("PayPal credentials not found in environment variables.");

            PayPalEnvironment environment = mode.Equals("Live", StringComparison.OrdinalIgnoreCase)
                ? new LiveEnvironment(clientId, clientSecret)
                : new SandboxEnvironment(clientId, clientSecret);

            return new PayPalHttpClient(environment);
        }
    }
}
