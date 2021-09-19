using Apex.DataAccess.Response;
using FluentValidation.Results;

namespace Apex_Api
{
    public static class Extensions
    {
        public static ApiResponse PrepareInvalidRequest(this ApiResponse res, ref ValidationResult valRes)
        {
            res.Data = new object();
            res.Errors = valRes.Errors;
            res.Message = "Invalid Request";
            return res;
        }
    }
}