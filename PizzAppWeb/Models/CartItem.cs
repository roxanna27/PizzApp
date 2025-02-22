using System.ComponentModel.DataAnnotations;

public class CartItem
{
    public int Id { get; set; }

    // Legătura cu utilizatorul curent
    [Required]
    public string ApplicationUserId { get; set; }
    public ApplicationUser ApplicationUser { get; set; }

    [Required]
    public int ProductId { get; set; }
    public Product Product { get; set; }

    [Required]
    public int Quantity { get; set; }
}
