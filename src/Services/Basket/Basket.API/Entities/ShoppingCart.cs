using System.Collections.Generic;

namespace Basket.API.Entities
{
    public class ShoppingCart
    {
        public string UserName { get; set; }
        public List<ShoppingCartItem> Items { get; set; } = new List<ShoppingCartItem>();

        public decimal TotalPice
        {
            get
            {
                decimal totalPrice = 0;

                foreach (var item in this.Items)
                {
                    totalPrice += item.Price * item.Quantity;
                }

                return totalPrice;
            }
        }

        public ShoppingCart()
        {

        }

        public ShoppingCart(string userName)
        {
            this.UserName = userName;
        }
    }
}
