using System.Net;
using MangoStore_API.Data;
using MangoStore_API.Models;
using MangoStore_API.Models.Dtos.OrderDetailsDto;
using MangoStore_API.Models.Dtos.OrderHeaderDto;
using MangoStore_API.Services.BlobService;
using MangoStore_API.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MangoStore_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private ApiResponse response;
        public OrderController(ApplicationDbContext db, ApiResponse response)
        {
            _db = db;
            this.response = response;
        }
        [HttpGet]
        public async Task<ActionResult<ApiResponse>> GetOrders(string? userId)
        {
            try
            {
                var orderHeaders = _db.OrderHeaders.Include(u=>u.OrderDetails).ThenInclude(u=>u.MenuItem).OrderByDescending(u=>u.OrderHeaderId);
                if(!string.IsNullOrEmpty(userId))
                {
                    response.Result = orderHeaders.Where(u=>u.UserId == userId);
                }
                else
                {
                    response.Result = orderHeaders;
                }
                response.StatusCode = HttpStatusCode.OK;
                return Ok(response);
            }
            catch(Exception e)
            {
                response.IsSuccess = false;
                response.ErrorMessages = new List<string>()
                {
                    e.ToString()
                };
            }
            return response;
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse>> GetOrders(int id)
        {
            try
            {
                if(id == 0)
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(response);
                }

                var orderHeaders = _db.OrderHeaders.Include(u=>u.OrderDetails).ThenInclude(u=>u.MenuItem).Where(u=>u.OrderHeaderId == id);
                if(orderHeaders == null)
                {
                    response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(response);
                }
                response.Result = orderHeaders;
                response.StatusCode = HttpStatusCode.OK;
                return Ok(response);
            }
            catch(Exception e)
            {
                response.IsSuccess = false;
                response.ErrorMessages = new List<string>()
                {
                    e.ToString()
                };
            }
            return response;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse>> CreateOrder([FromBody] OrderHeaderCreateDto orderRequest)
        {
            try
            {
                OrderHeader order = new()
                {
                    UserId = orderRequest.UserId,
                    PickupEmail = orderRequest.PickupEmail,
                    PickupName = orderRequest.PickupName,
                    PickupPhoneNumber = orderRequest.PickupPhoneNumber,
                    OrderTotal = orderRequest.OrderTotal,
                    OrderDate = DateTime.Now,
                    StripePaymentIntentId = orderRequest.StripePaymentIntentId,
                    TotalItems = orderRequest.TotalItems,
                    Status = String.IsNullOrEmpty(orderRequest.Status)? SD.status_pending : orderRequest.Status
                };

                if(ModelState.IsValid)
                {
                    _db.OrderHeaders.Add(order);
                    _db.SaveChanges();
                    foreach(var orderDetailDto in orderRequest.OrderDetailsDto)
                    {
                        OrderDetails orderDetails = new()
                        {
                            OrderHeaderId = order.OrderHeaderId,
                            ItemName = orderDetailDto.ItemName,
                            MenuItemId = orderDetailDto.MenuItemId,
                            Price = orderDetailDto.Price,
                            Quantity = orderDetailDto.Quantity
                        };
                        _db.OrderDetails.Add(orderDetails);
                    }
                    _db.SaveChanges();
                    response.Result = order;
                    order.OrderDetails = null;
                    response.StatusCode = HttpStatusCode.Created;
                    return Ok(response);
                }
            }
            catch(Exception e)
            {
                response.IsSuccess = false;
                response.ErrorMessages = new List<string>()
                {
                    e.ToString()
                };
            }
            return response;
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponse>> UpdateOrderHeader(int id, [FromBody] OrderHeaderUpdateDto orderRequest)
        {
            try
            {
                if(orderRequest == null || id != orderRequest.OrderHeaderId)
                {
                    return BadRequest();
                }

                OrderHeader order = _db.OrderHeaders.FirstOrDefault(u => u.OrderHeaderId == id);

                if(order == null)
                {
                    response.IsSuccess = false;
                    response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest();
                }

                if(!string.IsNullOrEmpty(orderRequest.PickupName))
                {
                    order.PickupName = orderRequest.PickupName;
                }
                if(!string.IsNullOrEmpty(orderRequest.PickupEmail))
                {
                    order.PickupEmail = orderRequest.PickupEmail;
                }
                if(!string.IsNullOrEmpty(orderRequest.PickupPhoneNumber))
                {
                    order.PickupPhoneNumber = orderRequest.PickupPhoneNumber;
                }
                if(!string.IsNullOrEmpty(orderRequest.Status))
                {
                    order.Status = orderRequest.Status;
                }
                if(!string.IsNullOrEmpty(orderRequest.StripePaymentIntentId))
                {
                    order.StripePaymentIntentId = orderRequest.StripePaymentIntentId;
                }
                _db.SaveChanges();
                response.StatusCode = HttpStatusCode.NoContent;
                response.IsSuccess = true;
                return Ok(response);
            }
            catch(Exception e)
            {
                response.IsSuccess = false;
                response.ErrorMessages = new List<string>()
                {
                    e.ToString()
                };
            }
            return response;
        }
    }
}
