using Apex.DataAccess.Models;
using Apex.DataAccess.Repositories;
using Apex_Api.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Apex_Api
{
    public class Common
    {
        public Common(IConfiguration configuration)
        {
            _Configuration = configuration;
        }

        public IConfiguration _Configuration { get; }

        public class HttpException : Exception
        {
            private readonly int httpStatusCode;

            public HttpException(HttpStatusCode httpStatusCode)
            {
                this.httpStatusCode = (int)httpStatusCode;
            }

            public HttpException(int httpStatusCode, string message) : base(message)
            {
                this.httpStatusCode = httpStatusCode;
            }
        }

        public static class Instances
        {
            public static UserRepo User = new UserRepo();
            public static PatientProfileRepo PatientProfile = new PatientProfileRepo();
            public static RoleRepo roles = new RoleRepo();
            public static FileRepo files = new FileRepo();
            public static PatientVisitHistoryRepo patientVisitHistory = new PatientVisitHistoryRepo();
            public static SettingRepo SettingsRepoInst = new SettingRepo();
            public static PatientUnavailableTimeSlotRepo unavailableTimeSlot = new PatientUnavailableTimeSlotRepo();
            public static PatientVisitScheduleRepo VisitScheduleRepoInst = new PatientVisitScheduleRepo();
            public static CountryRepo CountryInst = new CountryRepo();
            public static TemplateRepo TemplateInst = new TemplateRepo();
            public static AgencyRepo AgencyInst = new AgencyRepo();
            public static RecertificationRepo RecertificationInst = new RecertificationRepo();
            public static RecertificationService RecertificationServiceInst = new RecertificationService();
            public static PatientAvailabilityRepo PatientAvailabilityInst = new PatientAvailabilityRepo();
            public static ClinicianAvailabilityRepo ClinicianAvailabilityInst = new ClinicianAvailabilityRepo();
            public static PlanRepo PlanInst = new PlanRepo();
            public static PermissionRepo PermissionInst = new PermissionRepo();
            public static PlanPermissionRepo PlanPermissionInst = new PlanPermissionRepo();
            public static PatientProfileRepo PatientProfileInst = new PatientProfileRepo();
            public static SettingRepo SettingInst = new SettingRepo();
            public static PatientVisitScheduleRepo VisitScheduleInst = new PatientVisitScheduleRepo();
            public static PatientUnavailableTimeSlotRepo UnavailableTimeSlotInst = new PatientUnavailableTimeSlotRepo();
            public static AgencyRepo ApiInst = new AgencyRepo();
            public static CardRepo CardInst = new CardRepo();
            public static DriveHistoryRepo DriveHistoryInst = new DriveHistoryRepo();
            public static DashboardRepo DashboardRepoInst = new DashboardRepo();

            public static PatientProfileService patientProfileService = new PatientProfileService();
        }

        public static string Encrypt(string text)
        {
            byte[] encodedPassword = new UTF8Encoding().GetBytes(text);
            byte[] hash = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(encodedPassword);
            string encoded = BitConverter.ToString(hash)
               .Replace("-", string.Empty)
               .ToLower();
            return encoded;
        }

        public static string UploadPath
        {
            get
            {
                return "Upload";
            }
        }

        public static string MyRoot(HttpContext context)
        {
            var request = context.Request;
            var host = request.Host.ToUriComponent();
            var pathBase = request.PathBase.ToUriComponent();
            return $"{request.Scheme}://{host}{pathBase}" + "/";
        }

        public static User GetUserbyToken(HttpContext context)
        {
            var identity = context.User.Identity as ClaimsIdentity;
            if (identity.Claims.Count() <= 0)
            {
                throw new HttpException(HttpStatusCode.Unauthorized);
            }
            IEnumerable<Claim> claims = identity.Claims;
            var emailid = identity.Claims.FirstOrDefault().Value;
            var _user = Common.Instances.User.CheckEmail(emailid);
            return _user;
        }

        public static void FixNull<T>(ref T obj)
        {
            if (obj == null) return;
            foreach (var propertyInfo in obj.GetType().GetProperties())
            {
                if (propertyInfo.PropertyType == typeof(string))
                {
                    if (propertyInfo.GetValue(obj, null) == null)
                    {
                        propertyInfo.SetValue(obj, string.Empty, null);
                    }
                }
            }
        }

        public static bool ChecksubscriptionStatus(int agencyid)
        {
            var agency = Common.Instances.AgencyInst.GetAgency(agencyid);
            if (agency.Id > 0)
            {
                var customer = new StripeService().GetCustomer(agency.StripeCustomerId);
                var options = new SubscriptionListOptions
                { Customer = customer.Id, };
                var service = new SubscriptionService();
                var subscriptions = service.List(options);
                foreach (var sub in subscriptions.Data)
                {
                    if (sub.Status == "trialing" || sub.Status == "active")
                        return true;
                }
            }
            return false;
        }
    }
}