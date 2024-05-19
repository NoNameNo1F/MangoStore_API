using System.Net;
using MangoStore_API.Data;
using MangoStore_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MangoStore_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShoppingCartController : ControllerBase
    {
        private ApiResponse response;
        private readonly ApplicationDbContext _db;

        public ShoppingCartController(ApiResponse response, ApplicationDbContext db)
        {
            this.response = response;
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse>> GetShoppingCart(string userId)
        {
            try
            {
                ShoppingCart shoppingCart;
                if(string.IsNullOrEmpty(userId))
                {
                    shoppingCart = new();
                    // response.IsSuccess = false;
                    // response.StatusCode = HttpStatusCode.BadRequest;
                    // return BadRequest(response);
                }
                else
                {
                    shoppingCart = _db.ShoppingCarts
                    .Include(u=>u.CartItems)
                    .ThenInclude(u=>u.MenuItem)
                    .FirstOrDefault(u => u.UserId == userId);

                }

                if(shoppingCart.CartItems != null && shoppingCart.CartItems.Count > 0)
                {
                    shoppingCart.CartTotal = shoppingCart.CartItems.Sum(u=>u.Quantity*u.MenuItem.Price);
                }
                response.Result = shoppingCart;
                response.StatusCode = HttpStatusCode.OK;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.ErrorMessages = new List<string>() {
                    ex.ToString(),
                };
                response.StatusCode = HttpStatusCode.BadRequest;
            }
            return response;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse>> AddOrUpdateItemInCart(string userId, int menuItemId, int updateQuantityBy)
        {
             // Shopping cart will have one entry per user id, even if a user has many items in cart.
            // Cart items will have all the items in shopping cart for a user
            // updatequantityby will have count by with an items quantity needs to be updated
            // if it is -1 that means we have lower a count if it is 5 it means we have to add 5 count to existing count.
            // if updatequantityby by is 0, item will be removed


            // when a user adds a new item to a new shopping cart for the first time
            // when a user adds a new item to an existing shopping cart (basically user has other items in cart)
            // when a user updates an existing item count
            // when a user removes an existing item
            ShoppingCart shoppingCart = _db.ShoppingCarts.Include(u => u.CartItems).FirstOrDefault(u =>u.UserId == userId);
            MenuItem menuItem = _db.MenuItems.FirstOrDefault(u =>u.Id == menuItemId);

            if(menuItem == null)
            {
                // ko ton tai item
                response.StatusCode = HttpStatusCode.BadRequest;
                response.IsSuccess = false;
                return BadRequest(response);
            }

            if(shoppingCart == null && updateQuantityBy > 0)
            {
                // create a shopping cart & update cart item
                ShoppingCart newCart = new()
                {
                    UserId = userId
                };

                _db.ShoppingCarts.Add(newCart);
                _db.SaveChanges();

                CartItem newCartItem = new()
                {
                    MenuItemId = menuItemId,
                    Quantity = updateQuantityBy,
                    ShoppingCartId = newCart.Id,
                    MenuItem=null
                };
                _db.CartItems.Add(newCartItem);
                _db.SaveChanges();

            }
            else
            {
                // existing cart
                // CartItem cartIteminDb = _db.CartItems.FirstOrDefault( u => u.MenuItemId == menuItemId);
                CartItem cartItemInCart = shoppingCart.CartItems.FirstOrDefault(u=> u.MenuItemId == menuItemId);
                if (cartItemInCart == null)
                {
                    // item does not exist in current cart
                    CartItem newCartItem = new()
                    {
                        MenuItemId = menuItemId,
                        Quantity = updateQuantityBy,
                        ShoppingCartId = shoppingCart.Id,
                        MenuItem = null
                    };
                    _db.CartItems.Add(newCartItem);
                    _db.SaveChanges();
                }
                else
                {
                    //item exists in cart , we gonna update
                    int newQuantity = cartItemInCart.Quantity + updateQuantityBy;
                    if(updateQuantityBy == 0 || newQuantity <= 0)
                    {
                        //remove cart item from cartt and if it is the only item
                        //then remove cart
                        _db.CartItems.Remove(cartItemInCart);
                        if(shoppingCart.CartItems.Count() == 0)
                        {
                            _db.ShoppingCarts.Remove(shoppingCart);
                        }
                        _db.SaveChanges();
                    }
                    else
                    {
                        cartItemInCart.Quantity = newQuantity;
                        _db.SaveChanges();
                    }
                }
            }
            return response;
        }
    }
}
