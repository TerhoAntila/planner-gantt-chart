using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using Microsoft.Net.Http.Headers;

namespace fnGanttChartRating
{
    public static class fnRatingImageSrc
    {
        [FunctionName("fnRatingImageSrc")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "rating-img/{userId}/rating.png")] HttpRequest req,
            ILogger log, string userId)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            log.LogInformation(userId);

            string imgUrl = "https://strganttchartratings.blob.core.windows.net/rating-public-images/stars-5.png";
            using (var client = new HttpClient())
            {
                var img = await client.GetStreamAsync(imgUrl);
                using (var ms = new MemoryStream())
                {
                    img.CopyTo(ms);
                    return new FileContentResult(ms.ToArray(), "image/png");
                }
            }
            

        }
    }
}
