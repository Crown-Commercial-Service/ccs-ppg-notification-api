namespace  Ccs.Ppg.Utility.Authorization
{
    public class RequestContext
    {
        public int UserId { get; set; }

        public string ServiceClientId { get; set; }

        public int ServiceId { get; set; }

        public string UserName { get; set; }

        public string CiiOrganisationId { get; set; }

        public string IpAddress { get; set; }

        public string Device { get; set; }

        public List<string> Roles { get; set; }

        public RequestType RequestType { get; set; }

        public string RequestIntendedOrganisationId { get; set; }

        public string RequestIntendedUserName { get; set; }
    }
}