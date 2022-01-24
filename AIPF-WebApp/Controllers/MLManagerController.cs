using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using AIPF_RESTController.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AIPF.MLManager.Metrics;
using System.IO;
using AIPF.MLManager.EventQueue;
using System.Threading;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AIPF_RESTController.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MLManagerController : ControllerBase
    {
        private readonly MLService MLService;
        private readonly MessageQueue<double> MessageQueue;

        public MLManagerController(MessageQueue<double> messageQueue)
        {
            MLService = new MLService(messageQueue);
            MessageQueue = messageQueue;

        }

        // fit
        // POST api/<MLManagerController>
        [HttpPost("train")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task Fit([FromBody] FitBody value)
        {
            Response.ContentType = "text/event-stream";
            Response.StatusCode = 200;

            StreamWriter streamWriter = new StreamWriter(Response.Body);

            //MessageQueue.Register(id);
            var taskFit = MLService.Fit(value);

            try
            {
                //await MessageQueue.EnqueueAsync(id, $"Subscribed to id {id}", HttpContext.RequestAborted);

                await foreach (var percentage in MessageQueue.DequeueAsync(@"Process#1", HttpContext.RequestAborted))
                {
                    await streamWriter.WriteLineAsync($"{percentage}");
                    await streamWriter.FlushAsync();
                    if (percentage >= 1) break;
                }

                await taskFit;
            }
            catch (OperationCanceledException)
            {
                //this is expected when the client disconnects the connection
            }
            catch (Exception)
            {
                Response.StatusCode = 400;
            }
            finally
            {
                MessageQueue.Unregister(@"Process#1");
            }
        }

        // predict
        [HttpPut("predict/{id}")]
        public object Predict(string id, [FromBody] JsonElement value)
        {
            return MLService.Predict(id, value);
        }

        // metrics
        // GET: api/<MLManagerController>
        [HttpPut("metrics")]
        public IEnumerable<MetricContainer> Metrics([FromBody] FitBody value)
        {
            return MLService.Metrics(value).Result;
        }

        [HttpGet]
        [Route("testSSE/{id}")]
        public async Task Subscribe(string id)
        {
            Response.ContentType = "text/event-stream";
            Response.StatusCode = 200;

            StreamWriter streamWriter = new StreamWriter(Response.Body);

            //MessageQueue.Register(id);

            try
            {
                //await MessageQueue.EnqueueAsync(id, $"Subscribed to id {id}", HttpContext.RequestAborted);

                await foreach (var progress in MessageQueue.DequeueAsync(@"Process#1", HttpContext.RequestAborted))
                {
                    await streamWriter.WriteLineAsync(progress.ToString());
                    await streamWriter.FlushAsync();
                }
            }
            catch (OperationCanceledException)
            {
                //this is expected when the client disconnects the connection
            }
            catch (Exception)
            {
                Response.StatusCode = 400;
            }
            finally
            {
                MessageQueue.Unregister(id);
            }
        }

        [HttpGet]
        [Route("messages/sse/{id}")]
        public async Task SimpleSSE(string id)
        {
            //1. Set content type
            Response.ContentType = "text/event-stream";
            Response.StatusCode = 200;

            StreamWriter streamWriter = new StreamWriter(Response.Body);
            int i = 0;
            while (!HttpContext.RequestAborted.IsCancellationRequested && i < 10)
            {
                //2. Await something that generates messages
                await Task.Delay(1000);

                //3. Write to the Response.Body stream
                await streamWriter.WriteLineAsync($"{DateTime.Now} Looping");
                await streamWriter.FlushAsync();
                i++;

            }
        }

    }
}
