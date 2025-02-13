using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;



namespace _233763P_BookwormsOnline.Services
{
    public class ReCaptchaService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public ReCaptchaService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<bool> VerifyTokenAsync(string token)
        {
            var secretKey = _configuration["GoogleReCaptcha:SecretKey"];
            var httpClient = _httpClientFactory.CreateClient();

            var response = await httpClient.PostAsync($"https://www.google.com/recaptcha/api/siteverify?secret={secretKey}&response={token}", null);
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ReCaptchaResponse>(jsonResponse);

            return result.Success && result.Score >= 0.5; // Pass if score ≥ 0.5
        }

        private class ReCaptchaResponse
        {
            public bool Success { get; set; }
            public double Score { get; set; }
            public string Action { get; set; }
        }
    }
}
