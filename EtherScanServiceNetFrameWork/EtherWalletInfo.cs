using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtherScanServiceNetFrameWork
{
    public class EtherWalletInfo
    {
        public WalletType? Type;

        public WalletType? GetWalletType()
        {
            if (Type == null || Type == WalletType.Unknow)
            {
                ENS.GetWalletType(Address);
            }

            return Type;
        }

        public string UpdatedDateTime { get; set; } = DateTime.Now.ToString(ConfigSettings.RecordDateTimeFormat);

        public string CreateDateTime { get; set; } = DateTime.Now.ToString(ConfigSettings.RecordDateTimeFormat);

        public int Rank { get; set; }

        public string Address { get; set; }

        public string NameTag { get; set; } = string.Empty;

        public decimal Balance { get; set; }

        public string Symbol { get; set; } = "ETH";

        public string Percentage { get; set; }

        public int TxnCount { get; set; }

        public void SetTopAddressValue(int id, string value)
        {
            switch (id)
            {
                case 0:
                    Rank = int.Parse(value);
                    break;

                case 1:
                    Address = value;
                    break;

                case 2:
                    NameTag = value;
                    break;

                case 3:
                    Balance = decimal.Parse(value.Split(' ').FirstOrDefault());
                    Symbol = value.Split(' ').LastOrDefault();
                    break;

                case 4:
                    Percentage = value;
                    break;

                case 5:
                    value = value.Replace(",", "");
                    TxnCount = int.Parse(value);
                    break;
            }
        }
        public static string CsvHeader()
        {
            return
                "UpdatedDateTime,CreateDateTime,Rank,NameTag,Address,Balance,Symbol,Percentage,TxnCount";
        }

        public string ToCsv()
        {
            var csvLine = string.Empty;

            csvLine += UpdatedDateTime + ",";
            csvLine += CreateDateTime + ",";
            csvLine += Rank + ",";
            csvLine += NameTag + ",";
            csvLine += Address + ",";
            csvLine += Balance + ",";
            csvLine += Symbol + ",";
            csvLine += Percentage + ",";
            csvLine += TxnCount + ",";

            return csvLine;
        }

        public static EtherWalletInfo FromCsv(string csvLine)
        {
            if (string.IsNullOrEmpty(csvLine)) return null;

            var values = csvLine.Split(',');
            EtherWalletInfo etherWalletInfo;

            try
            {
                etherWalletInfo = new EtherWalletInfo()
                {
                    UpdatedDateTime = values[0],
                    CreateDateTime = values[1],
                    Rank = int.Parse(values[2]),
                    NameTag = values[3],
                    Address = values[4],
                    Balance = decimal.Parse(values[5], NumberStyles.Float),
                    Symbol = values[6],
                    Percentage = values[7],
                    TxnCount = int.Parse(values[8]),
                };
            }
            catch(Exception ex)
            {
                Console.WriteLine($"[EtherWalletInfo.FromCsv] error: {ex.Message}");
                return null;
            }

            return etherWalletInfo;
        }
    }
}
