using Apex.DataAccess.Response;
using ElmahCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Apex_Api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("CorsPolicy")]
    public class FilesController : ControllerBase
    {
        private readonly IHostingEnvironment _environment;

        public FilesController(IHostingEnvironment environment)
        {
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        #region

        [HttpGet("Get")]
        public ApiResponse Get(int id)
        {
            var res = new ApiResponse();
            try
            {
                res.Data = Common.Instances.files.Get(id);
                res.Message = Constant.Message;
            }
            catch (Exception ex)
            {
                HttpContext.RiseError(ex);
                res.Message = ex.Message;
            }
            return res;
        }

        [HttpPost("Upload")]
        public async Task<ApiResponse> Upload(IFormFile file, string type, string filename)
        {
            var res = new ApiResponse();
            try
            {
                var _user = Common.GetUserbyToken(HttpContext);
                string imageUrl = "";
                var path = Path.Combine(_environment.ContentRootPath, Common.UploadPath);//test for webrootpath
                if (!Directory.Exists(path)) { Directory.CreateDirectory(path); }
                var filee = Request.Form.Files[0];
                var Filename = Guid.NewGuid() + filee.FileName;
                using (var fileStream = new FileStream(Path.Combine(path, Filename), FileMode.Create))
                {
                    imageUrl = Common.MyRoot(HttpContext) + Common.UploadPath + "/" + Filename;//ModifyImage name unique
                    await filee.CopyToAsync(fileStream);
                }
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    Apex.DataAccess.Models.File filedetail = new Apex.DataAccess.Models.File();
                    filedetail.FileUrl = imageUrl;
                    filedetail.FileName = filename;
                    filedetail.Type = type;
                    filedetail.AddedBy = _user.Id;
                    filedetail.LastModBy = _user.Id;
                    res.Data = Common.Instances.files.Save(filedetail);
                    res.Message = Constant.Message;
                }
            }
            catch (Exception ex)
            {
                HttpContext.RiseError(ex);
                res.Message = ex.Message;
            }
            return res;
        }

        #endregion
    }
}