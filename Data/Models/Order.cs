namespace Data.Models
{
    public class Order
    {
        public int Id { get; set; }

        public string ItemCode { get; set; }

        public string Location { get; set; }

        public int ItemId { get; set; }

        public string Name { get; set; }

        public int Quantity { get; set; }

        public double OrderPrice { get; set; }

        public string OrderedBy { get; set; }

        public DateTime OrderDate { get; set; }

        public string Status { get; set; }

        public bool IsDeleted { get; set; }
    }
}
