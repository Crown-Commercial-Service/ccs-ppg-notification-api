using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Text;

namespace  Ccs.Ppg.Utility.Authorization
{
    public class OrganisationRequestContextFilterMiddleware
    {
        private RequestDelegate _next;

        public OrganisationRequestContextFilterMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, RequestContext requestContext)
        {
            var path = context.Request.Path.Value.TrimStart('/').TrimEnd('/');
            var requestType = path.Contains("organisations") ? RequestType.HavingOrgId : path.Contains("users") ? RequestType.NotHavingOrgId : RequestType.Other;
            if (requestType == RequestType.HavingOrgId)
            {
                requestContext.RequestIntendedOrganisationId = ((dynamic)context.Request).RouteValues["organisationId"]?.ToString();
            }
            else if (requestType == RequestType.NotHavingOrgId)
            {
                if (context.Request.Method == "POST")// POST request includes the org id in the body
                {
                    requestContext.RequestIntendedOrganisationId = await ExtractOrganizationId(context);
                    requestType = RequestType.HavingOrgId;
                    // Reset the request body stream position so the next middleware can read it
                    context.Request.Body.Position = 0;
                }
                requestContext.RequestIntendedUserName = context.Request.Query["user-id"].ToString();
            }
            requestContext.RequestType = requestType;

            await _next(context);
        }

        private static async Task<string> ExtractOrganizationId(HttpContext context)
        {
            using (var reader = new StreamReader(context.Request.Body, encoding: Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true))
            {
                var body = await reader.ReadToEndAsync();
                var userRequestBody = JsonConvert.DeserializeObject<OrganizationInfo>(body);
                return userRequestBody.OrganisationId;
            }
        }
    }
}