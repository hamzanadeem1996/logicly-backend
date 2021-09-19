namespace Apex.DataAccess.RequestModel
{
    public class QuickSignupRequest
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PlanId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Address { get; set; }
        public string MaxSessionHours { get; set; }
        public bool IsActive { get; set; }
    }
}