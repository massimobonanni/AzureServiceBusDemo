namespace CommonLib.Entities
{
    public class Order
    {
        public int Id { get; set; }
        required public string CustomerName { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }

        public override string ToString()
        {
            return $"Order Id: {Id}, Customer: {CustomerName}, Date: {OrderDate}, Total: {TotalAmount:C}";
        }
    }
}