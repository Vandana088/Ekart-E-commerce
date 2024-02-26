using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Configuration;
using Microsoft.Azure.WebJobs.Host;
using System.Net.Http;
using System.Net;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace ProjectDemo
{
    public static class Permission
    {
        [FunctionName("PermissionApp")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var successful = true;
            try
            {
                var config = new ConfigurationBuilder()
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

                string cnnString = config["ConnectionStrings:sqlcon"];

                using (var connection = new SqlConnection(cnnString))
                {
                    connection.Open();
                    log.LogInformation("C# HTTP trigger function processed a request.");
                    string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                    dynamic data = JsonConvert.DeserializeObject(requestBody);
                    int? supplierId = data?.supplierId;
                    string status = data?.status;
                    var modifiedOn = DateTime.Now.ToString();

                    // insert a log to the database
                    // NOTE: Execute is an extension method from Dapper library
                    connection.Execute($"INSERT INTO [dbo].[Permission_table] (supplierId,status,modifiedOn) VALUES " +
                        $"('{supplierId}','{status}','{modifiedOn}')");
                }
            }
            catch
            {
                successful = false;
            }
            return successful
                ? new OkObjectResult("Record added successful!")
                : new OkObjectResult("Record is not added.Please try again");
        }
    }
}
