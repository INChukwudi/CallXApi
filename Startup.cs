using Azure.Core.Extensions;
//using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using CallXApi.Services;
using CallXApi.DataModels;
using CallXApi.Models;
using CallXApi.Services;
//using CallXApi.Worker;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.IdentityModel.Tokens;
using CallXApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text;
using System.Text.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static System.Runtime.InteropServices.JavaScript.JSType;
using CallXApi.DataModels;

namespace CallXApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddCors(options => options.AddPolicy("ApiCorsPolicy", builder =>
            //{
            //   builder.WithOrigins("http://localhost:5093").AllowAnyMethod().AllowAnyHeader();
            //}));


            services.AddCors(options => options.AddPolicy("ApiCorsPolicy", builder =>
            {
                builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
            }));

            services.AddMvc().AddJsonOptions(opt => { opt.JsonSerializerOptions.IgnoreNullValues = true; });
            services.AddControllers();
            var appSettingsSection = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);

            // configure jwt authentication
            var appSettings = appSettingsSection.Get<AppSettings>();
            //var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                x.Events = new JwtBearerEvents
                {
                    // OnAuthenticationFailed = context =>
                    // {
                    //     if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                    //     {
                    //         context.Response.StatusCode = 401;
                    //         context.Response.ContentType = "application/json";
                    //         //context.Response.Headers.Append("Access-Control-Allow-Origin", "*"); // Allow all origins, or specify a specific origin
                    //         return context.Response.WriteAsync("Token has expired");
                    //     }
                    //     return Task.CompletedTask;
                    //     //return Task.CompletedTask;
                    // }

                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception is SecurityTokenExpiredException)
                        {
                            context.Response.Headers.Add("Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    }
                };
            });
            //ElpsServices._elpsAppEmail = Configuration.GetSection("ElpsKeys").GetSection("elpsAppEmail").Value.ToString();
            //ElpsServices._elpsBaseUrl = Configuration.GetSection("ElpsKeys").GetSection("elpsBaseUrl").Value.ToString();
            //ElpsServices.public_key = Configuration.GetSection("ElpsKeys").GetSection("PK").Value.ToString();
            //ElpsServices._elpsAppKey = Configuration.GetSection("ElpsKeys").GetSection("elpsSecretKey").Value.ToString();


            services.AddTransient<AccountDb>();
            //services.AddTransient<>();
            //services.AddTransient<SchoolDB>();
            services.AddTransient<Connection>();
            services.AddTransient<GenericService>();

            //services.AddTransient<PaymentDB>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            //services.AddSingleton(sp => new ServiceBusClient(Configuration["Data:Worker:ServiceBus"]));
            //services.AddSingleton<PaymentScheduler>();

            //services.AddScoped(x => new BlobServiceClient(Configuration.GetValue<string>("AzureBlobStorage")));

            services.AddDbContext<CallXDBContext>(options => options.UseNpgsql(
                    Configuration["Data:GradeXGres:RemoteWebDataString"],
                    options => options.EnableRetryOnFailure(
                        maxRetryCount: 6
                    ))
            );

            //services.AddDbContext<GradeXcloudDbContext>(options =>
            //    options.UseSqlServer(Configuration["Data:BK_Connect:ConnectionString"],
            //    options => options.EnableRetryOnFailure(
            //        maxRetryCount: 6,
            //        maxRetryDelay: System.TimeSpan.FromSeconds(30),
            //        errorNumbersToAdd: null)
            //    ));
            //services.Configure<SmtpSettings>(Configuration.GetSection("SmtpSettings"));
            //services.AddSingleton<IMailer, Mailer>();
            //services.AddAzureClients(builder =>
            //{
            //    builder.AddBlobServiceClient(Configuration["ConnectionStrings:AzureBlobStorage:blob"], preferMsi: true);
            //    builder.AddQueueServiceClient(Configuration["ConnectionStrings:AzureBlobStorage:queue"], preferMsi: true);
            //});

        }
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            // global cors policy
            //app.UseCors(x => x
            //   .AllowAnyOrigin()
            //   .AllowAnyMethod()
            //   .SetIsOriginAllowed((host) => true)
            //   .AllowAnyHeader());

            //app.UseCors("ApiCorsPolicy");
            //app.UseMiddleware(typeof(CorsMiddleware));

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(
                options =>
                {
                    options.Run(
                        async context =>
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                            context.Response.ContentType = "text/html";
                            var ex = context.Features.Get<IExceptionHandlerFeature>();
                            if (ex != null)
                            {
                                var err = $"<h1>Error: {ex.Error.Message}</h1>{ex.Error.StackTrace}";
                                await context.Response.WriteAsync(err).ConfigureAwait(false);
                            }
                        });
                }
            );
                app.UseHsts();
            }
        }
    }

    public class CorsMiddleware
    {
        private readonly RequestDelegate _next;

        public CorsMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        // public async Task Invoke(HttpContext context)
        // {
        //     context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
        //     context.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
        //     // Added "Accept-Encoding" to this list
        //     //context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, X-CSRF-Token, X-Requested-With, Accept, Accept-Version, Accept-Encoding, Content-Length, Content-MD5, Date, X-Api-Version, X-File-Name");
        //    // context.Response.Headers.Add("Access-Control-Allow-Methods", "POST,GET,PUT,PATCH,DELETE,OPTIONS");
        //     // New Code Starts here
        //     if (context.Request.Method == "OPTIONS")
        //     {
        //         context.Response.StatusCode = (int)HttpStatusCode.OK;
        //         await context.Response.WriteAsync(string.Empty);
        //     }
        //     // New Code Ends here

        //     await _next(context);
        // }
    }
    //internal static class StartupExtensions
    //{
    //    public static IAzureClientBuilder<BlobServiceClient, BlobClientOptions> AddBlobServiceClient(this AzureClientFactoryBuilder builder, string serviceUriOrConnectionString, bool preferMsi)
    //    {
    //        if (preferMsi && Uri.TryCreate(serviceUriOrConnectionString, UriKind.Absolute, out Uri serviceUri))
    //        {
    //            return builder.AddBlobServiceClient(serviceUri);
    //        }
    //        else
    //        {
    //            return builder.AddBlobServiceClient(serviceUriOrConnectionString);
    //        }
    //    }
    //    public static IAzureClientBuilder<QueueServiceClient, QueueClientOptions> AddQueueServiceClient(this AzureClientFactoryBuilder builder, string serviceUriOrConnectionString, bool preferMsi)
    //    {
    //        if (preferMsi && Uri.TryCreate(serviceUriOrConnectionString, UriKind.Absolute, out Uri serviceUri))
    //        {
    //            return builder.AddQueueServiceClient(serviceUri);
    //        }
    //        else
    //        {
    //            return builder.AddQueueServiceClient(serviceUriOrConnectionString);
    //        }
    //    }
    //}
}


