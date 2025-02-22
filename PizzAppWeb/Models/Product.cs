using System.ComponentModel.DataAnnotations;

public class Product
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [StringLength(250)]
    public string Description { get; set; }

    [Required]
    public decimal Price { get; set; }

    // Opțional: URL pentru imaginea produsului
    public string ImageUrl { get; set; }
}
