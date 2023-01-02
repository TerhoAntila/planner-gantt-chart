using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fnGanttChartRating
{
    internal class TableHelper
    {
        private ILogger log;
        private TableServiceClient serviceClient;
        private TableClient averageClient;

        public TableHelper(string storageUri, string accountName, string storageAccountKey, ILogger log)
        {
            this.log = log;

            serviceClient = new TableServiceClient(
                new Uri(storageUri),
                new TableSharedKeyCredential(accountName, storageAccountKey));

            averageClient = new TableClient(
                new Uri(storageUri),
                "Average",
                new TableSharedKeyCredential(accountName, storageAccountKey));

            log.LogInformation("TableHelper initialized");
        }

        public void Rate(int stars)
        {
            int count;
            var oldAverage = GetAverage(out count);
            var oldM = oldAverage * count;
            count++;
            var newAverage = (double)(oldM + stars) / count;

            averageClient.DeleteEntity("", "average");
            var averEnt = new TableEntity("", "average");
            averEnt.Add("Average", newAverage);
            averEnt.Add("Count", count);
            averageClient.AddEntity(averEnt);
        }

        private double GetAverage(out int count)
        {
            var a = averageClient.GetEntity<TableEntity>("", "average").Value;
            count = a.GetInt32("Count").Value;
            return a.GetDouble("Average").Value;
        }

        public double GetAverage()
        {
            var a = averageClient.GetEntity<TableEntity>("", "average").Value;
            return a.GetDouble("Average").Value;
        }
    }
}
