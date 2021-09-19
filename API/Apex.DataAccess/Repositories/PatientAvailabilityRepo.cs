using Apex.DataAccess.Models;
using Apex.DataAccess.Response;
using NPoco;
using System;

namespace Apex.DataAccess.Repositories
{
    public class PatientAvailabilityRepo
    {
        public PatientAvailabilityResponse Get(int id)
        {
            var cmd = Sql.Builder.Select($@"a.*,
             CONCAT(
                 CAST(AES_DECRYPT(UNHEX(p.FirstName),'LpxenIGdqA0jMgrvFzpBdeFKtXUNNKMx') AS CHAR (150))
                 ,' ',
                 CAST(AES_DECRYPT(UNHEX(p.LastName),'LpxenIGdqA0jMgrvFzpBdeFKtXUNNKMx') AS CHAR (150))
                 ) AS PatientName").From("PatientAvailability a");
            cmd.InnerJoin("PatientProfiles p").On("p.Id = a.PatientId");
            cmd.Where("a.Id=@0", id);
            cmd.OrderBy("a.Id");
            using (var db = Utility.Database)
            {
                var result = db.FirstOrDefault<PatientAvailabilityResponse>(cmd);
                return result;
            }
        }

        public Page<PatientAvailabilityResponse> GetAll(int patientId, int pagenumber, int pagesize, string query = "")
        {
            var cmd = Sql.Builder.Select($@"a.*,
             CONCAT(
                 CAST(AES_DECRYPT(UNHEX(p.FirstName),'LpxenIGdqA0jMgrvFzpBdeFKtXUNNKMx') AS CHAR (150))
                 ,' ',
                 CAST(AES_DECRYPT(UNHEX(p.LastName),'LpxenIGdqA0jMgrvFzpBdeFKtXUNNKMx') AS CHAR (150))
                 ) AS PatientName").From("PatientAvailability a");
            cmd.InnerJoin("PatientProfiles p").On("p.Id = a.PatientId");
            cmd.Where("a.PatientId=@0", patientId);
            cmd.OrderBy("a.Id");
            using (var db = Utility.Database)
            {
                var result = db.Page<PatientAvailabilityResponse>(pagenumber, pagesize, cmd);
                return result;
            }
        }

        public PatientAvailability Save(PatientAvailability patientAvailability)
        {
            using (var db = Utility.Database)
            {
                db.Save(patientAvailability);
                return patientAvailability;
            }
        }

        public int Delete(int Id)
        {
            using (var db = Utility.Database)
            {
                var result = db.Delete<PatientAvailability>(Id);
                return Convert.ToInt32(result);
            }
        }
    }
}