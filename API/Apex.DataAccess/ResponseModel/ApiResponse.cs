namespace Apex.DataAccess.Response
{
    public class ApiResponse
    {
        public object Data { get; set; }
        public object Events { get; set; }
        public string Message { get; set; }
        public int Status { get; set; }
        public object Errors { get; set; }
    }
}