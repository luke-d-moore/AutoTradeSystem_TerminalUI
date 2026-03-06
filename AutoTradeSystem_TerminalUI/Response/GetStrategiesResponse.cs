using AutoTradeSystem_TerminalUI.Dtos;
using AutoTradeSystem_TerminalUI;
using System.Text.Json.Serialization;

namespace AutoTradeSystem_TerminalUI 
{
    public class GetStrategiesResponse
    {
        [JsonPropertyName("TradingStrategies")]
        public IDictionary<string, TradingStrategy> TradingStrategies { get; set; }

        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("timeStamp")]
        public DateTime TimeStamp { get; set; }
    }
}

