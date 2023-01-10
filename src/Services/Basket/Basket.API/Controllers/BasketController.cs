using AutoMapper;
using Basket.API.Entities;
using Basket.API.Repository;
using EventBus.Messages.Events;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Basket.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class BasketController : ControllerBase
    {
        private readonly IBasketRepository basketRepository;
        private readonly IPublishEndpoint publishEndpoint;
        private readonly IMapper mapper;

        public BasketController(Func<BasketType, IBasketRepository> basketRepositoryDelegate, IPublishEndpoint publishEndpoint, IMapper mapper)
        {
            this.basketRepository = basketRepositoryDelegate(BasketType.WithDiscount);
            this.publishEndpoint  = publishEndpoint;
            this.mapper           = mapper;
        }

        [HttpGet("{userName}", Name = "GetBasket")]
        [ProducesResponseType(typeof(ShoppingCart), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ShoppingCart>> GetBasket(string userName)
        {
            var basket = await this.basketRepository.GetBasket(userName);

            return Ok(basket ?? new ShoppingCart(userName));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ShoppingCart), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ShoppingCart>> UpdateBasket([FromBody] ShoppingCart basket)
        {
            return Ok(await this.basketRepository.UpdateBasket(basket));
        }

        [HttpDelete("{userName}", Name = "DeleteBasket")]
        [ProducesResponseType(typeof(ShoppingCart), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ShoppingCart>> DeleteBasket(string userName)
        {
            await this.basketRepository.DeleteBasket(userName);

            return Ok();
        }

        [Route("[action]")]
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult> Checkout([FromBody]BasketCheckoutEvent basketCheckoutEvent)
        {
            var basket = await this.basketRepository.GetBasket(basketCheckoutEvent.UserName);

            if (basket == null)
            {
                return BadRequest();
            }

            var eventMessage = this.mapper.Map<BasketCheckoutEvent>(basketCheckoutEvent);
            eventMessage.TotalPrice = basket.TotalPice;

            await this.publishEndpoint.Publish(eventMessage);

            await this.basketRepository.DeleteBasket(basket.UserName);

            return Accepted();
        }
    }
}
