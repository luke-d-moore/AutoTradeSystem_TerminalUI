using Microsoft.Extensions.Hosting; 
using AutoTradeSystem_TerminalUI;
using AutoTradeSystem_TerminalUI.Dtos;

namespace AutoTradeSystem_TerminalUI.Interfaces
{
    public interface IAutoTradingStrategyService :IHostedService
    {
        public IDictionary<string, TradingStrategy> GetStrategies();
        public Task<AddStrategyResponse> AddStrategy(TradingStrategyDto tradingStrategyDto);
    }
}