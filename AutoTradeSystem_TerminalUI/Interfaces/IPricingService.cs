using Microsoft.Extensions.Hosting; 

namespace AutoTradeSystem_TerminalUI.Interfaces
{
    public interface IPricingService :IHostedService
    {
        public IDictionary<string, decimal> GetLatestPrices();
        public IEnumerable<string> GetLatestTickers();
    }
}