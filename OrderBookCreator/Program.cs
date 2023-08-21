using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using OrderBookCreator.Models;

namespace OrderBookCreator
{
    public class Program
    {
        static void Main(string[] args)
        {
            var inputParams = ReadCsv(File.OpenRead("ticks.csv"));

            


        }

        public static IEnumerable<Tick> ReadCsv(Stream file)
        {
            var reader = new StreamReader(file);
            var config = new CsvConfiguration(CultureInfo.CurrentCulture) { Delimiter = ";" };
            var csv = new CsvReader(reader, config);

            var records = csv.GetRecords<Tick>();

            return records;
        }

        public void CreateOrderBook(IEnumerable<Tick> sourceTicks)
        {
            var ticks = new List<Tick>();
            double currentA0 = 0;
            int currentAQ0 = 0;
            int currentAN0 = 0;
            double currentB0 = 0;
            int currentBQ0 = 0;
            int currentBN0 = 0;

            foreach (var tick in sourceTicks)
            {
                if (tick.Action == 'Y' || tick.Action == 'F')
                {
                    tick.B0 = null;
                    tick.BQ0 = null;
                    tick.BN0 = null;
                    tick.A0 = null;
                    tick.AQ0 = null;
                    tick.AN0 = null;
                }
                else if (tick.Action == 'A' && tick.Side == TickSide.BID)
                {
                    ticks.Add(tick);

                    if (tick.Price >= currentB0)
                    {
                        currentB0 = tick.Price;
                        currentBQ0 = ticks.Count(p => p.Price == tick.Price);
                        currentBN0 = ticks.Where(p => p.Price == tick.Price).Select(p => p.Qty).Sum() + tick.Qty;
                    }

                    tick.B0 = currentB0;
                    tick.BN0 = currentBQ0;
                    tick.BQ0 = currentBN0;
                }
                else if (tick.Action == 'A' && tick.Side == TickSide.ASK)
                {
                    ticks.Add(tick);

                    if (tick.Price < currentA0)
                    {
                        currentA0 = tick.Price;
                        currentAN0 = ticks.Count(p => p.Price == tick.Price);
                        currentAQ0 = ticks.Where(p => p.Price == tick.Price).Select(p => p.Qty).Sum();
                    }

                    tick.A0 = currentA0;
                    tick.AQ0 = currentAQ0;
                    tick.AN0 = currentAN0;
                }
                else if (tick.Action == 'M')
                {
                    ticks.RemoveAll(p => p.OrderId == tick.OrderId);
                    ticks.Add(tick);

                    if (tick.Side == TickSide.BID)
                    {
                        tick.B0 = ticks.Max(p => p.Price);
                        tick.BN0 = ticks.Count(p => p.Price == tick.B0);
                        tick.BQ0 = ticks.Where(p => p.Price == tick.B0).Select(p => p.Qty).Sum();
                    }
                    else if (tick.Side == TickSide.ASK)
                    {
                        tick.A0 = ticks.Min(p => p.Price);
                        tick.AN0 = ticks.Count(p => p.Price == tick.A0);
                        tick.AQ0 = ticks.Where(p => p.Price == tick.A0).Select(p => p.Qty).Sum();
                    }
                }
                else if (tick.Action == 'D')
                {
                    var numberOfTicksRemoved = ticks.RemoveAll(p => p.OrderId == tick.OrderId);

                    if (numberOfTicksRemoved > 0 && tick.Side == TickSide.BID)
                    {
                        tick.B0 = ticks.Max(p => p.Price);
                        tick.BN0 = ticks.Count(p => p.Price == tick.B0);
                        tick.BQ0 = ticks.Where(p => p.Price == tick.B0).Select(p => p.Qty).Sum();
                    }
                    else if (numberOfTicksRemoved > 0 && tick.Side == TickSide.ASK)
                    {
                        tick.A0 = ticks.Min(p => p.Price);
                        tick.AN0 = ticks.Count(p => p.Price == tick.A0);
                        tick.AQ0 = ticks.Where(p => p.Price == tick.A0).Select(p => p.Qty).Sum();
                    }
                }
            }

        }
    }
}