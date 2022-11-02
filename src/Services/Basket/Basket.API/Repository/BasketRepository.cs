using Basket.API.Entities;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
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

        public async Task<ShoppingCart> GetBasket(string userName)
        {
            var basket = await this.redisCache.GetStringAsync(userName);

            if (string.IsNullOrEmpty(basket))
            {
                return null;
            }

            return JsonConvert.DeserializeObject<ShoppingCart>(basket);
        }

        public async Task<ShoppingCart> UpdateBasket(ShoppingCart basket)
        {
            await this.redisCache.SetStringAsync(basket.UserName, JsonConvert.SerializeObject(basket));
        
            return await this.GetBasket(basket.UserName);
        }

        public async Task DeleteBasket(string userName)
        {
            await this.redisCache.RemoveAsync(userName);
        }
    }
}
