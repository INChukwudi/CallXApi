using Azure.Core;
using CallXApi.DataModels;
using CallXApi.Services;
using Microsoft.Extensions.Options;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace CallXApi.Models
{
    public class ReportDb
    {

        private readonly AppSettings _appSettings;
        //private IConfiguration _configuration;
        private Connection mycon;
        public CallXDBContext _context;
        private GenericService gen;
        public string Email;
        public string Password;
        private BlobService _blober;


        public ReportDb(IOptions<AppSettings> appSettings, GenericService generic, Connection connection, CallXDBContext context, BlobService blober)
        {
            _appSettings = appSettings.Value;
            mycon = connection;
            _context = context;
            gen = generic;
            _blober = blober;
        }



        public async Task<KeyValuePair<string, string>> UploadSchoolImageRemote(IFormFile file)
        {
            var blobimgname = gen.Filenamer(file, "cpic");
            var blobimgpath = await _blober.UploadFileBlobAsync("logo", file.OpenReadStream(), file.ContentType, blobimgname);
            var ext = Path.GetExtension(file.FileName);
            var reel = new KeyValuePair<string, string>(blobimgname, blobimgpath);
            return reel;
            //return new {ext = ext, path = blobimgpath, size = Lengther(myfile.Length), name = myfile.Name};
        }

       
    }
}
