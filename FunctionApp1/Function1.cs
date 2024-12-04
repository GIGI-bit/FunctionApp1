using System.Net;
using System.Net.Mail;
using System.Text.Json;
using Grpc.Core;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace FunctionApp1
{
    public class SmtpSettings
    {
        public string Server { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool EnableSsl { get; set; }
    }
    public class Function1
    {
        private readonly ILogger _logger;

        public Function1(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Function1>();
        }

        [Function("Function1")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            string smtpSettingsJson = Environment.GetEnvironmentVariable("SMTP_SETTINGS");
            var smtpSettings = JsonSerializer.Deserialize<SmtpSettings>(smtpSettingsJson);

            var smtpClient = new SmtpClient(smtpSettings.Server)
            {
                Port = smtpSettings.Port,
                Credentials = new NetworkCredential(smtpSettings.Username, smtpSettings.Password),
                EnableSsl = smtpSettings.EnableSsl
            };
            string recipientEmail = req.Query["email"];
            var mailMessage = new MailMessage
            {
                From = new MailAddress(smtpSettings.Username),
                Subject = "Function app message",
                Body = "Hello "+recipientEmail+"this message is from AzureFunction",
                IsBodyHtml = false
            };
            try
            {
                await smtpClient.SendMailAsync(mailMessage);
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

                response.WriteString($"Email sent successfully to {recipientEmail}");
                return response;
            }
            catch (Exception ex)
            {
                var response = req.CreateResponse(HttpStatusCode.NotFound);
                return response;
            }
            //_logger.LogInformation("C# HTTP trigger function processed a request.");

            //var response = req.CreateResponse(HttpStatusCode.OK);
            //response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            //response.WriteString("Welcome to Azure Functions from world!");

            //return response;
        }
    }
}
