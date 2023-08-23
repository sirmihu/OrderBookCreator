using System.Diagnostics;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using OrderBookCreator.Models;

public class OrderBook
{
    static void Main(string[] args)
    {
        for (int i = 0; i < 10; i++)
        {
            var orderBook = new OrderBook();

            orderBook.CreateOrderBookCsv();
        }

        Console.ReadKey();
    }

    private void CreateOrderBookCsv()
    {
        var bidTicks = new Dictionary<long, Tick>();
        var askTicks = new Dictionary<long, Tick>();
        double? currentA0 = 0;
        int? currentAQ0 = 0;
        int? currentAN0 = 0;
        double? currentB0 = 0;
        int? currentBQ0 = 0;
        int? currentBN0 = 0;

        var ticks = ReadCsv();

        var sw = new Stopwatch();
        sw.Start();
        Console.WriteLine("*** Creation OrderBook process started ***");

        foreach (var tick in ticks)
        {
            if (tick.Action == 'Y' || tick.Action == 'F')
            {
                currentB0 = null;
                currentBQ0 = null;
                currentBN0 = null;
                currentA0 = null;
                currentAQ0 = null;
                currentAN0 = null;
            }
            else if (tick.Action == 'A' && tick.Side == "1")
            {
                bidTicks.Add(tick.OrderId, tick);

                if (tick.Price == currentB0)
                {
                    currentBQ0 += tick.Qty;
                    currentBN0++;
                }
                else if (tick.Price > currentB0 || currentB0 == null)
                {
                    currentB0 = tick.Price;
                    currentBQ0 = tick.Qty;
                    currentBN0 = 1;
                }
            }
            else if (tick.Action == 'A' && tick.Side == "2")
            {
                askTicks.Add(tick.OrderId, tick);

                if (tick.Price == currentA0)
                {
                    currentAQ0 += tick.Qty;
                    currentAN0++;
                }
                else if (tick.Price < currentA0 || currentA0 == null)
                {
                    currentA0 = tick.Price;
                    currentAQ0 = tick.Qty;
                    currentAN0 = 1;
                }
            }
            else if (tick.Action == 'M' && tick.Side == "1")
            {
                if (bidTicks.ContainsKey(tick.OrderId))
                {
                    var modifiedTick = bidTicks[tick.OrderId];
                    bidTicks[tick.OrderId] = tick;

                    if (tick.Price == currentB0)
                    {
                        currentBQ0 += tick.Qty - modifiedTick.Qty;
                    }
                }
                else
                {
                    bidTicks.Add(tick.OrderId, tick);

                    if (tick.Price == currentB0)
                    {
                        currentBQ0 += tick.Qty;
                        currentBN0++;
                    }
                }

                if (tick.Price > currentB0 || currentB0 == null)
                {
                    currentB0 = tick.Price;
                    currentBQ0 = tick.Qty;
                    currentBN0 = 1;
                }
            }
            else if (tick.Action == 'M' && tick.Side == "2")
            {
                if (askTicks.ContainsKey(tick.OrderId))
                {
                    var modifiedTick = askTicks[tick.OrderId];
                    askTicks[tick.OrderId] = tick;

                    if (tick.Price == currentA0)
                    {
                        currentAQ0 += tick.Qty - modifiedTick.Qty;
                    }
                }
                else
                {
                    askTicks.Add(tick.OrderId, tick);

                    if (tick.Price == currentA0)
                    {
                        currentAQ0 += tick.Qty;
                        currentAN0++;
                    }
                }

                if (tick.Price < currentA0 || currentA0 == null)
                {
                    currentA0 = tick.Price;
                    currentAQ0 = tick.Qty;
                    currentAN0 = 1;
                }
            }
            else if (tick.Action == 'D' && tick.Side == "1")
            {
                if (bidTicks.ContainsKey(tick.OrderId))
                {
                    var removedTick = bidTicks[tick.OrderId];
                    bidTicks.Remove(tick.OrderId);

                    if (tick.Price == currentB0 && currentBN0 > 1)
                    {
                        currentBQ0 -= removedTick.Qty;
                        currentBN0--;
                    }

                    else if (tick.Price == currentB0)
                    {
                        currentB0 = bidTicks.Max(p => p.Value.Price);
                        currentBQ0 = bidTicks.Where(p => p.Value.Price == currentB0).Sum(p => p.Value.Qty);
                        currentBN0 = bidTicks.Count(p => p.Value.Price == currentB0);
                    }

                }
            }

            else if (tick.Action == 'D' && tick.Side == "2")
            {
                if (askTicks.ContainsKey(tick.OrderId))
                {
                    var removedTick = askTicks[tick.OrderId];
                    askTicks.Remove(tick.OrderId);

                    if (tick.Price == currentA0 && currentAN0 > 1)
                    {
                        currentAQ0 -= removedTick.Qty;
                        currentAN0--;
                    }

                    else if (tick.Price == currentA0)
                    {
                        currentA0 = askTicks.Min(p => p.Value.Price);
                        currentAQ0 = askTicks.Where(p => p.Value.Price == currentA0).Sum(p => p.Value.Qty);
                        currentAN0 = askTicks.Count(p => p.Value.Price == currentA0);
                    }

                }
            }

            tick.B0 = currentB0;
            tick.BQ0 = currentBQ0;
            tick.BN0 = currentBN0;
            tick.A0 = currentA0;
            tick.AQ0 = currentAQ0;
            tick.AN0 = currentAN0;
        }

        sw.Stop();
        Console.WriteLine("*** Creation OrderBook process completed ***");
        Console.WriteLine($"Total time [us]: {sw.ElapsedMilliseconds * 1000.0:F3}");
        Console.WriteLine($"Time per tick [us]: {sw.ElapsedMilliseconds * 1000.0 / (ticks.Count):F3}");

        WriteCsv(ticks);
    }

    private List<Tick> ReadCsv()
    {
        var config = new CsvConfiguration(CultureInfo.CurrentCulture) { Delimiter = ";", MissingFieldFound = null };

        using (var reader = new StreamReader(File.OpenRead("../../../ticks.csv")))
        using (var csv = new CsvReader(reader, config))
        {
            return csv.GetRecords<Tick>().ToList();
        }
    }

    private void WriteCsv(IEnumerable<Tick> ticks)
    {
        var config = new CsvConfiguration(CultureInfo.CurrentCulture) { Delimiter = ";" };

        using (var writer = new StreamWriter("../../../ticks_result.csv"))
        using (var csv = new CsvWriter(writer, config))
        {
            csv.WriteRecords(ticks);
        }
    }
}


