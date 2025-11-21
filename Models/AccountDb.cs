
using CallXApi.DataModels;
using CallXApi.Services;
using CallXApi.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
//using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;



namespace CallXApi.Models
{
    public class AccountDb
    {

        private readonly AppSettings _appSettings;
        //private IConfiguration _configuration;
        private Connection mycon;
        public CallXDBContext _context;
        private GenericService gen;
        public string Email;
        public string Password;


        public AccountDb(IOptions<AppSettings> appSettings, GenericService generic, Connection connection, CallXDBContext context)
        {
            _appSettings = appSettings.Value;
            mycon = connection;
            _context = context;
            gen = generic;
        }

        public string Encrypt(string clearText)
        {
            string EncryptionKey = "MAKV2SPBNI99212";
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }



        public string Decrypt(string cipherText)
        {
            string EncryptionKey = "MAKV2SPBNI99212";
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }

            return cipherText;
        }

        public async Task<int> CheckEmail(NpgsqlConnection conn, string username)
        {
            string query0 = @"SELECT COUNT(id) FROM users WHERE username = @email";
            NpgsqlCommand cmd0 = new NpgsqlCommand(query0, conn);
            cmd0.Parameters.AddWithValue("@email", username);
            return (int)(await cmd0.ExecuteScalarAsync());
        }

        public async Task<int> CheckEmailLogin(NpgsqlConnection conn, string username, int? schoolId)
        {
            string query0 = @"SELECT COUNT(""Id"") FROM ""Users"" WHERE ""Username"" = @Email AND ""SchoolId"" = @SchoolId";
            NpgsqlCommand cmd0 = new NpgsqlCommand(query0, conn);
            cmd0.Parameters.AddWithValue("@Email", username);
            cmd0.Parameters.AddWithValue("@SchoolId", schoolId);
            return (int)(await cmd0.ExecuteScalarAsync());
        }

        public async Task<long> CheckUser(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return 0;

            var count = await _context.users
                .Where(s => s.username == username.Trim())
                .LongCountAsync();

            return count;
        }

        public async Task<long> CheckGroupLoginInfo(NpgsqlConnection conn, string username)
        {
            string query0 = @"SELECT COUNT(""Id"") FROM ""GroupSchools"" WHERE ""Username"" = @username";
            NpgsqlCommand cmd0 = new NpgsqlCommand(query0, conn);
            cmd0.Parameters.AddWithValue("@username", username);
            return (long)(await cmd0.ExecuteScalarAsync());
        }

        public async Task<UserToken> AuthReg(int? userId)
        {
            var userdata = await _context.users.FindAsync(userId);
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            UserToken tok = new UserToken { id = userId, token = tokenHandler.WriteToken(token), name = userdata.surname + " " + userdata.first_name, photo = userdata.passport };
            return tok;
        }

        public async Task<UserToken> Authenticate(string username, string password)
        {
            //var userId = await GetStaffId(username, password, schoolId);
            var user = await GetUserStatus(username);
            //Staff mystaff = new Staff();

            if (user.id > 0)
            {
                // var school = await GetUserSchool(user.SchoolId);
                //if (user.role_id == 2)
                //{
                //    mystaff = await GetStaff(userId);
                //}
                // authentication successful so generate jwt token
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.id.ToString()),
                        new Claim(ClaimTypes.Sid, ""),
                    }),
                    Expires = DateTime.UtcNow.AddDays(1),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);

                UserToken tok;
                if (user.role_id == 1)
                {
                    tok = new UserToken { id = user.id, token = tokenHandler.WriteToken(token), code = 1, status = user.role_id.ToString(), photo = user.photo, name = user.last_name + " " + user.first_name };
                }
                else
                {
                    tok = new UserToken { id = user.id, token = tokenHandler.WriteToken(token), code = 1, status = user.role_id.ToString(), photo = user.photo, name = user.last_name + " " + user.first_name };
                }

                return tok;
            }
            return null;

        }


        public async Task<long> GetUserId(NpgsqlConnection conn, string email, string password)
        {
            var user = await (from a in _context.users where a.username == email select a).FirstOrDefaultAsync();
            return user.password == password ? user.id : 0;
        }

        public async Task<int> GetUserIdRegister(string email, string password)
        {
            var encryptedPassword = Encrypt(password);
            var user = await (from a in _context.users where a.username == email select a).FirstOrDefaultAsync();
            return user.password == encryptedPassword ? user.id : 0;
        }

        public async Task<bool> CheckAdminUsernameLogin(string username)
        {
            if (await _context.admin_users.AnyAsync(t => t.username == username.Trim()))
            {
                return true;
            }
            return false;
        }




        public async Task<admin_user> GetUserStatus(string email)
        {
            var user = await (from a in _context.admin_users where a.username == email.Trim() select a).FirstOrDefaultAsync();
            return user;
        }

        //public async Task<StudentDetails> GetStudentStatus(string email, int? mySchoolId)
        //{
        //    var user = await (from a in _context.StudentDetails where a.AdmissionNo.Trim() == email.Trim() && a.SchoolId == mySchoolId select a).FirstOrDefaultAsync();
        //    return user;
        //}

        //public async Task<School> GetUserSchool(int? id)
        //{
        //    var school = await (from a in _context.Schools where a.Id == id select a).FirstOrDefaultAsync();
        //    return school;
        //}

        //public async Task<Staff> GetStaff(long id)
        //{
        //    var staff = await (from a in _context.Staff where a.Id == id select a).FirstOrDefaultAsync();
        //    return staff;
        //}


        //public async Task<bool> CheckMemberId(string memberId)
        //{
        //    var user = await (from a in _context.Users where a.Id.ToString() == memberId select a).FirstOrDefaultAsync();
        //    return user != null ? true : false;
        //}

        public object GetData()
        {
            var code = "1295";
            var data = DateTime.UtcNow + code + " True Date";
            return data;
        }


        //public async Task<object> GetAllSchools()
        //{
        //    //var infom = new ShelfDetails();
        //    var books = new object();
        //    //var schools = await _context.Schools.ToListAsync();
        //    using (NpgsqlConnection conn = new NpgsqlConnection(gen.DefaultString))
        //    {
        //        await conn.OpenAsync();
        //        string query3 = @"SELECT * FROM ""Schools""";
        //        NpgsqlCommand cmd2 = new NpgsqlCommand(query3, conn);
        //        var res = await cmd2.ExecuteReaderAsync();
        //        books = gen.sqlDataToJson(res);
        //    }
        //    return books;
        //}



        //public async Task<GroupSchool> GetGroupIdInfo(NpgsqlConnection conn, string email, string password)
        //{
        //    var user = await (from a in _context.GroupSchools where a.Username == email && a.Password == password select a).FirstOrDefaultAsync();
        //    if (user == null)
        //    {
        //        return null;
        //    }
        //    else
        //    {
        //        return user;
        //    }
        //}
    }
}
