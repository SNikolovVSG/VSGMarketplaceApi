using System.ComponentModel.DataAnnotations;

namespace VSGMarketplaceApi.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        public int Code { get; set; }

        [Required]
        [MinLength(3)]
        public string Name { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public double OrderPrice { get; set; }

        [Required]
        [EmailAddress]
        public string OrderedBy { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime OrderDate { get; set; }

        [Required]
        public string Status { get; set; }

        [Required]
        public int UserId { get; set; }
    }
}
