using System.ComponentModel.DataAnnotations;
using MangoStore_API.Models.Dtos.OrderDetailsDto;

namespace MangoStore_API.Models.Dtos.OrderHeaderDto
{
    public class OrderHeaderUpdateDto
    {
        public int OrderHeaderId { get; set; }
        public string PickupName { get; set; }
        public string PickupPhoneNumber { get; set; }
        public string PickupEmail { get; set; }
        public string StripePaymentIntentId { get; set; }
        public string Status { get; set; }
    }
}
