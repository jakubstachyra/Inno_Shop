using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductService.Domain.Entities
{
        public class Product
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int ID { get; set; }

            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public decimal Price { get; set; }
            public bool IsAvailable { get; set; }
            public int CreatorUserID { get; set; }
            public DateTime CreationDate { get; set; }
            public bool IsDeleted { get; set; } = false;
        }
}
