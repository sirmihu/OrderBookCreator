using System.Diagnostics;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

public enum TickSide
{
    BID = 1,
    ASK
}

public class Tick
{
    public long SourceTime { get; set; }
    public string Side { get; set; }
    public char Action { get; set; }
    public long OrderId { get; set; }
    public double Price { get; set; }
    public int Qty { get; set; }
    public double? B0 { get; set; }
    public int? BQ0 { get; set; }
    public int? BN0 { get; set; }
    public double? A0 { get; set; }
    public int? AQ0 { get; set; }
    public int? AN0 { get; set; }

    public Tick()
    {
        
    }

    public Tick(
        long sourceTime,
        string side,
        char action,
        long orderId,
        double price,
        int qty)
    {
        SourceTime = sourceTime;
        Side = side;
        Action = action;
        OrderId = orderId;
        Price = price;
        Qty = qty;
    }
}

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

        var sourceTicks = ReadCsv();

        foreach (var tick in sourceTicks)
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
        }

        WriteCsv(sourceTicks);

        sw.Stop();
        Console.WriteLine("*** Creation OrderBook process completed ***");
        Console.WriteLine($"Total time [us]: {sw.ElapsedMilliseconds * 1000.0:F3}");
        Console.WriteLine($"Time per tick [us]: {sw.ElapsedMilliseconds * 1000.0 / ticks.Count:F3}");
    }

    private List<Tick> ReadCsv()
    {
        var config = new CsvConfiguration(CultureInfo.CurrentCulture) { Delimiter = ";", MissingFieldFound = null};

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


