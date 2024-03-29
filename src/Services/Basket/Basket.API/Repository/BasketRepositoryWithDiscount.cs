﻿using Basket.API.Entities;
using Basket.API.GrpcServices;
using RailwayExtensions;
using System;
using System.Threading.Tasks;

namespace Basket.API.Repository
{
    public class BasketRepositoryWithDiscount : IBasketRepository
    {
        private readonly IBasketRepository basketRepository;
        private readonly DiscountGrpcService discountGrpcService;

        public BasketRepositoryWithDiscount(
            Func<BasketType, IBasketRepository> basketRepositoryDelegate, 
            DiscountGrpcService discountGrpcService)
        {
            this.basketRepository    = basketRepositoryDelegate(BasketType.Default);
            this.discountGrpcService = discountGrpcService;
        }

        public async Task<Result<ShoppingCart>> GetBasket(string userName)
        {
            return await this.basketRepository.GetBasket(userName);
        }

        public async Task<Result<ShoppingCart>> UpdateBasket(ShoppingCart basket)
        {
            foreach (var item in basket.Items)
            {
                var coupon = await this.discountGrpcService.GetDiscount(item.ProductName);

                item.Price -= coupon.Amount;
            }

            return await this.basketRepository.UpdateBasket(basket);
        }

        public async Task<Result> DeleteBasket(string userName)
        {
            return await this.basketRepository.DeleteBasket(userName);
        }
    }
}
