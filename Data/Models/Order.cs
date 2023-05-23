using System.ComponentModel.DataAnnotations;

namespace Data.Models
{
    public class Order
    {
        [Required]
        public int Code { get; set; }

        [Required]
        public int ItemCode { get; set; }

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
        public string OrderedBy { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime OrderDate { get; set; }

        [Required]
        public string Status { get; set; }

        [Required]
        public bool IsDeleted { get; set; }
    }
}
