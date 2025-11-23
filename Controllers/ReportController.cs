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
            var data = await _context.network_reports
                .OrderByDescending(x => x.datetime_recorded).ToListAsync();

            return Ok(data);
        }

        [HttpGet("GetAllMyReport")]
        public async Task<IActionResult> GetAllMyReport()
        {
            var data = await _context.network_reports.Where(t => t.user_id == myId)
                .OrderByDescending(x => x.datetime_recorded).ToListAsync();

            return Ok(data);
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




    }
}


public class UpdateUserDto
{
    public IFormFile? file { get; set; }
    public string fullname { get; set; }
}

