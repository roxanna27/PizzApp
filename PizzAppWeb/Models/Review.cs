using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.ComponentModel.DataAnnotations;

public class Review
{
    public int Id { get; set; }

    // Fiecare recenzie este asociată unei comenzi
    [Required]
    public int OrderId { get; set; }

    [BindNever]
    public Order Order { get; set; }

    // Rating de la 1 la 5
    [Range(1, 5, ErrorMessage = "Ratingul trebuie să fie între 1 și 5.")]
    public int Rating { get; set; }

    [StringLength(500)]
    public string Comment { get; set; }

    public DateTime ReviewDate { get; set; }
}
