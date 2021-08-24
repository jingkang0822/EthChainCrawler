using EtherscanApi.Net.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtherScanServiceNetFrameWork
{
    public enum Flow
    {
        In,
        Out,
        Unkown
    }

    public class EthTransaction : Transaction
    {
        public static EthTransaction ToEthTransaction(Transaction transaction)
        {
            return new EthTransaction()
            {
                BlockNumber = transaction.BlockNumber,
                TimeStamp = transaction.TimeStamp,
                TxId = transaction.TxId,
                nonce = transaction.nonce,
                BlockHash = transaction.BlockHash,
                TxIndex = transaction.TxIndex,
                FromAddress = transaction.FromAddress,
                ToAddress = transaction.ToAddress,
                Amount = transaction.Amount,
                gas = transaction.gas,
                gasPrice = transaction.gasPrice,
                IsError = transaction.IsError,
                txreceipt_status = transaction.txreceipt_status,
                input = transaction.input,
                contractAddress = transaction.contractAddress,
                cumulativeGasUsed = transaction.cumulativeGasUsed,
                gasUsed = transaction.gasUsed,
                confirmations = transaction.confirmations
            };
        }

        public Flow ExgInOrOut { get; set; }

        public void SetWalletTag(string wallet, string tag)
        {
            if (wallet == FromAddress)
            {
                ExgInOrOut = Flow.Out;
                FromWalletTag = tag;
            }
            else if (wallet == ToAddress)
            {
                ExgInOrOut = Flow.In;
                ToWalletTag = tag;
            }
            else
            {
                ExgInOrOut = Flow.Unkown;
            }
        }

        public string FromWalletTag { get; set; } = string.Empty;

        public string ToWalletTag { get; set; } = string.Empty;

        private WalletType? Sender { get; set; }

        public WalletType? SenderType()
        {
            if (Sender == null || Sender == WalletType.Unknow)
            {
                Sender = ENS.GetWalletType(FromAddress);
            }

            return Sender;
        }

        private WalletType? Receiver { get; set; }

        public WalletType? ReceiverType()
        {
            if (Receiver == null || Receiver == WalletType.Unknow)
            {
                Receiver = ENS.GetWalletType(ToAddress);
            }

            return Receiver;
        }

        public static EthTransaction FromCsv(string csvLine)
        {
            if (string.IsNullOrEmpty(csvLine)) return null;

            EthTransaction transaction;

            try
            {
                var values = csvLine.Split(',');

                transaction = new EthTransaction()
                {
                    TimeStamp = Convert.ToDateTime(values[0]),
                    ExgInOrOut = values[1] == Flow.In.ToString() ? Flow.In :
                             values[1] == Flow.Out.ToString() ? Flow.Out :
                             Flow.Unkown,
                    Amount = Convert.ToDecimal(values[2]),
                    FromWalletTag = values[3],
                    FromAddress = values[4],
                    ToWalletTag = values[5],
                    ToAddress = values[6],
                    TxId = values[7],
                    BlockNumber = ulong.Parse(values[8]),
                    nonce = values[9],
                    BlockHash = values[10],
                    gas = values[11],
                    gasPrice = values[12],
                    IsError = Convert.ToBoolean(values[13]),
                    txreceipt_status = values[14],
                    input = values[15],
                    contractAddress = values[16],
                    cumulativeGasUsed = values[17],
                    gasUsed = values[18],
                    confirmations = values[19]
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Transaction.FromCsv] error: {ex.Message}");
                return null;
            }

            return transaction;
        }

        public static string CsvHeader()
        {
            return
                "TimeStamp,ExgInOrOut,Amount,FromWalletTag,FromAddress,ToWalletTag,ToAddress,TxId," +
                "BlockNumber,nonce,BlockHash,gas,gasPrice,IsError,txreceipt_status," +
                "input,contractAddress,cumulativeGasUsed,gasUsed,confirmations";
        }

        public string ToCsv()
        {
            var csvLine = string.Empty;

            csvLine += TimeStamp.ToString("yyyy-MM-dd HH:mm:ss") + ",";
            csvLine += ExgInOrOut.ToString() + ",";
            csvLine += Amount.ToString() + ",";
            csvLine += FromWalletTag + ",";
            csvLine += FromAddress + ",";
            csvLine += ToWalletTag + ",";
            csvLine += ToAddress + ",";
            csvLine += TxId + ",";
            csvLine += BlockNumber + ",";
            csvLine += nonce + ",";
            csvLine += BlockHash + ",";
            csvLine += gas + ",";
            csvLine += gasPrice + ",";
            csvLine += IsError + ",";
            csvLine += txreceipt_status + ",";
            csvLine += input + ",";
            csvLine += contractAddress + ",";
            csvLine += cumulativeGasUsed + ",";
            csvLine += gasUsed + ",";
            csvLine += confirmations + ",";

            return csvLine;
        }
    }
}
