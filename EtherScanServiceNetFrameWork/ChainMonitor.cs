using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtherScanServiceNetFrameWork
{
    public class ChainMonitor
    {
        public void Analysis()
        {
            var txs = EtherDBService
                .DBTransaction
                .Where(x => x.Value.Amount > ConfigSettings.BigWalletBarrier)
                .Where(x => x.Value.TimeStamp > DateTime.Now.AddDays(-40))
                .Select(x => x.Value)
                .ToList();

            Visual(txs);
        }

        public void Visual(IList<EthTransaction> txs)
        {
            var sender = txs
                          .Where(x => x.SenderType() == WalletType.CEX)
                          .ToList();

            var receiver = txs
                            .Where(x => x.ReceiverType() == WalletType.CEX)
                            .ToList();

            foreach (var day in EachDay(GetStartDate(txs), GetLastDate(txs)))
            {
                var outFlow = sender
                    .Where(x => x.TimeStamp.Date == day)
                    .Sum(x => x.Amount);

                var inFlow = receiver
                    .Where(x => x.TimeStamp.Date == day)
                    .Sum(x => x.Amount);

                Console.WriteLine($"[ChainMonitor] {day.ToString("yyyy-MM-dd")}, Net flow: {inFlow - outFlow}");
            }
        }

        public DateTime GetStartDate(IList<EthTransaction> txs)
        {
            return txs
                .OrderByDescending(x => x.TimeStamp)
                .LastOrDefault()
                .TimeStamp.Date;
        }

        public DateTime GetLastDate(IList<EthTransaction> txs)
        {
            return txs
                .OrderByDescending(x => x.TimeStamp)
                .FirstOrDefault()
                .TimeStamp.Date;
        }

        public IEnumerable<DateTime> EachDay(DateTime from, DateTime to)
        {
            for (var day = from.Date; day.Date <= to.Date; day = day.AddDays(1))
                yield return day;
        }
    }
}
