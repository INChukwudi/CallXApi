using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Runtime;
using System;
using System.Collections.Generic;

namespace CallXApi.DataModels
{

    public partial class CallXDBContext : DbContext
    {
        string localString = "Server=.\\SQLEXPRESS;Database=NewTest;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=true;";
        string remoteString = "Server=tcp:gradex.database.windows.net,1433;Initial Catalog=gradexdb;Persist Security Info=False;User ID=gradexAdminUser;Password=4VVUHkrL2AbfX2G;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        string localGres = "Host=localhost;Port=5432;Database=callx;Username=postgres;Password=123456";
        //string remoteGres = "Host=gradex-npg-db.postgres.database.azure.com;Port=5432;Database=gradex;Username=gradex_user;Password=nfJYEMifWZ6vE5R;SslMode=require";
        string remoteGres = "Host=gradex-npgsql-db.postgres.database.azure.com;Port=5432;Database=callx;Username=gradex_user;Password=nfJYEMifWZ6vE5R;SslMode=require";

        public CallXDBContext()
        { }

        public CallXDBContext(DbContextOptions<CallXDBContext> options)
            : base(options)
        {
        }

        static CallXDBContext()
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }

        public virtual DbSet<user> Users { get; set; }
        public virtual DbSet<admin_user> Admin_Users { get; set; }
        public virtual DbSet<new_account_otp> New_Account_Otps { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql(localGres);

      

        //partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}

