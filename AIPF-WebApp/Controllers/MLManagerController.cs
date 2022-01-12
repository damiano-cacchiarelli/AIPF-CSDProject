using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AIPF_WebApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AIPF_WebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MLManagerController : ControllerBase
    {
        private MLService mlService = new MLService();

        // fit
        // POST api/<MLManagerController>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public void Fit([FromBody] FitBody value)
        {
            mlService.Fit(value);
        }

        // predict
        [HttpPut("{id}")]
        public void Predict(int id, [FromBody] string value)
        {
        }

        // metrics
        // GET: api/<MLManagerController>
        [HttpGet]
        public IEnumerable<string> Metrics()
        {
            return new string[] { "value1", "value2" };
        }
    }
}
