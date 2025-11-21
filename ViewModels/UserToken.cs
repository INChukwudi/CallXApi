namespace CallXApi.ViewModels
{
     public class UserToken
    {
        public long? id { get; set; }
        public string? token { get; set; }
        public int code { get; set; }
        public string? status { get; set; }
        public string? name { get; set; }
        public string? logo { get; set; }
        public int? schoolId {get; set;}
        public string? schoolName {get; set;}
        public string? photo {get; set;}
    }
}
