using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DoAnCS.Models.ViewModels;

namespace DoAnCS.Services
{
    public class MomoService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;

        public MomoService(IConfiguration config, HttpClient httpClient)
        {
            _config = config;
            _httpClient = httpClient;
        }

        public async Task<string> CreatePaymentUrl(string orderId, long amount, string orderInfo)
        {
            var partnerCode = _config["MoMo:PartnerCode"];
            var accessKey = _config["MoMo:AccessKey"];
            var secretKey = _config["MoMo:SecretKey"];
            var returnUrl = _config["MoMo:ReturnUrl"];
            var ipnUrl = _config["MoMo:IpnUrl"];
            var endpoint = _config["MoMo:PaymentUrl"];
            
            var requestId = Guid.NewGuid().ToString();
            var extraData = "";
            
            // Format bắt buộc của MoMo
            var rawHash = $"accessKey={accessKey}&amount={amount}&extraData={extraData}&ipnUrl={ipnUrl}&orderId={orderId}&orderInfo={orderInfo}&partnerCode={partnerCode}&redirectUrl={returnUrl}&requestId={requestId}&requestType=captureWallet";
            
            var signature = ComputeHmacSha256(rawHash, secretKey);

            var request = new MomoPaymentRequest
            {
                partnerCode = partnerCode,
                requestId = requestId,
                amount = amount,
                orderId = orderId,
                orderInfo = orderInfo,
                redirectUrl = returnUrl,
                ipnUrl = ipnUrl,
                signature = signature
            };

            var jsonRequest = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(endpoint, content);
            var responseString = await response.Content.ReadAsStringAsync();
            var momoResponse = JsonSerializer.Deserialize<MomoPaymentResponse>(responseString);

            if (momoResponse != null && momoResponse.resultCode == 0)
            {
                return momoResponse.payUrl;
            }
            throw new Exception($"Lỗi tạo thanh toán MoMo: {momoResponse?.message}");
        }

        public bool ValidateSignature(MomoIpnRequest request)
        {
            var accessKey = _config["MoMo:AccessKey"];
            var secretKey = _config["MoMo:SecretKey"];
            
            var rawHash = $"accessKey={accessKey}&amount={request.amount}&extraData={request.extraData}&message={request.message}&orderId={request.orderId}&orderInfo={request.orderInfo}&orderType={request.orderType}&partnerCode={request.partnerCode}&payType={request.payType}&requestId={request.requestId}&responseTime={request.responseTime}&resultCode={request.resultCode}&transId={request.transId}";

            var expectedSignature = ComputeHmacSha256(rawHash, secretKey);
            return request.signature == expectedSignature;
        }

        private string ComputeHmacSha256(string message, string secretKey)
        {
            byte[] keyByte = Encoding.UTF8.GetBytes(secretKey);
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                return BitConverter.ToString(hashmessage).Replace("-", "").ToLower();
            }
        }
    }
}
