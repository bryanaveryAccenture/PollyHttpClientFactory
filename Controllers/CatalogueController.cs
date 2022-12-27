using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Polly;
using System.Net.Http;
using System.Threading.Tasks;

namespace PollyHttpClientFactory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CatalogueController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger _logger;

        public CatalogueController(IHttpClientFactory httpClientFactory,
            ILoggerFactory loggerFactory)
        {
            _httpClientFactory = httpClientFactory;
            _logger = loggerFactory.CreateLogger("LoggerKey");
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> Get(int id)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"inventory/{id}");
           
            request.SetPolicyExecutionContext(new Context().WithLogger(_logger));

            var httpClient = _httpClientFactory.CreateClient("RemoteServer");

            HttpResponseMessage response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var itemsInStock = await response.Content.ReadAsStringAsync();
                return Ok(itemsInStock);
            }

            return StatusCode((int)response.StatusCode, response.Content.ReadAsStringAsync());
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, $"inventory/{id}");

            request.SetPolicyExecutionContext(new Context().WithLogger(_logger));

            var httpClient = _httpClientFactory.CreateClient("RemoteServer");

            HttpResponseMessage response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return Ok();
            }

            return StatusCode((int)response.StatusCode, response.Content.ReadAsStringAsync());
        }

        [HttpPost("{id}")]
        public async Task<ActionResult> Post(int id)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, string.Format("inventory/{id}", id));

            request.SetPolicyExecutionContext(new Context().WithLogger(_logger));

            var httpClient = _httpClientFactory.CreateClient("RemoteServer");

            HttpResponseMessage response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return Ok();
            }

            return StatusCode((int)response.StatusCode, response.Content.ReadAsStringAsync());
        }
    }
}