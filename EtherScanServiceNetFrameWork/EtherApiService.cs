using EtherscanApi.Net.Interfaces;
using EtherscanApi.Net.Objects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EtherScanServiceNetFrameWork
{
    class EtherApiService
    {
        private static readonly string ApiKey = "2SFPY65Y2DUEXJBFJD75PZYVMNSTZTCQXT";
        private static readonly EtherScanClient EthClient = new EtherScanClient(ApiKey);

        public EtherApiServiceResponse GetBalance(List<string> address)
        {
            var batchId = 0;
            var serviceResponse = new EtherApiServiceResponse();

            foreach (var adr in address)
            {
                batchId++;
                var response = EthClient.GetEtherBalance(adr);

                if (response.Success)
                {
                    serviceResponse.SuccessWallets.Add(adr);
                    serviceResponse.WalletsInfo.Add(new EtherWalletInfo()
                    {
                        Address = adr,
                        Balance = response.Result,
                    });

                    Console.WriteLine($"[GetBalance] {batchId}/{address.Count} success, address: {adr} balance: {response.Result}");
                }
                else
                {
                    Console.WriteLine($"[GetBalance] failed, address: {adr} error: {response.Message}");
                }
            }

            return serviceResponse;
        }

        public EtherApiServiceResponse BatchGetTransaction(IList<string> walletsAddress)
        {
            var batchId = 0;
            var serviceResponse = new EtherApiServiceResponse();

            foreach (var adr in walletsAddress)
            {
                batchId++;
                var response = EthClient.GetTransactions(adr);

                if (response.Success)
                {
                    var txs = response.Result
                                      .Where(x => x.Amount > ConfigSettings.FilterTransactionAmount)
                                      .Select(x => EthTransaction.ToEthTransaction(x))
                                      .ToList();

                    serviceResponse.SuccessWallets.Add(adr);
                    serviceResponse.Transactions.AddRange(txs);

                    Console.WriteLine($"[GetTransaction] {batchId}/{walletsAddress.Count} success, address: {adr}, count: {txs.Count}");
                }
                else
                {
                    Console.WriteLine($"[GetTransaction] error: {response.Message}. address: {adr}");
                }
            }

            return serviceResponse;
        }
    }
}
