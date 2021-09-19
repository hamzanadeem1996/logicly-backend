using System;

namespace Apex.DataAccess.Response
{
    public class PatientVisitHistoryResponse
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; }
        public DateTime DischargeDate { get; set; }
        public string Status { get; set; }
    }
}