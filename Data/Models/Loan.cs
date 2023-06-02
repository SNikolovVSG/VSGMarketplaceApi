namespace Data.Models
{
    public class Loan
    {
        public int Id { get; set; }

        public int ItemId { get; set; }

        public string OrderedBy { get; set; }

        public int Quantity { get; set; }

        public DateTime LoanStartDate { get; set; }
        
        public DateTime? LoanEndDate { get; set; }
    }
}
