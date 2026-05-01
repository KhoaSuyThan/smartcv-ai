using System.Text.Json.Serialization;

namespace DoAnCS.Models.ViewModels
{
    public class MomoPaymentRequest
    {
        public string partnerCode { get; set; } = string.Empty;
        public string requestId { get; set; } = string.Empty;
        public long amount { get; set; }
        public string orderId { get; set; } = string.Empty;
        public string orderInfo { get; set; } = string.Empty;
        public string redirectUrl { get; set; } = string.Empty;
        public string ipnUrl { get; set; } = string.Empty;
        public string requestType { get; set; } = "captureWallet";
        public string extraData { get; set; } = "";
        public string lang { get; set; } = "vi";
        public string signature { get; set; } = string.Empty;
    }

    public class MomoPaymentResponse
    {
        public string payUrl { get; set; } = string.Empty;
        public int resultCode { get; set; }
        public string message { get; set; } = string.Empty;
    }

    public class MomoIpnRequest
    {
        public string partnerCode { get; set; } = string.Empty;
        public string orderId { get; set; } = string.Empty;
        public string requestId { get; set; } = string.Empty;
        public long amount { get; set; }
        public string orderInfo { get; set; } = string.Empty;
        public string orderType { get; set; } = string.Empty;
        public long transId { get; set; }
        public int resultCode { get; set; }
        public string message { get; set; } = string.Empty;
        public string payType { get; set; } = string.Empty;
        public long responseTime { get; set; }
        public string extraData { get; set; } = string.Empty;
        public string signature { get; set; } = string.Empty;
    }
}
