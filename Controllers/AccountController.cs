using CallXApi.DataModels;
using CallXApi.Models;
using CallXApi.Services;

//using CallXAPI.Models;
//using CallXAPI.Services;
using CallXApi.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Win32;
using Newtonsoft.Json;
using Npgsql;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Threading.Tasks;

namespace CallXApi
{
    // [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private AccountDb _account;
        //private SchoolDB _schoolDB;
        public CallXDBContext _context;
        private GenericService gen;
        private EmailService _emailService;
        public IConfiguration _configuration;
        private readonly AppSettings _appSettings;
        public string myConnectString;


        public AccountController(CallXDBContext context, GenericService generic, EmailService emailService, IConfiguration configuration, AccountDb account, IOptions<AppSettings> appSettings)
        {
            ///_httpContextAccessor = httpContextAccessor;
            //_schoolDB = schoolDB;
            gen = generic;
            _appSettings = appSettings.Value;
            _account = account;
            _context = context;
            _configuration = configuration;
            myConnectString = generic.DefaultString;
            _emailService = emailService;
        }

        [HttpPost("AdminLogin")]
        public async Task<IActionResult> AdminLogin([FromBody] Login creds)
        {
            try
            {
                //creds.password = WallCrypto.DecryptStringAES(creds.password);
                //creds.userName = WallCrypto.DecryptStringAES(creds.userName);

                if (await _account.CheckAdminUsernameLogin(creds.email?.Trim()))
                {
                    var user = await _account.AuthenticateAdmin(creds?.email?.Trim(), creds?.password?.Trim());

                    if (user == null)
                    {
                        return Ok(new UserToken { code = 2 });
                    }
                    //await UpdateLastLogin(myId);
                    return Ok(user);
                }
                return Ok(new UserToken { code = 3 });
            }
            catch (Exception ex)
            {
                //await er.LogError(ex);

                return Ok(new UserToken { code = 0, status = ex.Message });
            }
        }


        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] Login creds)
        {
            try
            {
                //creds.password = WallCrypto.DecryptStringAES(creds.password);
                //creds.userName = WallCrypto.DecryptStringAES(creds.userName);
                using (NpgsqlConnection conn = new NpgsqlConnection(myConnectString))
                {
                    await conn.OpenAsync();
                    if (await _account.CheckUser(creds.email) > 0)
                    {
                        var user = await _account.Authenticate(creds.email, creds.password);

                        if (user == null)
                        {
                            return Ok(new UserToken { code = 2 });
                        }
                        //await UpdateLastLogin(myId);
                        return Ok(user);
                    }
                    return Ok(new UserToken { code = 3 });
                }
            }
            catch (Exception ex)
            {
                //await er.LogError(ex);
                return Ok(new UserToken { code = 0, status = ex.Message });
            }
        }


        [HttpGet("GetData")]
        public object GetData()
        {
            try
            {
                return _account.GetData();
            }
            catch (Exception ex)
            {
                return new { message = ex.Message, trace = ex.StackTrace };
            }
        }

        //[HttpGet("GetSchools")]
        //public async Task<object> GetSchools()
        //{
        //    try
        //    {
        //        var dava = await _account.GetAllSchools();
        //        return dava;
        //    }
        //    catch (Exception ex)
        //    {
        //        return new { message = ex.Message, trace = ex.StackTrace };
        //    }
        //}


        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] Register model)
        {
            try
            {
                int? myuserId;
                //var isMember = await _account.CheckUser(model.email);
                //if (isMember == 0)
                //{
                    using (NpgsqlConnection conn = new NpgsqlConnection(myConnectString))
                    {
                        await conn.OpenAsync();
                        var mycheck = await _account.CheckUser(model.email);
                        if (mycheck > 0)
                        {
                            return BadRequest(new { message = "This email has already been registered!" });
                        }

                        string query = @"INSERT INTO users (username, password, created, status) 
                       VALUES(@username, @password, @created, @status)";

                        NpgsqlCommand cmd = new NpgsqlCommand(query, conn);
                        //cmd.Parameters.AddWithValue("@Username", model.userName);
                        cmd.Parameters.AddWithValue("@username", model.email.Trim());
                        cmd.Parameters.AddWithValue("@password", _account.Encrypt(model.password.Trim()));
                        //cmd.Parameters.AddWithValue("@MemberId", model.memberId);
                        cmd.Parameters.AddWithValue("@created", DateTime.UtcNow);
                        cmd.Parameters.AddWithValue("@status", "ACTIVE");

                        await cmd.ExecuteNonQueryAsync();

                        myuserId = await _account.GetUserIdRegister(model.email, model.password.Trim());

                    }
                    return Ok(_account.AuthReg(myuserId));
                //}
                //else
                //{
                //    return BadRequest(new { message = "Member ID already registered!" });
                //}
            }
            catch (Exception ex)
            {
                //await er.LogError(ex);
                return BadRequest(new { message = ex.Message });
            }
        }





        [HttpPost("new-account-otp")]
        public async Task<IActionResult> NewAccountOtp([FromBody] OtpRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest(new { error = "Email required" });

            string otp = new Random().Next(1000, 9999).ToString();

            try
            {
                await using var conn = new NpgsqlConnection(myConnectString);
                await conn.OpenAsync();
                await using var tx = await conn.BeginTransactionAsync();

                try
                {
                    // Check if record exists
                    var existsCmd = new NpgsqlCommand(
                        "SELECT 1 FROM new_account_otps WHERE email = @email LIMIT 1",
                        conn, tx);
                    existsCmd.Parameters.AddWithValue("email", request.Email);

                    bool exists = (await existsCmd.ExecuteScalarAsync()) != null;

                    if (exists)
                    {
                        var updateCmd = new NpgsqlCommand(
                            @"UPDATE new_account_otps
                          SET token = @otp, created = NOW() 
                          WHERE email = @email",
                            conn, tx);
                        updateCmd.Parameters.AddWithValue("email", request.Email);
                        updateCmd.Parameters.AddWithValue("otp", otp);
                        await updateCmd.ExecuteNonQueryAsync();
                    }
                    else
                    {
                        var insertCmd = new NpgsqlCommand(
                            @"INSERT INTO new_account_otps (email, token, created) 
                          VALUES (@email, @otp, NOW())",
                            conn, tx);
                        insertCmd.Parameters.AddWithValue("email", request.Email);
                        insertCmd.Parameters.AddWithValue("otp", otp);
                        await insertCmd.ExecuteNonQueryAsync();
                    }

                    await tx.CommitAsync();
                }
                catch (Exception ex)
                {
                    await tx.RollbackAsync();
                    throw;
                }

                // Send email through your bus
                await _emailService.SendOTPEmailAsync(request.Email, otp);

                return Ok(new { success = true, message = "OTP sent" });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(500, new { error = "Server error" });
            }
        }

        // ---------------------------------------------
        // 2) VERIFY OTP
        // ---------------------------------------------
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Otp))
                return BadRequest(new { error = "Missing fields" });

            try
            {
                await using var conn = new NpgsqlConnection(myConnectString);
                await conn.OpenAsync();

                var selectCmd = new NpgsqlCommand(
                    @"SELECT email, token, created 
                  FROM new_account_otps
                  WHERE email = @email 
                  LIMIT 1",
                    conn);

                selectCmd.Parameters.AddWithValue("email", request.Email);

                using var reader = await selectCmd.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                    return BadRequest(new { error = "Invalid OTP" });

                string savedToken = reader.GetString(reader.GetOrdinal("token"));
                DateTime created = reader.GetDateTime(reader.GetOrdinal("created"));

                if (savedToken != request.Otp)
                    return BadRequest(new { error = "Invalid OTP" });

                // Expiry check (10 mins)
                if (DateTime.UtcNow - created.ToUniversalTime() > TimeSpan.FromMinutes(10))
                    return BadRequest(new { error = "OTP expired" });

                // Generate reset token
                string resetToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

                reader.Close();

                var updateCmd = new NpgsqlCommand(
                    @"UPDATE new_account_otps
                  SET token = @newToken, created = NOW()
                  WHERE email = @email",
                    conn);

                updateCmd.Parameters.AddWithValue("newToken", resetToken);
                updateCmd.Parameters.AddWithValue("email", request.Email);

                await updateCmd.ExecuteNonQueryAsync();

                return Ok(new { success = true, resetToken });
            }
            catch (Exception ex)
            {
                Console.WriteLine("verify_otp failed: " + ex.Message);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }



        [HttpGet("MyAd")]
        public async Task<IActionResult> MyAd()
        {
           
            var data = await (from a in _context.admin_users select a).ToListAsync();
            return Ok(data);
        }



    }
}



public class OtpRequest
{
    public string Email { get; set; }
}

public class VerifyOtpRequest
{
    public string Email { get; set; }
    public string Otp { get; set; }
}
