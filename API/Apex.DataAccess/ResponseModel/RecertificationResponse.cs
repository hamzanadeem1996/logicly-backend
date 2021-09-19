using System;

namespace Apex.DataAccess.ResponseModel
{
    public class RecertificationResponse
    {
        public int Id { get; set; } //primaryKey
        public int PatientId { get; set; }
        public string PatientName { get; set; }
        public DateTime RecertificationDate { get; set; }
        public string Frequency { get; set; }
        public string NurseName { get; set; }
    }
}