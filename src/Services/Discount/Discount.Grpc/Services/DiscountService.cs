using AutoMapper;
using Discount.Grpc.Entities;
using Discount.Grpc.Protos;
using Discount.Grpc.Repositories;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Discount.Grpc.Services
{
    public class DiscountService : DiscountProtoService.DiscountProtoServiceBase
    {
        private readonly IDiscountRepository discountRepository;
        private readonly IMapper mapper;
        private readonly ILogger<DiscountService> logger;

        public DiscountService(IDiscountRepository discountRepository, IMapper mapper, ILogger<DiscountService> logger)
        {
            this.discountRepository = discountRepository ?? throw new ArgumentNullException(nameof(discountRepository));
            this.mapper             = mapper             ?? throw new ArgumentNullException(nameof(mapper));
            this.logger             = logger             ?? throw new ArgumentNullException(nameof(logger));
        }

        public override async Task<CouponModel> GetDiscount(GetDiscountRequest request, ServerCallContext context)
        {
            Coupon coupon = await this.discountRepository.GetDiscount(request.ProductName);

            if (coupon == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Discount with ProductName={request.ProductName} is not found."));
            }

            this.logger.LogInformation("Discount is retrieved for ProductName : {productName}, Amount : {amount}", coupon.ProductName, coupon.Amount);


            return this.mapper.Map<CouponModel>(coupon);
        }

        public override async Task<CouponModel> CreateDiscount(CreateDiscountRequest request, ServerCallContext context)
        {
            Coupon coupon = this.mapper.Map<Coupon>(request.Coupon);

            await this.discountRepository.CreateDiscount(coupon);

            this.logger.LogInformation("Discount is successfully created. ProductName : {ProductName}", coupon.ProductName);

            return this.mapper.Map<CouponModel>(coupon);
        }

        public override async Task<CouponModel> UpdateDiscount(UpdateDiscountRequest request, ServerCallContext context)
        {
            var coupon = this.mapper.Map<Coupon>(request.Coupon);

            await this.discountRepository.UpdateDiscount(coupon);

            this.logger.LogInformation("Discount is successfully updated. ProductName : {ProductName}", coupon.ProductName);

            var couponModel = this.mapper.Map<CouponModel>(coupon);

            return couponModel;
        }

        public override async Task<DeleteDiscountResponse> DeleteDiscount(DeleteDiscountRequest request, ServerCallContext context)
        {
            var deleted = await this.discountRepository.DeleteDiscount(request.ProductName);

            var response = new DeleteDiscountResponse
            {
                Success = deleted
            };

            return response;
        }
    }
}
