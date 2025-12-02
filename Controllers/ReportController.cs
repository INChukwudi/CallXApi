using CallXApi.DataModels;
using CallXApi.Models;
using CallXApi.Services;
using CallXApi.ViewModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private AccountDb _account;
        private ReportDb _reportDb;
        public CallXDBContext _context;
        private GenericService gen;
        private EmailService _emailService;
        public IConfiguration _configuration;
        private readonly AppSettings _appSettings;
        public string myConnectString;



        public ReportController(CallXDBContext context, GenericService generic, EmailService emailService, IConfiguration configuration, AccountDb account, IOptions<AppSettings> appSettings, ReportDb reportDb)
        {
            ///_httpContextAccessor = httpContextAccessor;
            _reportDb = reportDb;
            gen = generic;
            _appSettings = appSettings.Value;
            _account = account;
            _context = context;
            _configuration = configuration;
            myConnectString = generic.DefaultString;
            _emailService = emailService;
        }

        private int myId => Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier).Value);


        [HttpPost("CreateReport")]
        public async Task<IActionResult> CreateReport(network_report report)
        {
            report.datetime_recorded = report.datetime_recorded == default ? DateTime.UtcNow : report.datetime_recorded;
            report.created = DateTime.UtcNow;
            report.user_id = myId;
            await _context.network_reports.AddAsync(report);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Saved successfully", data = report });
        }


        [HttpGet("GetReportById")]
        public async Task<IActionResult> GetReportById(int id)
        {
            var record = await _context.network_reports.FindAsync(id);

            if (record == null)
                return NotFound(new { error = "Record not found" });

            return Ok(record);
        }


        [HttpGet("GetAllReport")]
public async Task<IActionResult> GetAllReport()
{
    var data = await (
        from a in _context.network_reports
        join b in _context.users on a.user_id equals b.id
        orderby a.datetime_recorded
        select new
        {
            a.id,
            a.datetime_recorded,
            a.experience_type,
            a.report_category,
            a.network_provider,
            a.location,
            a.environment,
            a.rating,
            a.client_network_provider,
            a.user_id,
            a.client_network_digits,
            a.description,
            a.call_direction,
            passport = b.passport,
            fullname = b.surname + " " + b.first_name,
            email = b.username,
            a.network_type,
            a.state,
            a.device_model
        }
    ).ToListAsync();

   // await this.LogAdminActivity("Viewed Reports");

    return Ok(data);
}



        // [HttpGet("GetAllReport")]
        // public async Task<IActionResult> GetAllReport()
        // {
        //     var data = await (from a in _context.network_reports join b in _context.users on a.user_id equals b.id orderby a.datetime_recorded select a).ToListAsync();

        //     return Ok(data);
        // }

        [HttpGet("GetAllMyReport")]
        public async Task<IActionResult> GetAllMyReport()
        {
            var data = await _context.network_reports.Where(t => t.user_id == myId)
                .OrderByDescending(x => x.datetime_recorded).ToListAsync();

            return Ok(data);
        }

        [HttpGet("GetAllOperators")]
        public async Task<IActionResult> GetAllOperators()
        {
            var data = await _context.admin_users.Where(t => t.role_id == 2)
                .OrderByDescending(x => x.created).ToListAsync();
            return Ok(data);
        }

        [HttpGet("MyUserProfile")]
        public async Task<IActionResult> MyUserProfile()
        {
           
            var data = await (from a in _context.users where a.id == myId select new
            {
                passport = a.passport,
                fullname = a.surname + " " + a.first_name
            }).FirstOrDefaultAsync();
            return Ok(data);
        }

         [HttpGet("MyOperatorProfile")]
        public async Task<IActionResult> MyOperatorProfile()
        {
           
            var data = await (from a in _context.admin_users where a.id == myId select new
            {
                a.id,
            fullname = a.last_name + " " + a.first_name,
            a.provider,
            a.email,
            a.username,
            a.role_id,
            a.status,
            last_login = (int)(DateTime.UtcNow - a.last_login).Value.TotalDays,
            a.created,
            a.department,
            a.photo,
            a.phone
            }).FirstOrDefaultAsync();
            return Ok(data);
        }


        //  [HttpGet("GetOperatorProfile")]
        // public async Task<IActionResult> GetOperatorProfile(string operatorId)
        // {
           
        //     var data = await (from a in _context.admin_users where a.id == Convert.ToInt32(operatorId) select new
        //     {
        //         a.id,
        //     fullname = a.last_name + " " + a.first_name,
        //     a.provider,
        //     a.email,
        //     a.username,
        //     a.role_id,
        //     a.status,
        //     last_login = (int)(DateTime.UtcNow - a.last_login).Value.TotalDays,
        //     a.created,
        //     a.department,
        //     a.photo,
        //     a.phone
        //     }).FirstOrDefaultAsync();
        //     return Ok(data);
        // }

[HttpGet("GetOperatorProfile")]
public async Task<IActionResult> GetOperatorProfile(string operatorId)
{
    try
    {
        int opId = Convert.ToInt32(operatorId);

        // ⭐ Get profile
        var profile = await (from a in _context.admin_users
                             where a.id == opId
                             select new
                             {
                                 a.id,
                                 fullname = a.last_name + " " + a.first_name,
                                 a.provider,
                                 a.email,
                                 a.username,
                                 a.role_id,
                                 a.status,
                                 last_login = a.last_login == null ? (int?)null : (int)(DateTime.UtcNow - a.last_login.Value).TotalDays,
                                 a.created,
                                 a.department,
                                 a.photo,
                                 a.phone
                             })
                             .FirstOrDefaultAsync();

        if (profile == null)
            return NotFound("Operator not found");

        // ⭐ Get activity logs for this operator
        var logs = await _context.admin_activity_logs
            .Where(x => x.admin_id == opId)
            .OrderByDescending(x => x.created)
            .Select(x => new
            {
                x.id,
                x.description,
                x.ip_address,
                x.platform,
                x.created
            })
            .ToListAsync();

        // ⭐ Merge both results
        var result = new
        {
            profile,
            activity_logs = logs
        };

        return Ok(result);
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { error = ex.Message });
    }
}


[HttpGet("GetAdminProfile")]
public async Task<IActionResult> GetAdminProfile()
{
    try
    {

        // ⭐ Get profile
        var profile = await (from a in _context.admin_users
                             where a.id ==  myId
                             select new
                             {
                                 a.id,
                                 fullname = a.last_name + " " + a.first_name,
                                 a.provider,
                                 a.email,
                                 a.username,
                                 a.role_id,
                                 a.status,
                                 last_login = a.last_login == null ? (int?)null : (int)(DateTime.UtcNow - a.last_login.Value).TotalDays,
                                 a.created,
                                 a.department,
                                 a.photo,
                                 a.phone
                             })
                             .FirstOrDefaultAsync();

        if (profile == null)
            return NotFound("Operator not found");

        // ⭐ Get activity logs for this operator
        var logs = await _context.admin_activity_logs
            .Where(x => x.admin_id == myId)
            .OrderByDescending(x => x.created)
            .Select(x => new
            {
                x.id,
                x.description,
                x.ip_address,
                x.platform,
                x.created
            })
            .ToListAsync();

        // ⭐ Merge both results
        var result = new
        {
            profile,
            activity_logs = logs
        };

        return Ok(result);
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { error = ex.Message });
    }
}



        [HttpPost("UpdateUser")]
        public async Task<IActionResult> UpdateUser([FromForm] UpdateUserDto model)
        {
            try
            {
                // ---- 1. Split fullname ----------------------------------
                if (string.IsNullOrWhiteSpace(model.fullname))
                    return BadRequest("Fullname is required");

                var parts = model.fullname.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

                string surname = parts[0];
                string firstName = parts.Length > 1 ? parts[1] : "";
                string middleName = parts.Length > 2 ? string.Join(" ", parts.Skip(2)) : "";

                // ---- 2. Get user from DB ---------------------------------
                var user = await _context.users.FirstOrDefaultAsync(u => u.id == myId);
                if (user == null)
                    return NotFound("User not found");

                // ---- 3. Upload passport if file included -----------------
                if (model.file != null)
                {
                    var fileData = await _reportDb.UploadSchoolImageRemote(model.file);
                    user.passport = fileData.Value; // blob URL
                }

                // ---- 4. Update user fields -------------------------------
                user.surname = surname;
                user.first_name = firstName;
                user.middle_name = middleName;

                // ---- 5. Save changes -------------------------------------
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "User updated successfully",
                    user
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }


        // [HttpPost("UpdateUser")]
        // public async Task<IActionResult> UpdateAdminUser([FromForm] UpdateUserDto model)
        // {
        //     try
        //     {
        //         // ---- 1. Split fullname ----------------------------------
        //         if (string.IsNullOrWhiteSpace(model.fullname))
        //             return BadRequest("Fullname is required");

        //         var parts = model.fullname.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        //         string surname = parts[0];
        //         string firstName = parts.Length > 1 ? parts[1] : "";
        //         string middleName = parts.Length > 2 ? string.Join(" ", parts.Skip(2)) : "";

        //         // ---- 2. Get user from DB ---------------------------------
        //         var user = await _context.users.FirstOrDefaultAsync(u => u.id == myId);
        //         if (user == null)
        //             return NotFound("User not found");

        //         // ---- 3. Upload passport if file included -----------------
        //         if (model.file != null)
        //         {
        //             var fileData = await _reportDb.UploadSchoolImageRemote(model.file);
        //             user.passport = fileData.Value; // blob URL
        //         }

        //         // ---- 4. Update user fields -------------------------------
        //         user.surname = surname;
        //         user.first_name = firstName;
        //         user.middle_name = middleName;

        //         // ---- 5. Save changes -------------------------------------
        //         await _context.SaveChangesAsync();

        //         return Ok(new
        //         {
        //             message = "User updated successfully",
        //             user
        //         });
        //     }
        //     catch (Exception ex)
        //     {
        //         return StatusCode(500, new { error = ex.Message });
        //     }
        // }

        [HttpPost("UpdateAdminProfile")]
        public async Task<IActionResult> UpdateAdminProfile([FromForm] UpdateAdminDto model)
        {
            try
            {

                // ---- 2. Get user from DB ---------------------------------
                var user = await _context.admin_users.FirstOrDefaultAsync(u => u.id == myId);
                if (user == null)
                    return NotFound("User not found");

                // ---- 3. Upload passport if file included -----------------
                if (model.file != null)
                {
                    var fileData = await _reportDb.UploadSchoolImageRemote(model.file);
                    user.photo = fileData.Value; // blob URL
                }

                // ---- 4. Update user fields -------------------------------
                user.last_name = model?.lastname;
                user.first_name = model?.firstname;
                user.department = model?.department;

                // ---- 5. Save changes -------------------------------------
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "User updated successfully",
                    user
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }



         [HttpPost("UpdateAdminPassword")]
        public async Task<IActionResult> UpdateAdminPassword([FromForm] PasswordObj model)
        {
            try
            {

                // ---- 2. Get user from DB ---------------------------------
                var user = await _context.admin_users.FirstOrDefaultAsync(u => u.id == myId);
                if (user == null)
                    return NotFound("User not found");

                // ---- 4. Update user fields -------------------------------
                user.password = _account.Encrypt(model?.password);

                // ---- 5. Save changes -------------------------------------
                await _context.SaveChangesAsync();
                await this.LogAdminActivity("Updated own password");

                return Ok(new
                {
                    message = "User updated successfully",
                    user
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }


        [HttpPost("RegisterOperator")]
        public async Task<IActionResult> RegisterOperator([FromBody] RegisterOperator model)
        {
            try
            {
                    int? myuserId;
                    using (NpgsqlConnection conn = new NpgsqlConnection(myConnectString))
                    {
                        await conn.OpenAsync();
                        var mycheck = await _account.CheckAdminUser(model.email?.Trim());
                        if (mycheck > 0)
                        {
                            return BadRequest(new { message = "This email has already been registered!" });
                        }

                        string query = @"INSERT INTO admin_users (username, password, provider, created_by, created, role_id, status) 
                       VALUES(@username, @password, @provider, @created_by, @created, @role_id, @status)";

                        NpgsqlCommand cmd = new NpgsqlCommand(query, conn);
                        //cmd.Parameters.AddWithValue("@Username", model.userName);
                        cmd.Parameters.AddWithValue("@username", model.email.Trim());
                        cmd.Parameters.AddWithValue("@password", _account.Encrypt(model.password.Trim()));
                        cmd.Parameters.AddWithValue("@provider", model.provider.Trim());
                        cmd.Parameters.AddWithValue("@status", "ACTIVE");
                        cmd.Parameters.AddWithValue("@created_by", myId);
                        cmd.Parameters.AddWithValue("@created", DateTime.UtcNow);
                        cmd.Parameters.AddWithValue("@role_id", 2);
                        //cmd.Parameters.AddWithValue("@status", "ACTIVE");

                        await cmd.ExecuteNonQueryAsync();
                        await this.LogAdminActivity("Created an account");

                        await _emailService.SendPasswordEmailAsync(model.email.Trim(), model.password.Trim());

                        //myuserId = await _account.GetUserIdRegister(model.email, model.password.Trim());

                    }
                    return Ok();
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

        [HttpPost("RecordAdminActivity")]
public async Task<IActionResult> RecordAdminActivity(string description)
{
    try
    {
        if (string.IsNullOrWhiteSpace(description))
            return BadRequest("Description is required");

        await LogAdminActivity(description);

        return Ok();
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { error = ex.Message });
    }
}



        [HttpGet("device-info")]
    public IActionResult GetDeviceInfo()
    {
        // Get IP Address
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

        // Get User-Agent (device name)
        var deviceName = Request.Headers["User-Agent"].ToString();

        // Return result
        return Ok(new
        {
            ipAddress = ip,
            device = deviceName
        });
    }

    private async Task LogAdminActivity(string description)
{
    var activity = new admin_activity_log
    {
        admin_id = myId,
        ip_address = HttpContext.Connection.RemoteIpAddress?.ToString(),
        platform = Request.Headers["User-Agent"].ToString(),
        description = description,
        created = DateTime.UtcNow
    };

    await _context.admin_activity_logs.AddAsync(activity);
    await _context.SaveChangesAsync();
}

        

    }
}


public class UpdateUserDto
{
    public IFormFile? file { get; set; }
    public string fullname { get; set; }
}


public class UpdateAdminDto
{
    public IFormFile? file { get; set; }
    public string? lastname { get; set; }
    public string? firstname { get; set; }
    public string? department { get; set; }
}

public class PasswordObj
{
    public string? password { get; set; }
}

