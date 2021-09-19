using NPoco;
using System;

namespace Apex.DataAccess.Models
{
    [TableName("DriveHistory")]
    [PrimaryKey("Id")]
    public class DriveHistory
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime DriveDate { get; set; }
        public decimal MilesDriven { get; set; }
        public int DrivenTime { get; set; }
        public DateTime AddedOn { get; set; }
        public int AddedBy { get; set; }
        public DateTime LastModOn { get; set; }
        public int LastModBy { get; set; }
    }
}