using HRM_API.Models.Enums;

namespace HRM_API.Dtos
{
    public class UpdateRequestStatusRequest
    {
        public int RequestId { get; set; }
        public RequestStatus Status { get; set; }
    }
}

