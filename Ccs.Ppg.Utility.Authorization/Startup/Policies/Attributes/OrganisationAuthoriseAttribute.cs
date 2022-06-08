﻿using Microsoft.AspNetCore.Authorization;

namespace  Ccs.Ppg.Utility.Authorization
{
    public class OrganisationAuthoriseAttribute : AuthorizeAttribute
    {
        public const string POLICY_PREFIX = "AUTHORISE_ORGANISATION_FOR:";

        public OrganisationAuthoriseAttribute(string requestType)
        {
            Claims = requestType;
        }

        public string Claims
        {
            get
            {
                return Policy.Substring(POLICY_PREFIX.Length);
            }

            set
            {
                Policy = $"{POLICY_PREFIX}{value}";
            }
        }
    }
}