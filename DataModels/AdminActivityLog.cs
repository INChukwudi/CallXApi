namespace CallXApi.DataModels
{
    public class admin_activity_log
    {
        public int id { get; set; }
        public int admin_id { get; set; }
        public string? ip_address { get; set; }

        public string? platform { get; set; }

        public string? description { get; set; }

        public DateTime? created { get; set; }
    }
}
