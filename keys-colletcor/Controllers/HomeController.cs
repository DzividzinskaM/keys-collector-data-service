using System.Threading.Tasks;
using keys_collector.Models;
using keys_collector.Services;
using Microsoft.AspNetCore.Mvc;

namespace keys_collector.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HomeController : ControllerBase
    {

        private readonly GithubService _service;

        public HomeController(GithubService service)
        {
            _service = service;
        } 

        [HttpPost]
        public async Task<IActionResult> IndexAsync(RequestModel requestModel)
        {
            var res = await _service.GetPage(requestModel.Keyword, requestModel.PageNumbers, requestModel.Language);
            return Ok(res);
        }
    }
}
