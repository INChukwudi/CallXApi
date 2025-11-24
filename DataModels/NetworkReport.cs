namespace CallXApi.DataModels
{
    public class network_report
    {
        public int id { get; set; }
        public int user_id { get; set; }
        public DateTime datetime_recorded { get; set; }
        public string? issue_type { get; set; }
        public string? network_provider { get; set; }
        public string? location { get; set; }
        public string? environment { get; set; }
        public string? description { get; set; }
        public int? rating { get; set; }
        public string? client_network_provider { get; set; }
        public string? client_network_digits { get; set; }
        public DateTime created { get; set; }
    }
}
