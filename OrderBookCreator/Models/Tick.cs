namespace OrderBookCreator.Models
{
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
}
