using Apex.DataAccess.Models;
using Apex.DataAccess.Response;
using NPoco;
using System;

namespace Apex.DataAccess.Repositories
{
    public class PatientVisitHistoryRepo
    {
        public PatientVisitHistory Get(int id)
        {
            using (var db = Utility.Database)
            {
                var result = db.SingleById<PatientVisitHistory>(id) ?? new PatientVisitHistory();
                return result;
            }
        }

        public Page<PatientVisitHistoryResponse> GetAll(int pagenumber, int pagesize, string query = "", int patientid = 0)
        {
            var cmd = Sql.Builder.Select($@"pvh.*,
              CONCAT(
                 CAST(AES_DECRYPT(UNHEX(pp.FirstName),'{Encryption.Key}') AS CHAR (150))
                 ,' ',
                 CAST(AES_DECRYPT(UNHEX(pp.LastName),'{Encryption.Key}') AS CHAR (150))
                 ) AS PatientName
                ").From("PatientVisitHistory pvh");

            cmd.LeftJoin("PatientProfiles pp").On("pvh.PatientId = pp.Id");

            if (patientid > 0) { cmd.Where("pvh.PatientId=@0", patientid); }

            cmd.OrderBy("pvh.Id");
            using (var db = Utility.Database)
            {
                var result = db.Page<PatientVisitHistoryResponse>(pagenumber, pagesize, cmd);
                return result;
            }
        }

        public object Save(PatientVisitHistory patientVisitHistory)
        {
            using (var db = Utility.Database)
            {
                if (patientVisitHistory.Id == 0) patientVisitHistory.AddedOn = DateTime.UtcNow;

                patientVisitHistory.LastModOn = DateTime.UtcNow;
                db.Save(patientVisitHistory);
                return patientVisitHistory;
            }
        }

        public int Delete(int Id)
        {
            using (var db = Utility.Database)
            {
                var result = db.Delete<PatientVisitHistory>(Id);
                return Convert.ToInt32(result);
            }
        }
    }
}