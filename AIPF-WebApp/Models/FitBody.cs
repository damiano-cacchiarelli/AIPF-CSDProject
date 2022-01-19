using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace AIPF_RESTController.Models
{
    public class FitBody
    {
        public string ModelName { get; set; }
        public IList<JsonElement> Data { get; set; }

    }
}
