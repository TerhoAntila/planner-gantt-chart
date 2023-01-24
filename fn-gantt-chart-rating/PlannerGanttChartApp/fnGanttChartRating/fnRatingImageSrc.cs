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
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "rating-img/{guid}/{rating}/rating.png")] HttpRequest req,
            ILogger log, string guid, int rating)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            log.LogInformation(guid);
            log.LogInformation($"{rating}");

            try
            {
                var tableHelper = new TableHelper(
                    System.Environment.GetEnvironmentVariable("StorageUri"),
                    System.Environment.GetEnvironmentVariable("StorageAccount"),
                    System.Environment.GetEnvironmentVariable("StorageKey"),
                    log
                    );

                if (rating > 0)
                {
                    tableHelper.Rate(rating);
                }

                var averRating = tableHelper.GetAverage();
                var averRounded = (int)Math.Round(averRating, 0);

                string imgUrl = $"https://strganttchartratings.blob.core.windows.net/rating-public-images/stars-{averRounded}.png";
                log.LogInformation(imgUrl);
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
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                throw;
            }           

        }
    }
}
