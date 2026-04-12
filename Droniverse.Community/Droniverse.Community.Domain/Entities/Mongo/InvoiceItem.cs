namespace Droniverse.Community.Domain.Entities.Mongo
{
    public class InvoiceItem
    {
        public Guid ProductID { get; set; }
        public string ProductNameVN { get; set; } = null!;
        public string ProductNameEN { get; set; } = null!;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Total { get; set; }
        public string Currency { get; set; } = "VND";
    }
}
