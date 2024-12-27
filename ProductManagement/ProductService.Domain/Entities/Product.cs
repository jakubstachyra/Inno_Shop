namespace ProductService.Domain.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public  required string Name { get; set; }
        public required string Description { get; set; }
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }
        public int CreatorUserId { get; set; }
        public DateTime CreationDate { get; set; }
        public bool IsDeleted { get; set; }
    }

}
