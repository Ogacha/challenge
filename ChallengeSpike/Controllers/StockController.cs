using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using static ChallengeSpike.Global;
using static ChallengeSpike.MainLogic;
using System.Text.RegularExpressions;

namespace ChallengeSpike.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class StockController : ControllerBase
    {
        Regex symbolRegex = new Regex(@"^([A-Z0-9.]+)$");
        Regex stockExchangesRegex = new Regex(@"^([A-Z0-9.]+)(,[A-Z0-9.]+)*$");

        [HttpGet("{symbol}")]
        public async Task<ActionResult<dynamic>> GetSymbolDataAsync(string symbol)
        {
            if (!symbolRegex.IsMatch(symbol)) return BadRequest("The symbol's characters should be upper alphabets and/or numbers.");

            var stockExchangesRawString = Request.Query["stock_exchange"].ToString();
            var stockExchangesString = (stockExchangesRawString == "") ? DefaultStockExchange : stockExchangesRawString;
            if (!stockExchangesRegex.IsMatch(stockExchangesString))
                return BadRequest("The stock exchange's characters should be upper alphabets and/or numbers. And the separator should be comma.");

            var stockExchanges = stockExchangesString.Split(',');
            var (error, data) = await GetQueryResultAsync(symbol, stockExchanges);

            if (error == Error.BadRequest) return BadRequest(data);
            if (error == Error.Exception) return StatusCode(503, data);
            return data;
        }
    }
}