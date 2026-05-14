namespace PuppetFestAPP.Web.Data // Make sure this matches your project's namespace
{
    public class Sale
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int LocationId { get; set; }
        public int QuantitySold { get; set; }
        public decimal UnitPriceAtSale { get; set; }
        public DateTime SaleDate { get; set; }
        public bool IsManualAdjustment { get; set; }

        public virtual Product? Product { get; set; }
        public virtual Location? Location { get; set; }

    }
}
