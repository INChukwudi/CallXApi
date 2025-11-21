using CallXApi.DataModels;
using CallXApi.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace CallXApi.Services
{
    public class GenericService
    {
        private CallXDBContext _context;
        private string myConnectString;

        public GenericService(CallXDBContext ctx, IConfiguration config)
        {
            _context = ctx;
            myConnectString = config["Data:GradeXGres:RemoteWebDataString"];
        }

        public string DefaultString
        {
            get { return myConnectString; }
        }

        public string FileLocation(string resource, string dir, string fileName)
        {
            var folder = Path.Combine(resource, dir);
            var filedir = Path.Combine(Directory.GetCurrentDirectory(), folder);
            return Path.Combine(filedir, fileName);
        }
        public async Task<FileReturn> SaveFileOnly(string resource, string dir, IFormFile file)
        {
            var folderName = Path.Combine(resource, dir);
            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
            var fileName = Path.GetFileNameWithoutExtension(file.FileName);
            var ext = Path.GetExtension(file.FileName);
            var fileId = fileName + ext;
            var path = Path.Combine(pathToSave, fileId);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return new FileReturn { fileId = fileId, path = path };
        }
        public async Task<FileReturn> SaveFile(string resource, string dir, IFormFile file)
        {
            var folderName = Path.Combine(resource, dir);
            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
            var fileName = Path.GetFileNameWithoutExtension(file.FileName);
            var ext = Path.GetExtension(file.FileName);
            var size = Lengther(file.Length);
            var guid = Guid.NewGuid().ToString().Substring(0, 5);
            var fileId = fileName + guid + ext;
            var path = Path.Combine(pathToSave, fileId);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return new FileReturn { fileId = fileId, fileSize = size, path = path };
        }

        public FileReturn GetFileData(string resource, string dir, IFormFile file)
        {
            var folderName = Path.Combine(resource, dir);
            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
            var fileName = Path.GetFileNameWithoutExtension(file.FileName);
            var ext = Path.GetExtension(file.FileName);
            var size = Lengther(file.Length);
            var guid = Guid.NewGuid().ToString().Substring(0, 5);
            var fileId = fileName + guid + ext;
            var path = Path.Combine(pathToSave, fileId);
            return new FileReturn { fileId = fileId, fileSize = size, path = path };
        }
        public async Task<FileReturn> SaveFile(string resource, string dir, IFormFile file, string pretext)
        {
            var folderName = Path.Combine(resource, dir);
            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
            var fileName = Path.GetFileNameWithoutExtension(file.FileName);
            var ext = Path.GetExtension(file.FileName);
            var size = Lengther(file.Length);
            var guid = Guid.NewGuid().ToString().Substring(0, 5);
            var imageId = pretext + fileName + guid + ext;
            var path = Path.Combine(pathToSave, imageId);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return new FileReturn { fileId = imageId, fileSize = size, path = path };
        }

        public void DeleteFile(string resource, string dir, string mediaName)
        {
            var folderName = Path.Combine(resource, dir);
            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
            var path = Path.Combine(pathToSave, mediaName);
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
        }

        public async Task<object> Numberlize(IQueryable<object> data, int? pageindex, int? size)
        {
            double count = Math.Ceiling(await data.CountAsync() / Convert.ToDouble(size));
            var userz = await data.Skip(Convert.ToInt32(pageindex)).Take(Convert.ToInt32(size)).ToListAsync();
            return new { pageCount = count, data = userz };
        }

        public async Task<object> Numberlize(IQueryable<object> data, int seen, int? pageindex, int? size)
        {
            double count = Math.Ceiling(await data.CountAsync() / Convert.ToDouble(size));
            var userz = await data.Skip(Convert.ToInt32(pageindex)).Take(Convert.ToInt32(size)).ToListAsync();
            return new { pageCount = count, data = userz, seen = seen };
        }

        public dynamic ObtainMedia(string dir, string subdir, string filename)
        {
            var medfolder = Path.Combine(dir, subdir);
            var meddir = Path.Combine(Directory.GetCurrentDirectory(), medfolder);
            var filepath = Path.Combine(meddir, filename);
            if (!System.IO.File.Exists(filepath))
            {
                return StatusCodes.Status404NotFound;
            }
            return new FileStream(filepath, FileMode.Open, FileAccess.Read);
        }


        public string Filenamer(IFormFile file)
        {
            var name = Path.GetFileNameWithoutExtension(file.FileName);
            var ext = Path.GetExtension(file.FileName);

            var guid = Guid.NewGuid().ToString().Substring(2, 9);
            var span = DateTime.Now.Subtract(DateTime.MinValue).TotalSeconds;
            Random rand = new Random();
            var regtext = name.Substring(0, 5) + span.ToString() + guid + rand.Next(1000).ToString();
            var texo = System.Text.RegularExpressions.Regex.Replace(regtext, @"[^a-zA-Z0-9]", "");
            var goodname = texo + ext;
            return goodname;
        }

        public string Filenamer(IFormFile file, string text)
        {
            var name = Path.GetFileNameWithoutExtension(file.FileName);
            var ext = Path.GetExtension(file.FileName);

            var guid = Guid.NewGuid().ToString().Substring(2, 15);
            var span = DateTime.Now.Subtract(DateTime.MinValue).TotalSeconds;
            Random rand = new Random();
            var regtext = text + span.ToString() + guid + rand.Next(1000).ToString();
            var texo = System.Text.RegularExpressions.Regex.Replace(regtext, @"[^a-zA-Z0-9]", "");
            var goodname = texo + ext;
            return goodname;
        }

        public string Tagnamer(string text)
        {
            var guid = Guid.NewGuid().ToString().Substring(2, 12);
            var span = DateTime.Now.Subtract(DateTime.MinValue).TotalSeconds;
            Random rand = new Random();
            var regtext = text + span.ToString() + guid + rand.Next(10000).ToString();
            var texo = System.Text.RegularExpressions.Regex.Replace(regtext, @"[^a-zA-Z0-9]", "");
            var goodname = texo + ".png";
            return goodname;
        }

        public long Lengther(long length)
        {
            return Convert.ToInt64(Math.Round(length / 1024.0));
        }

        public string GetFileExtension(string filename)
        {
            var ext = Path.GetExtension(filename);
            return ext.Remove(0, 1).ToUpper();
        }

        public String sqlDataToJson(NpgsqlDataReader dataReader)
        {
            var dataTable = new DataTable();
            dataTable.Load(dataReader);
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(dataTable);
            return JSONString;
        }


        public string GetFirstAlphabets(string sentence)
        {
            if (string.IsNullOrWhiteSpace(sentence))
                return string.Empty;

            string[] words = sentence.Split(new char[] { ' ', '\t', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string result = string.Empty;

            foreach (string word in words)
            {
                if (!string.IsNullOrWhiteSpace(word))
                    result += word[0].ToString().ToUpper();
            }

            return result;
        }


        public List<int?> AnnualResultByClassLevelSchools()
        {
            return new List<int?> { 540 };
        }

    }

}
