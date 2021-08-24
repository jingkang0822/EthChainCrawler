using EtherscanApi.Net.Interfaces;
using EtherscanApi.Net.Objects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EtherScanServiceNetFrameWork
{
    public class EtherScanService
    {
        private static readonly EtherDBService EthDB = new EtherDBService(ConfigSettings.DataFolder);
        private static readonly EtherWebCrawler EthWeb = new EtherWebCrawler();
        private static readonly EtherApiService EthApiService = new EtherApiService();

        public static void Start()
        {
            CrawlTopWallet();

            UpdateDBWalletBalance();

            SpreadingSearchNetwork();

            QueryBigWalletNameTag();
        }

        private static void CrawlTopWallet()
        {
            Task.Factory.StartNew(() =>
            {
                if (IfNeedToCrawlTopWallet())
                {
                    var topWallets = EthWeb.GetTopWallets();
                    EthDB.ReplaceWallet(topWallets);

                    GetTransaction(topWallets
                        .Select(x => x.Address)
                        .ToList());
                }
            });
        }

        private static bool IfNeedToCrawlTopWallet()
        {
            var dir = new DirectoryInfo(ConfigSettings.BigWalletFolder);
            var lastestFile = dir.GetFiles("TopWallets_*")
                .OrderByDescending(x => x.Name)
                .FirstOrDefault();

            if (!lastestFile.Name.Contains(DateTime.Now.ToString(ConfigSettings.FileDateFormat)))
            {
                return true;
            }

            return false;
        }

        private static void UpdateDBWalletBalance()
        {
            foreach (var type in Enum.GetValues(typeof(WalletType)).Cast<WalletType>())
            {
                if (type == WalletType.Unknow) continue;

                UpdateDBWalletBalance(type);
            }
        }

        private static void UpdateDBWalletBalance(WalletType walletType)
        {
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    if (UpdateBalanceAndTransaction(walletType) == 0)
                    {
                        Console.WriteLine($"[UpdateDBWalletBalance] No {walletType.ToString()} wallet need to query balance, waiting..");
                        Thread.Sleep((int)TimeSpan.FromMinutes(ConfigSettings.UpdateBalanceTransactionWaitingInMinutes).TotalMilliseconds);
                    }
                }
            });
        }

        private static int UpdateBalanceAndTransaction(WalletType walletType)
        {
            var addressList = ShouldUpdateBalance(walletType).ToList();
            Console.WriteLine($"[UpdateBalanceAndTransaction] walletType: {walletType.ToString()}, count: {addressList.Count}");

            var batchId = 0;
            var batchAddress = new List<string>();
            while ((batchAddress = addressList.Skip(ConfigSettings.BatchUpdateSize * batchId++).Take(ConfigSettings.BatchUpdateSize).ToList()).Any())
            {
                Console.WriteLine($"[UpdateBalanceAndTransaction] batch: {batchId}/{addressList.Count/ ConfigSettings.BatchUpdateSize}");
                
                var updatedWalletBalances = EthApiService.GetBalance(batchAddress).WalletsInfo;
                EthDB.UpdateBalances(updatedWalletBalances);

                GetTransaction(updatedWalletBalances
                                                   .Select(x => x.Address)
                                                   .ToList());
            }

            return addressList.Count;
        }

        private static IList<string> ShouldUpdateBalance(WalletType walletType)
        {
            Console.WriteLine($"[ShouldUpdateBalance] walletType: {walletType.ToString()}");

            var filteredWallet = EtherDBService.DBWallet
                .Where(x => x.Value.GetWalletType() == walletType);

            switch (walletType)
            {
                case WalletType.CEX:
                    return filteredWallet
                        .Where(x => (DateTime.Now - DateTime.Parse(x.Value.UpdatedDateTime)).TotalMinutes > ConfigSettings.UpdateCEXInMinutes)
                        .Select(x => x.Value.Address)
                        .ToList();

                case WalletType.DEX:
                    return filteredWallet
                        .Where(x => (DateTime.Now - DateTime.Parse(x.Value.UpdatedDateTime)).TotalMinutes > ConfigSettings.UpdateDEXInMinutes)
                        .Select(x => x.Value.Address)
                        .ToList();

                case WalletType.BigWallet:
                    return filteredWallet
                        .Where(x => (DateTime.Now - DateTime.Parse(x.Value.UpdatedDateTime)).TotalDays > ConfigSettings.UpdateBigWalletInDays)
                        .Select(x => x.Value.Address)
                        .ToList();

                case WalletType.SmallWallet:
                    return filteredWallet
                        .Where(x => (DateTime.Now - DateTime.Parse(x.Value.UpdatedDateTime)).TotalDays > ConfigSettings.UpdateSmallWalletInDays)
                        .Select(x => x.Value.Address)
                        .ToList();

                default:
                    return null;
            }
        }

        private static void SpreadingSearchNetwork()
        {
            Task.Factory.StartNew(() => 
            {
                while (true)
                {
                    if (SpreadingSearch(
                        ConfigSettings.BatchSizeSpreadingSearch, 
                        DateTime.Now.AddMonths(-ConfigSettings.SpreadingSearchWithinMonths)) == 0)
                    {
                        Console.WriteLine("[SpreadingSearchNetwork] No big wallet need to query balance, waiting..");
                        Thread.Sleep((int)TimeSpan.FromMinutes(ConfigSettings.SpreadingSearchWaitingInSeconds).TotalMilliseconds);
                    }
                }
            });
        }

        private static int SpreadingSearch(int checkNumber, DateTime laterThan)
        {
            Console.WriteLine($"[GetBigWalletBalance] checkNumber: {checkNumber}, laterThan: {laterThan.ToString(ConfigSettings.RecordDateTimeFormat)}");

            var checkWallets = new List<string>();
            var bigTransaction = EtherDBService.DBTransaction
                                    .Where(x => string.IsNullOrEmpty(x.Value.ToWalletTag) || string.IsNullOrEmpty(x.Value.FromWalletTag))
                                    .Where(x => !string.IsNullOrEmpty(x.Value.ToAddress) && !string.IsNullOrEmpty(x.Value.FromAddress))
                                    .Where(x => x.Value.TimeStamp > laterThan)
                                    .OrderByDescending(x => x.Value.Amount)
                                    .Select(x => x.Value)
                                    .ToList();

            bigTransaction.ForEach(bigT =>
            {
                if (string.IsNullOrEmpty(bigT.FromWalletTag) &&
                    !EtherDBService.DBWallet.ContainsKey(bigT.FromAddress) &&
                    !checkWallets.Contains(bigT.FromAddress))
                {
                    checkWallets.Add(bigT.FromAddress);
                }

                if (string.IsNullOrEmpty(bigT.ToWalletTag) &&
                    !EtherDBService.DBWallet.ContainsKey(bigT.ToAddress) &&
                    !checkWallets.Contains(bigT.ToAddress))
                {
                    checkWallets.Add(bigT.ToAddress);
                }
            });

            Console.WriteLine($"[GetBigWalletBalance] start checkWallets count: {checkWallets.Count}, take: {checkNumber}");

            checkWallets = checkWallets
                                .Take(checkNumber)
                                .ToList();

            if (checkWallets.Any())
            {
                var wallets = EthApiService.GetBalance(checkWallets).WalletsInfo;
                EthDB.ReplaceWallet(wallets);

                GetTransaction(checkWallets);
            }

            return checkWallets.Count;
        }

        private static void GetTransaction(IList<string> walletsAddress)
        {
            var walletTransaction = EthApiService.BatchGetTransaction(walletsAddress);

            EthDB.MergeTransaction(walletTransaction.Transactions);
        }

        private static void QueryBigWalletNameTag()
        {
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    if (QueryBigWalletNameTag(ConfigSettings.BatchSizeGetNameTag) == 0)
                    {
                        Console.WriteLine("[QueryBigWalletNameTag] No big wallet need to query, waiting..");
                        Thread.Sleep((int)TimeSpan.FromMinutes(5).TotalMilliseconds);
                    }
                }
            });
        }

        private static int QueryBigWalletNameTag(int checkNumber)
        {
            var bigWalletWithNoName = EtherDBService.DBWallet
                                    .Where(x => string.IsNullOrEmpty(x.Value.NameTag))
                                    .OrderBy(x => x.Value.Rank)
                                    .Take(checkNumber)
                                    .Select(x => x.Value.Address)
                                    .ToList();


            if (bigWalletWithNoName.Any())
            {
                var getWalletsBalance = EthApiService.GetBalance(bigWalletWithNoName).WalletsInfo;

                getWalletsBalance.ForEach(x =>
                {
                    x.NameTag = EthWeb.GetNameTag(x.Address);
                    Thread.Sleep((int)TimeSpan.FromSeconds(ConfigSettings.CrawlerWaitingInSeconds).TotalMilliseconds);
                });

                EthDB.ReplaceWallet(getWalletsBalance);
            }

            return bigWalletWithNoName.Count;
        }
    }
}
