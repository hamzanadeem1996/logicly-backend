using NPoco;
using System;

namespace Apex.DataAccess.Models
{
    [TableName("PlanPermissions")]
    [PrimaryKey("Id")]
    public class PlanPermission
    {
        public int Id { get; set; }
        public int PlanId { get; set; }
        public int PermissionId { get; set; }
        [ResultColumn] public string PlanName { get; set; }
        [ResultColumn] public string PermissionName { get; set; }
        public DateTime AddedOn { get; set; }
        public int AddedBy { get; set; }
        public int LastModBy { get; set; }
        public DateTime LastModOn { get; set; }
    }
}