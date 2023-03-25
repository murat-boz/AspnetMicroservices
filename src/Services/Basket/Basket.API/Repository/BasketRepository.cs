using Basket.API.Entities;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using RailwayExtensions;
using System.Threading.Tasks;

namespace Basket.API.Repository
{
    public class BasketRepository : IBasketRepository
    {
        private readonly IDistributedCache redisCache;

        public BasketRepository(IDistributedCache redisCache)
        {
            this.redisCache = redisCache;
        }

        public async Task<Result<ShoppingCart>> GetBasket(string userName)
        {
            var basket = await this.redisCache.GetStringAsync(userName);

            if (string.IsNullOrEmpty(basket))
            {
                return Result.Failure<ShoppingCart>("Basket is empty");
            }

            return Result.Create(JsonConvert.DeserializeObject<ShoppingCart>(basket));
        }

        public async Task<Result<ShoppingCart>> UpdateBasket(ShoppingCart basket)
        {
            await this.redisCache.SetStringAsync(basket.UserName, JsonConvert.SerializeObject(basket));
        
            return await this.GetBasket(basket.UserName);
        }

        public async Task<Result> DeleteBasket(string userName)
        {
            try
            {
                await this.redisCache.RemoveAsync(userName);
            }
            catch (System.Exception ex)
            {
                return Result.Failure($"Redis error has been thrown. Error message: {ex.Message}");
            }

            return Result.Ok();
        }
    }
}
