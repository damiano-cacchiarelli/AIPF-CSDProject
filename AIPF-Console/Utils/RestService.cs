using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AIPF.MLManager.Metrics;
using Newtonsoft.Json;

namespace AIPF_Console.Utils
{
    public static class RestService
    {
        private static readonly HttpClient client = new HttpClient();
        private static readonly string url = "http://localhost:5000/api/mlmanager";

        public static async Task<T> Get<T>(string uri)
        {
            var response = await client.GetAsync($"{url}/{uri}");
            if (response.IsSuccessStatusCode)
            {
                var str = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(str);
            }
            return default(T);
        }

        public static async Task<T> Put<T>(string uri, object body)
        {
            var json = JsonConvert.SerializeObject(body);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"{url}/{uri}", data);
            if (response.IsSuccessStatusCode)
            {
                var str = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(str);
            }
            return default(T);
        }

        public static async Task<T> Post<T>(string uri, object body)
        {

            var json = JsonConvert.SerializeObject(body);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"{url}/{uri}", data);
            if (response.IsSuccessStatusCode)
            {
                var str = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(str);
            }
            return default(T);
        }

        public static async Task<Stream> PostStream(string uri, object body)
        {

            var json = JsonConvert.SerializeObject(body);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"{url}/{uri}", data);
            return await response.Content.ReadAsStreamAsync();
            //return new StreamReader(stream);
        }

        /*
        public static async Task<List<MetricContainer>> MetricsRestCall(object body)
        {
            var json = JsonConvert.SerializeObject(body);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PutAsync(url, data);
            if (response.IsSuccessStatusCode)
            {
                var str = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<MetricContainer>>(str);
            }
            return new List<MetricContainer>();
        }

        public static async Task<string> TrainRestCall(object body)
        {
            var json = JsonConvert.SerializeObject(body);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, data);
            if (response.IsSuccessStatusCode)
            {
                return "OK";
            }
            return "Error";
        }

        public static async Task<T> PredictRestCall<T>(string modelName, object body)
        {
            var json = JsonConvert.SerializeObject(body);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            var url = $"http://localhost:5000/api/mlmanager/predict/{modelName}";
            var response = await client.PutAsync(url + "", data);
            if (response.IsSuccessStatusCode)
            {
                var str = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(str);
            }
            return default(T);
        }
        */
    }
}
