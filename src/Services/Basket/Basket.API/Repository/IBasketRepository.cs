using Basket.API.Entities;
using RailwayExtensions;
using System.Threading.Tasks;

namespace Basket.API.Repository
{
    public interface IBasketRepository
    {
        Task<Result<ShoppingCart>> GetBasket(string userName);
        Task<Result<ShoppingCart>> UpdateBasket(ShoppingCart basket);
        Task<Result> DeleteBasket(string userName);
    }
}
