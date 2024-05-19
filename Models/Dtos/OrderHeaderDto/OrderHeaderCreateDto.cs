using System.ComponentModel.DataAnnotations;
using MangoStore_API.Models.Dtos.OrderDetailsDto;

namespace MangoStore_API.Models.Dtos.OrderHeaderDto
{
    public class OrderHeaderCreateDto
    {
        [Required]
        public string PickupName { get; set; }
        [Required]
        public string PickupPhoneNumber { get; set; }
        [Required]
        public string PickupEmail { get; set; }
        public string UserId { get; set; }
        public double OrderTotal { get; set; }

        public string StripePaymentIntentId { get; set; }
        public string Status { get; set; }
        public int TotalItems { get; set; }

        public IEnumerable<OrderDetailsCreateDto> OrderDetailsDto { get; set; }
    }
}
