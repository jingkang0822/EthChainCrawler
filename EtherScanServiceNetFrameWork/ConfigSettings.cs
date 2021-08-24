using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtherScanServiceNetFrameWork
{
    public class ConfigSettings
    {
        public static string RecordDateTimeFormat = "yyyy/MM/dd HH:mm";

        public static string FileDateFormat = "yyyyMMdd";
        
        public static string DataFolder = @"..\..\..\Data";

        public static string BigWalletFolder = $@"{DataFolder}\TopWallet";

        public static readonly string BigWalletsSavePath = $@"{BigWalletFolder}\TopWallets_{DateTime.Now.ToString(FileDateFormat)}.csv";

        public static string TransactionFolder = $@"{DataFolder}\Transaction";

        public static string TransactionSavePath = $@"{TransactionFolder}\Transaction_{DateTime.Now.ToString(FileDateFormat)}.csv";

        public static int UpdateDBFileInMinutes = 15;

        public static decimal FilterTransactionAmount = 300;
     
        public static int UpdateCEXInMinutes = 15;

        public static int UpdateDEXInMinutes = 30;

        public static decimal BigWalletBarrier = 1000;
        
        public static int UpdateBigWalletInDays = 1;

        public static int UpdateSmallWalletInDays = 5;

        public static int BatchUpdateSize = 100;

        public static int UpdateBalanceTransactionWaitingInMinutes = 5;

        public static int SpreadingSearchWithinMonths = 3;

        public static int BatchSizeSpreadingSearch = 50;

        public static int SpreadingSearchWaitingInSeconds = 3;

        public static int BatchSizeGetNameTag = 3;

        public static int CrawlerWaitingInSeconds = 50;
    }
}
