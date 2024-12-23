using System.ComponentModel.DataAnnotations;

namespace UserManagement.Domain.Entities
{
    public class Base
    {
        [Key]
        public int ID { get; set; }
    }
}
