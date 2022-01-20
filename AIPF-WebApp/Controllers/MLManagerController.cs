using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AIPF_RESTController.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AIPF.MLManager.Metrics;
using System.IO;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AIPF_RESTController.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MLManagerController : ControllerBase
    {
        private static MLService mlService = new MLService();

        // fit
        // POST api/<MLManagerController>
        [HttpPost("train")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public void Fit([FromBody] FitBody value)
        {
            mlService.Fit(value);
        }

        // predict
        [HttpPut("predict/{id}")]
        public object Predict(string id, [FromBody] JsonElement value)
        {
            return mlService.Predict(id, value);
        }

        // metrics
        // GET: api/<MLManagerController>
        [HttpPut("metrics")]
        public IEnumerable<MetricContainer> Metrics([FromBody] FitBody value)
        {
            return mlService.Metrics(value);
        }
    }
}
