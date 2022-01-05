using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using keys_collector.Models;
using keys_collector.Services;
using Microsoft.AspNetCore.Mvc;
using Octokit.Internal;

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


        //  ------------SIMPLE EXAMPLE OF EVENT REQUEST
        // ----------DELETE IT LATER
        //[HttpGet]
        //public async Task Get()
        //{
        //    string[] data = new string[] {
        //        "Hello World!",
        //        "Hello Galaxy!",
        //        "Hello Universe!"
        //    };

        //    Response.Headers.Add("Content-Type", "text/event-stream");

        //    for (int i = 0; i < data.Length; i++)
        //    {
        //        await Task.Delay(TimeSpan.FromSeconds(5));
        //        string dataItem = $"data: {data[i]}\n\n";
        //        byte[] dataItemBytes = ASCIIEncoding.ASCII.GetBytes(dataItem);
        //        await Response.Body.WriteAsync(dataItemBytes, 0, dataItemBytes.Length);
        //        await Response.Body.FlushAsync();
        //    }
        //}

        [HttpPost]
        public async Task<IActionResult> IndexAsync(RequestModel requestModel)
        {
            var header = Request.Headers.FirstOrDefault(h => h.Key.Equals("Authorization"));
            var res = await _service.GetPage(header.Value, requestModel.Keyword, requestModel.PageNumbers, requestModel.Language);
            if (res != default)
            {
                return Ok(res);
            }
            return BadRequest("Non-existent language entered. Choose another one.");
        }
        [HttpPost("connect")]
        public IActionResult EstablishConnections(RequestModel requestModel)
        {
            var header = Request.Headers.FirstOrDefault(h => h.Key.Equals("Authorization"));
            _service.EstablishConnections(header.Value, requestModel);
            Response.Headers.Add("Content-Type", "text/event-stream");
            return Ok();
        }

        [HttpGet]
        async public Task GetNewRepositoryResultLogs()
        {
            List<RepositoryResult> data = _service.NewRepositoryResultsLogger;

            Response.Headers.Add("Content-Type", "text/event-stream");

            for (int i = 0; i < data.Count; i++)
            {
                await Task.Delay(TimeSpan.FromSeconds(5));
                string dataItem = $"data: {data[i]}\n\n";
                byte[] dataItemBytes = ASCIIEncoding.ASCII.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(dataItem));
                await Response.Body.WriteAsync(dataItemBytes, 0, dataItemBytes.Length);
                await Response.Body.FlushAsync();
            }
        }
    }
}
