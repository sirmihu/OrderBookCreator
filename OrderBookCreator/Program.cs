using System.Diagnostics;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using OrderBookCreator.Models;

public class OrderBook
{
    static void Main(string[] args)
    {
        var orderBook = new OrderBook();

        orderBook.CreateOrderBookCsv();

        Console.ReadKey();
    }

    private void CreateOrderBookCsv()
    {
        var sw = new Stopwatch();

        sw.Start();
        Console.WriteLine("*** Creation OrderBook process started ***");

        var ticks = new List<Tick>();
        double? currentA0 = 0;
        int? currentAQ0 = 0;
        int? currentAN0 = 0;
        double? currentB0 = 0;
        int? currentBQ0 = 0;
        int? currentBN0 = 0;

        var config = new CsvConfiguration(CultureInfo.CurrentCulture) { Delimiter = ";" };

        using (var writer = new StreamWriter(File.OpenWrite("../../../ticks_result.csv")))
        using (var reader = new StreamReader(File.OpenRead("../../../ticks.csv")))
        using (var csv = new CsvWriter(writer, config))
        {
            string headerLine = reader.ReadLine();
            writer.WriteLine($"{nameof(Tick.SourceTime)};{nameof(Tick.Side)};{nameof(Tick.Action)};{nameof(Tick.OrderId)};" +
                $"{nameof(Tick.Price)};{nameof(Tick.Qty)};{nameof(Tick.B0)};{nameof(Tick.BQ0)};" + $"{nameof(Tick.BN0)};" +
                $"{nameof(Tick.A0)};{nameof(Tick.AQ0)};{nameof(Tick.AN0)}");

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var columns = line?.Split(";");

                var tick = new Tick(
                    long.Parse(columns[0]),
                    columns[1],
                    char.Parse(columns[2]),
                    long.Parse(columns[3]),
                    double.Parse(columns[4]),
                    int.Parse(columns[5]));

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
                    ticks.Add(tick);

                    if (tick.Price >= (currentB0 ?? 0))
                    {
                        currentB0 = tick.Price;
                        currentBQ0 = ticks.Where(p => p.Price == currentB0 && p.Side == "1").Sum(p => p.Qty);
                        currentBN0 = ticks.Count(p => p.Price == currentB0 && p.Side == "1");
                    }
                }
                else if (tick.Action == 'A' && tick.Side == "2")
                {
                    ticks.Add(tick);

                    if (tick.Price <= currentA0 || currentA0 == null)
                    {
                        currentA0 = tick.Price;
                        currentAQ0 = ticks.Where(p => p.Price == currentA0 && p.Side == "2").Sum(p => p.Qty);
                        currentAN0 = ticks.Count(p => p.Price == currentA0 && p.Side == "2");
                    }
                }
                else if (tick.Action == 'M')
                {
                    ticks.RemoveAll(p => p.OrderId == tick.OrderId);
                    ticks.Add(tick);

                    if (tick.Side == "1")
                    {
                        currentB0 = ticks.Where(p => p.Side == "1").Max(p => p.Price);
                        currentBQ0 = ticks.Where(p => p.Price == currentB0 && p.Side == "1").Sum(p => p.Qty);
                        currentBN0 = ticks.Count(p => p.Price == currentB0 && p.Side == "1");
                    }
                    else if (tick.Side == "2")
                    {
                        currentA0 = ticks.Where(p => p.Side == "2").Min(p => p.Price);
                        currentAQ0 = ticks.Where(p => p.Price == currentA0 && p.Side == "2").Sum(p => p.Qty);
                        currentAN0 = ticks.Count(p => p.Price == currentA0 && p.Side == "2");
                    }
                }
                else if (tick.Action == 'D')
                {
                    var numberOfTicksRemoved = ticks.RemoveAll(p => p.OrderId == tick.OrderId);

                    if (numberOfTicksRemoved > 0 && tick.Side == "1")
                    {
                        currentB0 = ticks.Where(p => p.Side == "1").Max(p => p.Price);
                        currentBQ0 = ticks.Where(p => p.Price == currentB0 && p.Side == "1").Sum(p => p.Qty);
                        currentBN0 = ticks.Count(p => p.Price == currentB0 && p.Side == "1");
                    }
                    else if (numberOfTicksRemoved > 0 && tick.Side == "2")
                    {
                        currentA0 = ticks.Where(p => p.Side == "2").Min(p => p.Price);
                        currentAQ0 = ticks.Where(p => p.Price == currentA0 && p.Side == "2").Sum(p => p.Qty);
                        currentAN0 = ticks.Count(p => p.Price == currentA0 && p.Side == "2");
                    }
                }

                tick.B0 = currentB0;
                tick.BQ0 = currentBQ0;
                tick.BN0 = currentBN0;
                tick.A0 = currentA0;
                tick.AQ0 = currentAQ0;
                tick.AN0 = currentAN0;

                csv.WriteRecord(tick);
                csv.NextRecord();
            }
        }

        sw.Stop();
        Console.WriteLine("*** Creation OrderBook process completed ***");
        Console.WriteLine($"Total time [us]: {sw.ElapsedMilliseconds * 1000.0:F3}");
        Console.WriteLine($"Time per tick [us]: {sw.ElapsedMilliseconds * 1000.0 / ticks.Count:F3}");
    }
}

