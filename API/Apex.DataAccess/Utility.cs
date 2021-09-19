using Bogus;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using NPoco;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Apex.DataAccess.Models;
using Ical.Net.DataTypes;

namespace Apex.DataAccess
{
    public class Utility
    {
        public class WeekRange
        {
            public DateTime WeekStart{ get; set; }
            public DateTime WeekEnd { get; set; }
        }

        public const string LIVE_LOGO_URL =
            "https://api.logicly.ai/Upload/510e3810-53bc-4939-a00f-27d6549ef00bLogo.png";

        public const string HASPAYMENTMESSAGE =
            "Please add a valid payment method and activate or subscription in the admin panel to continue.";

        public class ResponseMessage
        {
            public static string Ok = "Successful";
            public static string Already = "Already registered";
            public static string Unauthorized = "401";
            public static string BadRequest = "Bad Request";
            public static string NotFound = "Not Found";
            public static string InternalServerError = "Internal server error";
        }

        private static IConfiguration _config;

        private const string _StagingConnectionString =
            "Server=155.138.235.40;Database=logicly_db;User Id=logicly_dusr;Password=gQirczTTr5";

        private const string _LiveConnectionString =
            "Server=linux15.nextpageit.net;Database= npit_apexappdb;User Id=  npit_apexuser;Password=zGikEBAimr";

        public static string ConnectionString = _StagingConnectionString;
        public static string Mode = "STAGING";

        public static Faker FakerInstance => new Faker("en");

        public Utility(IConfiguration config)
        {
            _config = config;
            if (ConnectionString.Length <= 0)
            {
                ConnectionString = Mode == "STAGING" ? _StagingConnectionString : _LiveConnectionString;
            }

            ConnectionString = _config.GetSection("ConnectionStrings").GetSection("Default").Value;
        }

        public static IDatabase Database
        {
            get
            {
                if (ConnectionString.Length <= 0)
                    ConnectionString = _StagingConnectionString;
                return new ProfiledDatabase(ConnectionString, DatabaseType.MySQL, MySqlClientFactory.Instance);
            }
        }

        public static bool ValidateDate(string datetime)
        {
            return
                DateTime.TryParseExact(datetime, "MM-dd-yyyy", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out _);
        }

        public enum CardType
        {
            MasterCard,
            Visa,
            AmericanExpress,
            Discover,
            JCB,
            Unknown
        };

        public static CardType FindType(string cardNumber)
        {
            //https://www.regular-expressions.info/creditcard.html
            if (Regex.Match(cardNumber, @"^4[0-9]{12}(?:[0-9]{3})?$").Success)
            {
                return CardType.Visa;
            }

            if (Regex.Match(cardNumber,
                @"^(?:5[1-5][0-9]{2}|222[1-9]|22[3-9][0-9]|2[3-6][0-9]{2}|27[01][0-9]|2720)[0-9]{12}$").Success)
            {
                return CardType.MasterCard;
            }

            if (Regex.Match(cardNumber, @"^3[47][0-9]{13}$").Success)
            {
                return CardType.AmericanExpress;
            }

            if (Regex.Match(cardNumber, @"^6(?:011|5[0-9]{2})[0-9]{12}$").Success)
            {
                return CardType.Discover;
            }

            if (Regex.Match(cardNumber, @"^(?:2131|1800|35\d{3})\d{11}$").Success)
            {
                return CardType.JCB;
            }

            return CardType.Unknown;
        }

        public static DateTime GetNextWeekday(DateTime start, DayOfWeek day)
        {
            int daysToAdd = ((int) day - (int) start.DayOfWeek + 7) % 7;
            return start.AddDays(daysToAdd);
        }

        public static string GetDateRangeOfWeek(DateTime dateTime)
        {
            var firstday = dateTime.AddDays(-(int) dateTime.DayOfWeek);
            if (dateTime.Month != firstday.Month)
            {
                var last = GetNextWeekday(firstday, DayOfWeek.Saturday);
                return $"{firstday.ToString("MMM dd, yyyy")} - {last.ToString("MMM dd, yyyy")} ";
            }

            var lastDay = firstday.AddDays(6).Month != firstday.Month
                ? Utility.GetLastDayOfMonth(firstday)
                : firstday.AddDays(6);
            return $"{firstday.ToString("MMM dd, yyyy")} - {lastDay.ToString("MMM dd, yyyy")} ";
        }

        public static DateTime GetLastDayOfMonth(DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, DateTime.DaysInMonth(dateTime.Year, dateTime.Month));
        }


        public static int WeekNumber(DateTime dateTime)
        {
            return CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(dateTime, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Sunday);

        }


        public static string GetScheduleType(string scheduleType)
        {
            switch (scheduleType)
            {
                case "E":
                    return "Evaluation";

                case "D":
                    return "Discharge";

                case "R":
                    return "Recert";

                case "30":
                    return "30DRE";

                default:
                    return "RoutineVisit";
            }
        }

        public static string GetColor(string colorChar)
        {
            switch (colorChar)
            {
                case "R":
                    return "#D40000";

                case "E":
                    return "#BA96D7";

                case "D":
                    return "#EDD300";

                case "RV":
                    return "#7db885";

                default:
                    return "759AE0";
            }
        }

        public enum Roles
        {
            ADMIN,
            USER,
            SUPERADMIN,
            SN,
            OT,
            PT,
            SLP,
            OTA,
            PTA,
            MSW,
        }

        //Allow only SN OT PT SLP, Admin to add cert periods
        public static bool CheckAddCertByRole(string role)
        {
            switch (role)
            {
                case "ADMIN":
                    return true;

                case "SN":
                    return true;

                case "OT":
                    return true;

                case "PT":
                    return true;

                case "SLP":
                    return true;

                case "SUPERADMIN":
                    return true;

                default:
                    return false;
            }
        }

        public static int GetLeftDaysInWeek(DateTime start)
        {
            int daysleft = (int) (GetNextWeekday(start, DayOfWeek.Saturday).DayOfWeek) - (int) start.DayOfWeek;
            return daysleft == 0 ? 0 : daysleft + 1;
        }

        public static bool DateInRange(DateTime dateToCheck, DateTime startDate, DateTime endDate)
        {
            return dateToCheck >= startDate && dateToCheck <= endDate;
        }

        public static int GetRemaningDaysInMonth(DateTime dateTime)
        {
            int leftdays = DateTime.DaysInMonth(dateTime.Year, dateTime.Month) - dateTime.Day;
            return leftdays + 1;
        }


        public static WeekRange WeekDateRange(int year, int weekOfYear)
        {
            var weekRange = new WeekRange();
            DateTime jan = new DateTime(year, 1, 1);
            int daysOffset = Convert.ToInt32(System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek) - Convert.ToInt32(jan.DayOfWeek);
            DateTime firstWeekDay = jan.AddDays(daysOffset);
            System.Globalization.CultureInfo curCulture = System.Globalization.CultureInfo.CurrentCulture;
            int firstWeek = curCulture.Calendar.GetWeekOfYear(jan, curCulture.DateTimeFormat.CalendarWeekRule, curCulture.DateTimeFormat.FirstDayOfWeek);
            weekRange.WeekStart = firstWeekDay.AddDays(weekOfYear * 7);
            weekRange.WeekEnd = weekRange.WeekStart.AddDays(6);
            return weekRange;
        }

        public static int GetSessionTime(string colortype, Setting setting)
        {
            var sessionTime = 30; //Default 30
            switch (colortype)
            {
                case "E":
                    sessionTime = setting.EvaluationSessionLength;
                    break;
                case "R":
                    sessionTime = setting.RecertSessionLength;
                    break;
                case "D":
                    sessionTime = setting.DischargeSessionLength;
                    break;
                case "30":
                    sessionTime = setting.ThirtyDayReEvalSessionLength;
                    break;
                case "RV":
                    sessionTime = setting.TreatmentSessionLength;
                    break;
                default:
                    sessionTime = 60;
                    break;
            }

            return sessionTime;
        }

        public static List<WeekDay> GetDaysOfWeek(int visitsPerUnit)
        {
            switch (visitsPerUnit)
            {
                case 1:
                    return new List<WeekDay>
                    {
                        new WeekDay {DayOfWeek = DayOfWeek.Saturday}
                    };
                case 2:
                    return new List<WeekDay>
                    {
                        new WeekDay {DayOfWeek = DayOfWeek.Friday},
                        new WeekDay {DayOfWeek = DayOfWeek.Saturday}
                    };
                case 3:
                    return new List<WeekDay>
                    {
                        new WeekDay {DayOfWeek = DayOfWeek.Thursday},
                        new WeekDay {DayOfWeek = DayOfWeek.Friday},
                        new WeekDay {DayOfWeek = DayOfWeek.Saturday}
                    };
                case 4:
                    return new List<WeekDay>
                    {
                        new WeekDay {DayOfWeek = DayOfWeek.Wednesday},
                        new WeekDay {DayOfWeek = DayOfWeek.Thursday},
                        new WeekDay {DayOfWeek = DayOfWeek.Friday},
                        new WeekDay {DayOfWeek = DayOfWeek.Saturday}
                    };
                case 5:
                    return new List<WeekDay>
                    {
                        new WeekDay {DayOfWeek = DayOfWeek.Tuesday},
                        new WeekDay {DayOfWeek = DayOfWeek.Wednesday},
                        new WeekDay {DayOfWeek = DayOfWeek.Thursday},
                        new WeekDay {DayOfWeek = DayOfWeek.Friday},
                        new WeekDay {DayOfWeek = DayOfWeek.Saturday}
                    };
                case 6:
                    return new List<WeekDay>
                    {
                        new WeekDay {DayOfWeek = DayOfWeek.Monday},
                        new WeekDay {DayOfWeek = DayOfWeek.Tuesday},
                        new WeekDay {DayOfWeek = DayOfWeek.Wednesday},
                        new WeekDay {DayOfWeek = DayOfWeek.Thursday},
                        new WeekDay {DayOfWeek = DayOfWeek.Friday},
                        new WeekDay {DayOfWeek = DayOfWeek.Saturday}
                    };
                default:
                {
                    if (visitsPerUnit >= 7)
                        return new List<WeekDay>
                        {
                            new WeekDay {DayOfWeek = DayOfWeek.Sunday},
                            new WeekDay {DayOfWeek = DayOfWeek.Monday},
                            new WeekDay {DayOfWeek = DayOfWeek.Tuesday},
                            new WeekDay {DayOfWeek = DayOfWeek.Wednesday},
                            new WeekDay {DayOfWeek = DayOfWeek.Thursday},
                            new WeekDay {DayOfWeek = DayOfWeek.Friday},
                            new WeekDay {DayOfWeek = DayOfWeek.Saturday}
                        };
                }
                    return new List<WeekDay> {new WeekDay {DayOfWeek = DayOfWeek.Monday}};
            }
        }

        public static bool IsBetween(DateTime dt, DateTime start, DateTime end)
        {
            return end <= dt;
        }
    }
}