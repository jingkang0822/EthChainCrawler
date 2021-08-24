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
    public class EtherDBService
    {
        public static ConcurrentDictionary<string, EthTransaction> DBTransaction = new ConcurrentDictionary<string, EthTransaction>();
        public static ConcurrentDictionary<string, EtherWalletInfo> DBWallet = new ConcurrentDictionary<string, EtherWalletInfo>();
        private static Timer DBWriter;

        public EtherDBService(string folderPath)
        {

            if (!DBTransaction.Any())
            {
                LoadTransactionDB();
            }

            if (!DBWallet.Any())
            {
                LoadTopWalletsDB(folderPath);
            }

            if (DBWriter == null)
            {
                DBWriter = new Timer(
                    t => WriteDataToDBFile(), 
                    null, 
                    (int)TimeSpan.FromMinutes(ConfigSettings.UpdateDBFileInMinutes).TotalMilliseconds, 
                    (int)TimeSpan.FromMinutes(ConfigSettings.UpdateDBFileInMinutes).TotalMilliseconds);

                //ChainMonitor monitor = new ChainMonitor();
                //monitor.Analysis();
            }
        }

        private void LoadTransactionDB()
        {
            var dir = new DirectoryInfo(ConfigSettings.TransactionFolder);
            var file = dir.GetFiles("Transaction_*")
                .OrderBy(x => x.Name)
                .LastOrDefault();

            if (file != null)
            {
                using (var fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (var sr = new StreamReader(fs))
                    {
                        string[] lines = sr.ReadToEnd().Split(new[] { Environment.NewLine }, StringSplitOptions.None);

                        foreach (var transaction in lines
                                                       .Skip(1)
                                                       .Select(x => EthTransaction.FromCsv(x)))
                        {
                            if (transaction != null && !DBTransaction.ContainsKey(transaction.TxId))
                            {
                                DBTransaction[transaction.TxId] = transaction;
                            }
                        }
                    }
                }
            }
        }

        private void WriteDataToDBFile()
        {
            WriteTransactionDBData();
            WriteWalletDataToDB();
        }

        private void WriteTransactionDBData()
        {
            using (var fs = new FileStream(ConfigSettings.TransactionSavePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                using (var sw = new StreamWriter(fs))
                {
                    sw.WriteLine(EthTransaction.CsvHeader());

                    foreach (var item in DBTransaction.OrderByDescending(x => x.Value.TimeStamp))
                    {
                        try
                        {
                            sw.WriteLine(item.Value.ToCsv());
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[WriteTransactionDBData] error: {ex.Message}");
                        }
                    }
                }
            }
        }

        public void MergeTransaction(IList<EthTransaction> transaction)
        {
            Console.WriteLine($"[MergeTransactionToDB] count: {transaction.Count}"); 
            
            if (transaction.Any())
            {
                foreach (var t in transaction)
                {
                    if (!DBTransaction.ContainsKey(t.TxId))
                    {
                        DBTransaction[t.TxId] = t;

                        PatchTransactionNameTag(t.TxId);
                    }
                }
            }
        }

        public void PatchAllTransactionWalletTag()
        {
            foreach (var item in DBTransaction)
            {
                PatchTransactionNameTag(item.Value.TxId);
            }
        }

        private void PatchTransactionNameTag(string txId)
        {
            if (DBTransaction.ContainsKey(txId))
            {
                var transaction = DBTransaction[txId];

                if (string.IsNullOrEmpty(transaction.FromWalletTag) && 
                    DBWallet.ContainsKey(transaction.FromAddress) &&
                    !string.IsNullOrEmpty(DBWallet[transaction.FromAddress].NameTag))
                {
                    Console.WriteLine($"[PatchTransactionNameTag] FromWallet: {DBWallet[transaction.FromAddress].NameTag}");
                    transaction.FromWalletTag = DBWallet[transaction.FromAddress].NameTag;
                }

                if (string.IsNullOrEmpty(transaction.ToWalletTag) && 
                    DBWallet.ContainsKey(transaction.ToAddress) &&
                    !string.IsNullOrEmpty(DBWallet[transaction.ToAddress].NameTag))
                {
                    Console.WriteLine($"[PatchTransactionNameTag] ToWallet: {DBWallet[transaction.ToAddress].NameTag}"); 
                    transaction.ToWalletTag = DBWallet[transaction.ToAddress].NameTag;
                }
            }
        }

        public void ReplaceWallet(IList<EtherWalletInfo> wallets)
        {
            Console.WriteLine($"[ReplaceWalletDataAndWrite] count: {wallets.Count}");
            ReplaceWalletDataToDB(wallets);

            PatchAllTransactionWalletTag();
        }

        public void UpdateBalances(IList<EtherWalletInfo> wallets)
        {
            foreach (var w in wallets)
            {
                DBWallet[w.Address].Balance = w.Balance;
                DBWallet[w.Address].UpdatedDateTime = w.UpdatedDateTime;
            }
        }

        private void ReplaceWalletDataToDB(IList<EtherWalletInfo> wallets)
        {
            foreach (var w in wallets)
            {
                // Assure lastest wallet info 
                Console.WriteLine($"[ReplaceWalletDataToDB] {w.NameTag} Address: {w.Address}");
                DBWallet[w.Address] = w;
            }
        }

        private void WriteWalletDataToDB()
        {
            PathWalletRank();

            var topWallets = DBWallet
                                .OrderBy(x => x.Value.Rank)
                                .Select(x => x.Value).ToList();

            try
            {
                using (var fs = new FileStream(ConfigSettings.BigWalletsSavePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                {
                    using (var sw = new StreamWriter(fs))
                    {
                        sw.WriteLine(EtherWalletInfo.CsvHeader());

                        foreach (var item in DBWallet.OrderBy(x => x.Value.Rank))
                        {
                            try
                            {
                                sw.WriteLine(item.Value.ToCsv());
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[WriteWalletDataToDB] error: {ex.Message}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WriteWalletDataToDB] error: {ex.Message}");
            }
        }

        private void PathWalletRank()
        {
            var rank = 1;

            var bigWalletOrderByBalance = DBWallet
                .OrderByDescending(x => x.Value.Balance)
                .ToList();

            bigWalletOrderByBalance
                .ForEach(x => x.Value.Rank = rank++);

            DBWallet = bigWalletOrderByBalance.ToConcurrentDictionary();
        }

        public void LoadTopWalletsDB(string folderPath)
        {
            var dir = new DirectoryInfo($@"{folderPath}\TopWallet");
            var file = dir.GetFiles("TopWallets_*")
                .OrderBy(x => x.Name)
                .LastOrDefault();

            if (file != null)
            {
                using (var fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (var sr = new StreamReader(fs))
                    {
                        string[] lines = sr.ReadToEnd().Split(new[] { Environment.NewLine }, StringSplitOptions.None);

                        foreach (var etherWallet in lines
                                                       .Skip(1)
                                                       .Select(x => EtherWalletInfo.FromCsv(x)))
                        {
                            if (etherWallet != null)
                            {
                                // Assure lastest wallet info 
                                DBWallet[etherWallet.Address] = etherWallet;
                            }
                        }
                    }
                }
            }
        }
    }
}
