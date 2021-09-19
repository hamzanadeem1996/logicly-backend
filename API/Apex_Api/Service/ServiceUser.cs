using Apex.DataAccess;
using Apex.DataAccess.Models;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace Apex_Api.Service
{
    public class ServiceUser
    {
        public object save(User user, IUserService userService, IConfiguration Configuration)
        {
            if (user.Id == 0)
            {
                string emailBody = "";
                var getTemplate = EmailService.SendWelcomeEmail(); // // Welcome Email
                emailBody = getTemplate;
                emailBody = emailBody.Replace("{Email}", user.Email)
                .Replace("{Name}", $"{user.FirstName}")
                .Replace("{Password}", user.Password);

                var wasSent = EmailService.SendEmail(user.Email, Constant.WELCOMEEMAIL, true, emailBody,
                    "", Configuration);

                user.Password = Common.Encrypt(user.Password);
            }
            else if (user.Id > 0 && user.Password == null || user.Password == "")
            {
                var Getpass = Common.Instances.User.Getpasswordbyuserid(user.Id);
                user.Password = Getpass.Password;
            }
            else if (user.Password != null || user.Password != "")
            {
                var Getpass = Common.Instances.User.Getpasswordbyuserid(user.Id);
                user.Password = Common.Encrypt(user.Password);
            }
            if (user.RoleId == 0)
            {
                user.RoleId = Common.Instances.roles.GetAll(1, 10, "").Items.Select(x => x.Id).LastOrDefault();
            }

            user.AddedBy = user.Id;
            user.LastModBy = user.Id;
            Common.Instances.User.Save(user);

            var Gettoken = userService.Authenticate(user.Email, user.Password);
            user.Token = Gettoken.Token;

            // save user setting
            var getUserSetting = Common.Instances.SettingsRepoInst.Get(user.Id);
            if (user != null && user.Id > 0 && getUserSetting.Id == 0)
            {
                Setting setting = Constant.DefaultSetting(user.Id);
                Common.Instances.SettingsRepoInst.Save(setting, setting.UserId);
            }
            return user;
        }
    }
}