
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
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;

namespace statusapp
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {



            var successful = true;
            try
            {

                //var cnnString = ConfigurationManager.ConnectionStrings[0].ToString();
                //var cnnString = "Data Source=.;Initial Catalog=EmpDb;Integrated Security=True;Trust Server Certificate=True";

                var config = new ConfigurationBuilder()
           .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                  .AddEnvironmentVariables()
                         .Build();

                string cnnString = config["ConnectionStrings:sqlcon"];


                using (var connection = new SqlConnection(cnnString))   //  clear memory after excuting
                {
                    connection.Open();
                    log.LogInformation("C# HTTP trigger function processed a request.");
                    string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                    dynamic data = JsonConvert.DeserializeObject(requestBody);
                    var id = data?.id;
                    string status = data?.status;
                    var ModifiedON = DateTime.Now.ToString();




                    // insert a log to the database
                    // NOTE: Execute is an extension method from Dapper library
                    connection.Execute($"INSERT INTO [dbo].[Store_tb1] (id,status,ModifiedON) VALUES ('{id}','{status}','{ModifiedON}')");


                }
            }
            catch
            {
                successful = false;
            }


            return !successful
                ? new OkObjectResult("failed")
                : new OkObjectResult("success");

        }
    }
}








