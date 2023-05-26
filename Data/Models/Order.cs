namespace Data.Models
{
    public class Order
    {
        public int Code { get; set; }

        public int ItemCode { get; set; }

        public string Name { get; set; }

        public int Quantity { get; set; }

        public double OrderPrice { get; set; }

        public string OrderedBy { get; set; }

        public DateTime OrderDate { get; set; }

        public string Status { get; set; }

        public bool IsDeleted { get; set; }
    }
}
