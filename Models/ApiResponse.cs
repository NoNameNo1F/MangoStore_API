using System.Net;

namespace MangoStore_API.Models
{
    public class ApiResponse
    {
        public HttpStatusCode StatusCode {get; set; }
        public bool IsSuccess {get; set; } = true;
        public List<string> ErrorMessages {get; set; } = new List<string>();
        public object Result {get; set; }
    }
}
