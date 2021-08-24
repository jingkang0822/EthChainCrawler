using EtherscanApi.Net.Converters;
using EtherscanApi.Net.Objects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace EtherscanApi.Net.Interfaces
{
    public class EtherScanClient : IEtherScanClient
    {
        private const string _baseUrl = "https://api.etherscan.io/api?";
        private readonly string _apiKey;
        WebClient wc = new WebClient();
        private static object _locker = new object();

        public EtherScanClient(string apiKey)
        {
            _apiKey = apiKey;
        }

        private void ApiThreadWaiting()
        {
            lock (_locker)
            {
                // Free API 5 calls per second
                Thread.Sleep((int)TimeSpan.FromMilliseconds(200).TotalMilliseconds);
            }
        }

        public EtherScanDefaultResponse<decimal> GetEtherBalance(string address)
        {
            var parameters = new Dictionary<string, object>()
            {
                {"module", "account" },
                {"action", "balance" },
                {"address", address }
            };
            var result = GetResult<decimal>(parameters);
            if (result.Success)
                result.Result = UnitConversion.Convert.FromWei(new System.Numerics.BigInteger(result.Result), UnitConversion.EthUnit.Ether);
            return result;
        }

        public EtherScanDefaultResponse<List<BatchAddressBalance>> GetEtherBalances(List<string> address)
        {
            var parameters = new Dictionary<string, object>()
            {
                {"module", "account" },
                {"action", "balance" },
                {"address", string.Join(",", address)}
            };

            return GetResult<List<BatchAddressBalance>>(parameters);
        }

        public EtherScanDefaultResponse<List<SmartContract>> GetSmartContractDescription(string address)
        {
            var parameters = new Dictionary<string, object>()
            {
                {"module", "contract" },
                {"action", "getsourcecode" },
                {"address", address }
            };

            return GetResult<List<SmartContract>>(parameters);
        }

        public EtherScanDefaultResponse<List<Transaction>> GetTransactions(string address, ulong? fromBlock = null, ulong? toBlock = null, string sort = "desc", int? page = 1, int? limit = 1000)
        {
            var parameters = new Dictionary<string, object>()
            {
                { "module", "account" },
                { "action", "txlist" },
                { "address", address },
                { "startblock", fromBlock },
                { "endblock", toBlock ?? 99999999 },
                { "sort", sort },
                { "page", page },
                { "offset", limit }

            };
            return GetResult<List<Transaction>>(parameters);
        }
        public EtherScanDefaultResponse<List<Transaction>> GetInternalTransactions(string address, ulong? fromBlock = null, ulong? toBlock = null, string sort = "asc", int? page = 1, int? limit = 1000)
        {
            var parameters = new Dictionary<string, object>()
            {
                {"module", "account" },
                {"action", "txlistinternal" },
                {"address", address },
                {"startblock",fromBlock },
                {"endblock",toBlock??99999999 },
                {"sort",sort },
                {"page",page },
                {"offset",limit }

            };
            return GetResult<List<Transaction>>(parameters);
        }

        public EtherScanDefaultResponse<List<Erc20TokenTransfer>> GetErc20TokenTransfers(string address, string contract = null, ulong? fromBlock = null, ulong? toBlock = null, string sort = "asc", int? page = 1, int? limit = 1000)
        {
            var parameters = new Dictionary<string, object>()
            {
                {"module", "account" },
                {"action", "tokentx" },
                {"address", address },

                {"startblock",fromBlock },
                {"endblock",toBlock??99999999 },
                {"sort",sort },
                {"page",page },
                {"offset",limit }

            };
            if (!string.IsNullOrEmpty(contract))
            {
                parameters.Add("contractaddress", contract);
            }
            return GetResult<List<Erc20TokenTransfer>>(parameters);
        }

        private string ConstructRequest(Dictionary<string, object> parameters)
        {
            parameters.Add("apiKey", _apiKey);
            string requestUrl = _baseUrl + string.Join("&", parameters.Select(kvp => string.Format("{0}={1}", kvp.Key, kvp.Value)));
            return requestUrl;
        }

        private EtherScanDefaultResponse<T> GetResult<T>(Dictionary<string, object> parameters)
        {
            ApiThreadWaiting();

            try
            {
                using (var webClient = new WebClient())
                using (var stream = webClient.OpenRead(ConstructRequest(parameters)))
                using (var reader = new StreamReader(stream))
                {
                    var response = reader.ReadToEnd();
                    return JsonConvert.DeserializeObject<EtherScanDefaultResponse<T>>(response);
                }
            }
            catch (Exception Ex)
            {
                return new EtherScanDefaultResponse<T>() { Message = $"Error: {Ex.Message}" };
            }
        }
    }
}
