using AutoTradeSystem_TerminalUI.Interfaces;
using AutoTradeSystem_TerminalUI.Dtos;
using AutoTradeSystem_TerminalUI;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;

namespace AutoTradeSystem_TerminalUI.Services
{
    public class AutoTradingStrategyService : AutoTradingStrategyServiceBase, IAutoTradingStrategyService
    {
        private ConcurrentDictionary<string, TradingStrategy> _tradingStrategies = new ConcurrentDictionary<string, TradingStrategy>();
        private int _retryInterval = 5000;
        private HttpClient _client;
        public ConcurrentDictionary<string, TradingStrategy> TradingStrategies => _tradingStrategies;
        public AutoTradingStrategyService(HttpClient client)
        {
            _client = client;
        }
        protected override async Task UpdateStrategies(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                var response = await _client.GetFromJsonAsync<GetStrategiesResponse>("api/TradingStrategy/");
                if (response.Success)
                {
                    foreach (var kvp in response.TradingStrategies) {
                        _tradingStrategies[kvp.Key] = kvp.Value;
                    }
                }
                    await Task.Delay(_retryInterval);    
                }
                catch (OperationCanceledException ex) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    await Task.Delay(_retryInterval);
                }
            }
        }
        public IDictionary<string, TradingStrategy> GetStrategies()
        {
            return TradingStrategies;
        }
        public async Task<AddStrategyResponse> AddStrategy(TradingStrategyDto tradingStrategyDto)
        {
            var response = await _client.PostAsJsonAsync("api/TradingStrategy/", tradingStrategyDto);
            if (response.IsSuccessStatusCode) {
                return await response.Content.ReadFromJsonAsync<AddStrategyResponse>();
            }
            else
            {
                return new AddStrategyResponse(){Success = false};
            }
        }
    }
}
