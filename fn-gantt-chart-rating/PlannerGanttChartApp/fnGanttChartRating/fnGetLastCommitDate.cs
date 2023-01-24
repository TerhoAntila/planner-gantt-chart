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
using Microsoft.Extensions.Azure;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace fnGanttChartRating
{
    public static class fnGetLastCommitDate
    {
        [FunctionName("fnGetLastCommitDate")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", 
            Route = "GetLastCommitDate/{cacheInvalidator}/{charIndex}/img.jpg")] HttpRequest req,
            int charIndex,
            ILogger log)
        {
            if (charIndex > 7)
            {
                log.LogInformation("char index:" + charIndex);
                log.LogInformation("End of message -> E");
                return GetImageResultForCharacter('E'); // As in End
            }

            string patToken = Environment.GetEnvironmentVariable("GitHubPatToken");

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + patToken);
                client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
                client.DefaultRequestHeaders.Add("User-Agent", "terhoantila");

                var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get,
                    "https://api.github.com/repos/TerhoAntila/planner-gantt-chart/commits?path=PlannerGanttChart.zip"));
                using (var ms = new MemoryStream())
                {
                    await response.Content.CopyToAsync(ms);
                    ms.Position = 0;
                    using (StreamReader reader = new StreamReader(ms))
                    {
                        var commit = JArray.Parse(reader.ReadToEnd()).First;
                        var date = (DateTime?) commit["commit"]["author"]["date"];
                        var dateStr = date.Value.ToString("yyyyMMdd");
                        
                        log.LogInformation("Date:" + dateStr);
                        log.LogInformation("Char index:" + charIndex);
                        var c = dateStr[charIndex];
                        log.LogInformation("Char:" + c);

                        return GetImageResultForCharacter(c);
                    }
                }
            }
        }

        private static FileContentResult GetImageResultForCharacter(char v)
        {
            var returnCode = GetCodeToMatchHeight(v);
            var img = new System.Drawing.Bitmap(1, returnCode);
            using (var ms = new MemoryStream())
            {
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                return new FileContentResult(ms.ToArray(), "img/jpg");
            }
        }

        private static int GetCodeToMatchHeight(int desiredHeight)
        {
            // 26 -> height 32
            return desiredHeight - 6;
        }
    }
}
