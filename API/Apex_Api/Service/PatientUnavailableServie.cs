using AgileObjects.AgileMapper.Extensions;
using Apex.DataAccess.Models;
using Apex.DataAccess.RequestModel;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using static Apex_Api.Common;

namespace Apex_Api.Service
{
    public class PatientUnavailableServie
    {
        public object Save(PatientAvailabilityRequest patientAvailabilityRequest)
        {
            var patientNonAvailabilityCount = Common.Instances.PatientAvailabilityInst.GetAll(patientAvailabilityRequest.PatientId,
                                      1, 10, "");

            if (patientNonAvailabilityCount.Items.Count >= 3 && patientAvailabilityRequest.Id == 0)

                throw new HttpException((int)HttpStatusCode.Unauthorized,
                                         "You maximum non availability save limit reached.");
            else if ((3 - patientNonAvailabilityCount.Items.Count) < patientAvailabilityRequest.WeekDayNo.Count()
                                                                    && patientAvailabilityRequest.Id == 0)

                throw new HttpException((int)HttpStatusCode.Unauthorized,
                                         $"Your {(3 - patientNonAvailabilityCount.Items.Count)} non availability save left");

            var patientUnavailable = new List<PatientAvailability>();
            foreach (var WeekDayNo in patientAvailabilityRequest.WeekDayNo)
            {
                var nonAvailability = new PatientAvailability
                {
                    WeekDayNo = WeekDayNo,
                };
                nonAvailability = patientAvailabilityRequest.Map().Over(nonAvailability);
                patientUnavailable.Add(Common.Instances.PatientAvailabilityInst.Save(nonAvailability));
            };
            return patientUnavailable;
        }
    }
}