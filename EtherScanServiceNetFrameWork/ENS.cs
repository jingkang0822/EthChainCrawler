using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtherScanServiceNetFrameWork
{
    public class ENS
    {
        public static List<string> CexWhiteList = new List<string>()
        {
            "Binance",
            "Kraken",
            "Bittrex",
            "Huobi",
            "Gemini",
            "FTX",
            "OKEx",
            "Gemini",
            "Bitfinex",
        };

        public static List<string> DexWhiteList = new List<string>()
        {
            "Polygon",
            "Wrapped Ether",
            "Compound",
            "Aave",
            "Celsius",
            "Synthetix",
            "Alpha Finance",
            "Cream",
            "CryptoPunks",
            "Polygon",
        };

        public static bool IsCex (string nameTag)
        {
            foreach(var cexName in CexWhiteList)
            {
                if (nameTag.Contains(cexName))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsDex(string nameTag)
        {
            foreach (var dexName in DexWhiteList)
            {
                if (nameTag.Contains(dexName))
                {
                    return true;
                }
            }

            return false;
        }

        public static WalletType GetWalletType(string address)
        {
            if (EtherDBService.DBWallet.ContainsKey(address))
            {
                var walletInfo = EtherDBService.DBWallet[address];

                if (IsCex(walletInfo.NameTag))
                {
                    return WalletType.CEX;
                }
                else if (IsDex(walletInfo.NameTag))
                {
                    return WalletType.DEX;
                }
                else if (walletInfo.Balance > ConfigSettings.BigWalletBarrier)
                {
                    return WalletType.BigWallet;
                }
                else
                {
                    return WalletType.SmallWallet;
                }
            }
            else
            {
                return WalletType.Unknow;
            }
        }
    }
}
