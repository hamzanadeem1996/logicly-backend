using ElmahCore;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Text;

namespace Apex_Api.Service
{
    public class EmailService
    {
        public static bool SendEmail(string mailTo, string mailSubject, bool mailIsHtml, string body, string replyto,
            IConfiguration config)
        {
            try
            {
                var root = Path.GetDirectoryName(Assembly.GetExecutingAssembly()?.Location);
                if (config == null)
                    config = new ConfigurationBuilder()
                        .SetBasePath(root)
                        .AddJsonFile("appsettings.json").Build();

                var mode = config.GetSection("mode").Value;
                var key = mode == "dev" ? "staging_mail" : "smtp";

                var host = config.GetSection(key).GetSection("host").Value;
                var port = Convert.ToInt32(config.GetSection(key).GetSection("port").Value);
                var userName = config.GetSection(key).GetSection("username").Value;
                var password = config.GetSection(key).GetSection("password").Value;
                var formEmail = config.GetSection(key).GetSection("fromemail").Value;

                if (mailTo == "" || mailSubject == "" || body == "" || formEmail == "")
                    return false;

                var smtp = new SmtpClient(host)
                {
                    Port = port,
                    UseDefaultCredentials = false,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = new NetworkCredential(userName, password)
                };
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(formEmail),
                    Subject = mailSubject,
                    BodyEncoding = Encoding.UTF8,
                    IsBodyHtml = mailIsHtml,
                    Body = body
                };
                string[] emails = mailTo.Split(',');
                foreach (var item in emails)
                {
                    mailMessage.To.Add(new MailAddress(item));
                }
                smtp.Send(mailMessage);
                mailMessage.Attachments.Dispose();
                smtp.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                return false;
            }
        }

        private static string LoadTemplate(string tempName)
        {
            var templateService = new TemplateService();
            var tmp = templateService.GetByKey(tempName);
            return tmp.Content;
        }

        public static string PatientAssignNurse()
        {
            var temp = LoadTemplate("PATIENTASSIGNNURSE");
            return temp;
        }

        public static string SendPasswordRecoveryEmail()
        {
            var temp = LoadTemplate("FORGOTPASSWORD");
            return temp;
        }

        public static string SendWelcomeEmail()
        {
            var temp = LoadTemplate("WELCOMEEMAIL");
            return temp;
        }
    }
}