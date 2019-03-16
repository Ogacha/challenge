using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static ChallengeSpike.Global;

namespace ChallengeSpike
{
    public static class MainLogic
    {
        /// <summary>
        /// Search symbols and get their price data
        /// </summary>
        /// <param name="symbol">a part of symbol string</param>
        /// <param name="exchanges">stock exchanges list</param>
        /// <returns>price data of seached symbols</returns>
        public static async Task<(Error, dynamic)> GetQueryResultAsync(string symbol, ICollection<string> exchanges)
        {
            var (error, symbolListData) =
                await GetDataFromWebAsync($"stock_search?search_term={symbol}&search_by=symbol&api_token={ApiToken}");

            if (error != Error.None) return (error, symbolListData);

            var symbols = from e in (symbolListData as JToken)
                          where e["name"].Value<string>() != "N/A"
                          select new
                          {
                              Symbol = e["symbol"].Value<string>(),
                              StockExchange = e["stock_exchange_short"].Value<string>()
                          };

            var group = from s in symbols group s.Symbol by s.StockExchange;

            var returnData = new Dictionary<string, dynamic>();
            foreach (var stockExchange in group)
            {
                var symbolPrices = new List<dynamic>();
                foreach (var symbolName in stockExchange)
                {
                    var (err, priceDataDynamic) = await GetDataFromWebAsync($"stock?api_token={ApiToken}&symbol={symbolName}");
                    if (err == Error.None)
                    {
                        var priceData = (priceDataDynamic as JArray)[0];
                        if (exchanges.Contains(priceData["stock_exchange_short"].ToString()))
                            symbolPrices.Add(
                              new
                              {
                                  symbol = priceData["symbol"].ToString(),
                                  name = priceData["name"].ToString(),
                                  price = priceData["price"].ToString(),
                                  close_yesterday = priceData["close_yesterday"].ToString(),
                                  currency = priceData["currency"].ToString(),
                                  market_cap = priceData["market_cap"].ToString(),
                                  volume = priceData["volume"].ToString(),
                                  timezone = priceData["timezone"].ToString(),
                                  timezone_name = priceData["timezone_name"].ToString(),
                                  gmt_offset = priceData["gmt_offset"].ToString(),
                                  last_trade_time = priceData["last_trade_time"].ToString()
                              });
                    }
                }
                if (symbolPrices.Any())
                    returnData.Add(stockExchange.Key, (symbolPrices.Count == 1) ? symbolPrices.First() : symbolPrices);
            }
            if(returnData.Any()) return (Error.None, returnData);
            else return (Error.NoData, new { });
        }

        /// <summary>
        /// Get error and data from WebAPI
        /// </summary>
        public static async Task<(Error, dynamic)> GetDataFromWebAsync(string query)
        {
            var client = new HttpClient { BaseAddress = ServiceUri };
            var response = await client.GetAsync(query);
            if (!response.IsSuccessStatusCode)
            {
                var statusCode = (int)response.StatusCode;
                if (400 <= statusCode && statusCode <= 499) return (Error.BadRequest, "The request is something wrong.");
                else return (Error.Exception, "Service error.");
            }

            var dataString = await response.Content.ReadAsStringAsync();

            var data = JObject.Parse(dataString)["data"];
            if (data == null) return (Error.NoData, new { });
            else return (Error.None, data);
        }
    }
}
