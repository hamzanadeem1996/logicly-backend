using Apex.DataAccess.Models;
using System;

namespace Apex_Api
{
    public class Constant
    {
        public const string AdminChatUserId = "kL7AvQsivgLaN6Wdo";
        public const string NOPERMISSION = @"Your plan doesn't support this feature. Please contact your team administrator for details.";
        public const string ADMISSIONDATE = @"Admission date should be greater or equal than current date";
        public const string STRIPESECRETKEY = "sk_test_51HhOSLByZlA80ds8ua7hIxLccC9K7uMcGh8pxWblgVf20K4fI1sar4M46OvNvqHSGvGeUgfgntuBjz7n02gGiCex00yzcS3X2z";
        public const string WELCOMEEMAIL = "Welcome to Logicly";
        public const string PATIENTDISCHARGE = "Patient Discharged. This patient will no longer show in the scheduling options.";
        public const string Message = "Successful";
        public const string IncorrectEmailpass = "Your Email or Password is incorrect !";
        public const string NotFound = "NotFound";
        public const string ForgotPasswordSubject = "Reset Password Logicly";
        public const string NurseAddress = "Agency Address";
        public const string PatientDelete = "Patient can't be deleted as it has active visit schedule. Please remove from visit schedule before deleting";
        public const string KeyForgotPassword = "FORGOTPASSWORD"; //key
        public const string ColorGrey = "#808080"; //key
        public const string ADMIN = "ADMIN"; //key
        public const string SUPERADMIN = "SUPERADMIN"; //key
        public const string USER = "USER"; //key
        public const string MODEAPP = "APP";
        public const string MODEWEB = "WEB";
        public const string AppPremission = "The app is optimised for clinicians role only.";
        public const string IncludeWeekendsInWeekView = "Please enable allow weekends in weekview from settings to schedule patients on weekends.";
        public const string FREQUENCYLIMITEXCEED = "This frequency exceeds the end of care.";
        public const string SATURDAY = "Saturday";
        public const string SUNDAY = "Sunday";
        public const string THURSDAY = "Thursday";
        public const string FRIDAY = "Friday";
        public const string NURSEADDRESS = "Agency Address";
        public const string COLORGREY = "#808080";
        public const string COLORMISSED = "#FF69B4";
        public const string COLORCOMPLETED = "#0000CD";
        public const string GOOGLE_API_KEY = "AIzaSyCqB8EW5b48lY_IOp5DIM0kRPC_H1igWc8";
        public const string STAGING_LOGO_URL = "http://apex-api.npit.info/Upload/25395613-24c9-468f-9554-9fb04dfa2da9Logo.png";
        public const string TODAY = "TODAY";
        public const string QUICKSIGNUP = "Signup successful, please check your email for login credentials.";
        public const string SUBSCRIPTIONENDED = "Your subscription has ended. please contact Apex team to renew.";
        public const string VISITSTATUSCOMPLETED = "COMPLETED";

        // fill data for user setting.
        public static Setting DefaultSetting(int userid)
        {
            var setting = new Setting
            {
                Units = "Miles",
                UserId = userid,
                RoutingApp = "Google Maps",
                TreatmentSessionLength = 60,
                EvaluationSessionLength = 60,
                AdmissionSessionLength = 60,
                DischargeSessionLength = 60,
                RecertSessionLength = 60,
                ThirtyDayReEvalSessionLength = 60,
                DistanceCalculator = "Starting/Home Address",
                IncludeWeekendsInWeekView = "Yes",
                WorkingHours = "9:00-15:00",
                PatinetNameFormat = "UpperCase",
                ColorCoding = "Black",
                AddedBy = userid,
                LastModBy = userid,
                Start = new TimeSpan(08, 00, 00),
                End = new TimeSpan(17, 00, 00)
            };
            return setting;
        }
    }
}