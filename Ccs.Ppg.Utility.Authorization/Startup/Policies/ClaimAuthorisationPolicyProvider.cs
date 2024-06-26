﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using  Ccs.Ppg.Utility.Authorization.Services;

namespace  Ccs.Ppg.Utility.Authorization
{
  public class ClaimAuthorisationPolicyProvider : IAuthorizationPolicyProvider
  {
    public DefaultAuthorizationPolicyProvider FallbackPolicyProvider { get; }

    private readonly IHttpContextAccessor _httpContextAccessor;

    public ClaimAuthorisationPolicyProvider(IOptions<AuthorizationOptions> options, IHttpContextAccessor httpContextAccessor)
    {
      FallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
      _httpContextAccessor = httpContextAccessor;
    }

    public async Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
    {
      var xapiKey = _httpContextAccessor.HttpContext.Request.Headers["X-API-Key"];

      if (!policyName.StartsWith(ClaimAuthoriseAttribute.POLICY_PREFIX))
      {
        return await FallbackPolicyProvider.GetPolicyAsync(policyName);
      }

      var policyBuilder = new AuthorizationPolicyBuilder();

      var requestContext = _httpContextAccessor.HttpContext.RequestServices.GetService<RequestContext>();

      if (!string.IsNullOrEmpty(xapiKey)) //  Requests with api key no authorization
      {
        policyBuilder.RequireAssertion(context => true);
      }
      else
      {
        ApplyAuthPolicy(policyBuilder, policyName);
      }

      return await Task.FromResult(policyBuilder.Build());
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
    {
      return ((IAuthorizationPolicyProvider)FallbackPolicyProvider).GetDefaultPolicyAsync();
    }

    public Task<AuthorizationPolicy> GetFallbackPolicyAsync()
    {
      return ((IAuthorizationPolicyProvider)FallbackPolicyProvider).GetFallbackPolicyAsync();
    }

    private void ApplyAuthPolicy(AuthorizationPolicyBuilder policyBuilder, string policyName)
    {
      var claimString = policyName.Substring(ClaimAuthoriseAttribute.POLICY_PREFIX.Length);
      var claimList = claimString.Split(',');
      var authService = _httpContextAccessor.HttpContext.RequestServices.GetService<IAuthService>();

      policyBuilder.RequireAssertion(context =>
      {
        return authService.AuthorizeUser(claimList);
      });
    }
  }
}