namespace Ninimum.Models.Dto;

public class CartProductDto : ProductDto
{
    public int cart_id { get; set; }
    public int quantity { get; set; }
}