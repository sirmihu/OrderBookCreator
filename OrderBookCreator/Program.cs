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

            var tempOrderBook = new List<Tick>();

            foreach (var tick in inputParams)
            {
                if (tick.Side == TickSide.BID)
                {
                    if (tick.Action == 'Y' || tick.Action == 'F')
                    {
                        tempOrderBook.Clear();
                    }
                    else if (tick.Action == 'A')
                    {
                        var existingOrder = tempOrderBook.FirstOrDefault(order => order.OrderId == tick.OrderId);

                        if (existingOrder != null)
                        {
                            tempOrderBook.Remove(existingOrder);
                        }
                        tempOrderBook.Add(tick);
                    }
                    else if (tick.Action == 'M')
                    {
                        var existingOrder = tempOrderBook.FirstOrDefault(order => order.OrderId == tick.OrderId);

                        if (existingOrder != null)
                        {
                            tempOrderBook.Remove(existingOrder);
                            tempOrderBook.Add(tick);
                        }
                        else
                        {
                            tempOrderBook.Add(tick);
                        }
                    }
                    else if (tick.Action == 'D')
                    {
                        var existingOrder = tempOrderBook.FirstOrDefault(order => order.OrderId == tick.OrderId);

                        if (existingOrder != null)
                        {
                            tempOrderBook.Remove(existingOrder);
                        }
                    }

                    var bestBid = tempOrderBook.Where(order => order.Side == TickSide.BID).Max(order => order.Price);
                    var totalBidQty = tempOrderBook.Where(order => order.Side == TickSide.BID && order.Price == bestBid).Sum(order => order.Qty);
                    var totalBidOrders = tempOrderBook.Count(order => order.Side == TickSide.BID && order.Price == bestBid);

                    tick.B0 = bestBid;
                    tick.BQ0 = totalBidQty;
                    tick.BN0 = totalBidOrders;
                }

                else if (tick.Side == TickSide.ASK)
                {
                    if (tick.Action == 'Y' || tick.Action == 'F')
                    {
                        tempOrderBook.Clear();
                    }
                    else if (tick.Action == 'A')
                    {
                        var existingOrder = tempOrderBook.FirstOrDefault(order => order.OrderId == tick.OrderId);
                        if (existingOrder != null)
                        {
                            tempOrderBook.Remove(existingOrder);
                        }
                        tempOrderBook.Add(tick);
                    }
                    else if (tick.Action == 'M')
                    {
                        var existingOrder = tempOrderBook.FirstOrDefault(order => order.OrderId == tick.OrderId);

                        if (existingOrder != null)
                        {
                            tempOrderBook.Remove(existingOrder);
                            tempOrderBook.Add(tick);
                        }
                        else
                        {
                            tempOrderBook.Add(tick);
                        }
                    }
                    else if (tick.Action == 'D')
                    {
                        var existingOrder = tempOrderBook.FirstOrDefault(order => order.OrderId == tick.OrderId);

                        if (existingOrder != null)
                        {
                            tempOrderBook.Remove(existingOrder);
                        }
                    }

                    var bestAsk = tempOrderBook.Where(order => order.Side == TickSide.ASK).Min(order => order.Price);
                    var totalAskQty = tempOrderBook.Where(order => order.Side == TickSide.ASK && order.Price == bestAsk).Sum(order => order.Qty);
                    var totalAskOrders = tempOrderBook.Count(order => order.Side == TickSide.ASK && order.Price == bestAsk);

                    tick.A0 = bestAsk;
                    tick.AQ0 = totalAskQty;
                    tick.AN0 = totalAskOrders;
                }
            }
        }

        public static IEnumerable<Tick> ReadCsv(Stream file)
        {
            var reader = new StreamReader(file);
            var config = new CsvConfiguration(CultureInfo.CurrentCulture) { Delimiter = ";" };
            var csv = new CsvReader(reader, config);

            var records = csv.GetRecords<Tick>();

            return records;
        }
    }
}