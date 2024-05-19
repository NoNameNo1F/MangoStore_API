using System.ComponentModel.DataAnnotations.Schema;

namespace MangoStore_API.Models
{
    public class CartItem
    {
        public int Id { get; set;}
        public int MenuItemId { get; set;}
        public int Quantity { get; set;}
        public int ShoppingCartId { get; set;}
        [ForeignKey("MenuItemId")]
        public MenuItem MenuItem {get; set;} = new();
    }
}
