using System.Collections.Generic;
using System.Text.Json;

namespace AIPF_RESTController.Models
{
    public class FitBody
    {
        public string ModelName { get; set; }
        public IList<JsonElement> Data { get; set; }

    }
}
