using System.Net;
using MangoStore_API.Data;
using MangoStore_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace MangoStore_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private ApiResponse response;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _db;
        public PaymentController(ApiResponse response, IConfiguration configuration, ApplicationDbContext db)
        {
            _configuration = configuration;
            _db = db;
            this.response = response;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse>> MakePayment(string userId)
        {
            ShoppingCart shoppingCart = _db.ShoppingCarts
                .Include(u => u.CartItems)
                .ThenInclude(u => u.MenuItem)
                .FirstOrDefault(u =>u.UserId == userId);

            if(shoppingCart == null || shoppingCart.CartItems == null)
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                response.IsSuccess = false;
                return BadRequest(response);
            }

            #region Create Payment Intent

            StripeConfiguration.ApiKey = _configuration["StripeSettings:Secret"];
            shoppingCart.CartTotal = shoppingCart.CartItems.Sum(u => u.Quantity * u.MenuItem.Price);

            PaymentIntentCreateOptions options = new()
            {
                Amount = (int)(shoppingCart.CartTotal * 100),
                Currency = "usd",
                PaymentMethodTypes = new List<string>
                {
                    "card",
                }
                // AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                // {
                //     Enabled = true,
                // },
            };
            PaymentIntentService service = new();
            PaymentIntent res = service.Create(options);
            shoppingCart.StripePaymentIntentId = res.Id;
            shoppingCart.ClientSecret = res.ClientSecret; // use to make actual payment
            #endregion

            response.Result = shoppingCart;
            response.StatusCode = HttpStatusCode.OK;
            return Ok(response);
        }

    }
}
