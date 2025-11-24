namespace CallXApi.DataModels
{
    public class admin_user
    {
        public int id { get; set; }

        public string? first_name { get; set; } = null!;
        public string? last_name { get; set; } = null!;

        public string? phone { get; set; }

        public string? email { get; set; }

        public string? username { get; set; } = null!;

        public string? password { get; set; }
        public string? photo { get; set; }

        public string? user_type { get; set; }
         public string? provider { get; set; }

         public string? department { get; set; }

        public string? status { get; set; }
        public int? created_by { get; set; }

        public DateTime? created { get; set; }

        public DateTime? last_login { get; set; }

        public int? company_id { get; set; }

        public int? role_id { get; set; }
    }
}
