using ChallengeSpike;
using ChallengeSpike.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static ChallengeSpike.Global;
using static ChallengeSpike.MainLogic;

namespace UnitTest
{
    [TestClass]
    public class ForAPI
    {
        [ClassInitialize]
        public static void Init(TestContext text)
        {
            ServiceUri = new Uri("https://www.worldtradingdata.com/api/v1/");
            ApiToken = "tsrbWQ9Jit4U61bgapelmQaOohkvqd2Ge98UMSEJzcd1LSDLUnypzWn4W4qM";

        }


        [DataTestMethod,
            DataRow("AAPL"),
            DataRow("SZ"),
            ]
        public async Task GetDataFromWebTest(string symbol)
        {
            var (error, data) = await GetDataFromWebAsync($"stock_search?search_term={symbol}&search_by=symbol&api_token={ApiToken}");
            var array = (data as JArray);
            Assert.AreEqual(Error.None, error);
            Assert.IsTrue(array.Count <= 5);
            Assert.IsTrue(array[0]["symbol"].Value<string>().Contains(symbol));
            Assert.IsTrue(array[4]["symbol"].Value<string>().Contains(symbol));
        }


        [DataTestMethod,
         DataRow("AAPL", "MEX,NASDAQ", 2),
         DataRow("ABC", "ASX,NYSE", 2),
         DataRow("OBK", "OTCMKTS", 1),
         DataRow("OBK", "AMEX", 0),
        ]
        public async Task ExchangeFilterTest(string symbol, string stockExchanges, int count)
        {
            var stockExchangesArray = stockExchanges.Split(',');
            var (error, data) = await GetQueryResultAsync(symbol, stockExchangesArray);
            var dic = (data as Dictionary<string, dynamic>);
            Assert.AreEqual(Error.None, error);
            Assert.AreEqual(count, dic.Count);
        }

        [DataTestMethod,
            DataRow("GABC.L", "LSE"),
            DataRow("ABCD", "NASDAQ"),
            DataRow("COBK", ""),
        ]
        public async Task ExceptionNoDataTest(string symbol, string stockExchanges)
        {
            var stockExchangesArray = stockExchanges.Split(',');
            var (error, data) = await GetQueryResultAsync(symbol, stockExchangesArray);
            Assert.AreEqual(Error.NoData, error);
        }

    }
}
