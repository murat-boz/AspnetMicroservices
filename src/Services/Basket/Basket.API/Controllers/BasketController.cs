using AutoMapper;
using Basket.API.Entities;
using Basket.API.Repository;
using EventBus.Messages.Events;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using RailwayExtensions;
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
            Result<ShoppingCart> basketOrNothing = await this.basketRepository.GetBasket(userName);

            if (basketOrNothing.IsFailure)
            {
                return NoContent();
            }

            return Ok(basketOrNothing.Value ?? new ShoppingCart(userName));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ShoppingCart), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ShoppingCart>> UpdateBasket([FromBody] ShoppingCart basket)
        {
            Result<ShoppingCart> basketOrNothing = await this.basketRepository.UpdateBasket(basket);

            if (basketOrNothing.IsFailure)
            {
                return NoContent();
            }

            return Ok(basketOrNothing.Value);
        }

        [HttpDelete("{userName}", Name = "DeleteBasket")]
        [ProducesResponseType(typeof(ShoppingCart), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ShoppingCart>> DeleteBasket(string userName)
        {
            Result result = await this.basketRepository.DeleteBasket(userName);

            if (result.IsFailure)
            {
                return NoContent();
            }

            return Ok();
        }

        [Route("[action]")]
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult> Checkout([FromBody]BasketCheckoutEvent basketCheckoutEvent)
        {
            Result<ShoppingCart> basketOrNothing = await this.basketRepository.GetBasket(basketCheckoutEvent.UserName);

            if (basketOrNothing.IsFailure)
            {
                return BadRequest();
            }

            var eventMessage = this.mapper.Map<BasketCheckoutEvent>(basketCheckoutEvent);
            eventMessage.TotalPrice = basketOrNothing.Value.TotalPice;

            await this.publishEndpoint.Publish(eventMessage);

            await this.basketRepository.DeleteBasket(basketOrNothing.Value.UserName);

            return Accepted();
        }
    }
}
