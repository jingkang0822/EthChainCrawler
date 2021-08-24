using EtherscanApi.Net.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtherScanServiceNetFrameWork
{
    public class EtherApiServiceResponse
    {
        public List<string> SuccessWallets { get; set; } = new List<string>();

        public List<EthTransaction> Transactions { get; set; } = new List<EthTransaction>();

        public List<EtherWalletInfo> WalletsInfo { get; set; } = new List<EtherWalletInfo>();
    }
}
