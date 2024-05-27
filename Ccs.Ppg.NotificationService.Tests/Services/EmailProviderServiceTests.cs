using Amazon.Runtime;
using Ccs.Ppg.NotificationService.Model;
using Ccs.Ppg.NotificationService.Services;
using Ccs.Ppg.NotificationService.Services.IServices;
using Ccs.Ppg.NotificationService.Tests.Infrastructure;
using Ccs.Ppg.Utility.Constants.Constants;
using Ccs.Ppg.Utility.Exceptions.Exceptions;
using Moq;
using Notify.Client;
using Notify.Interfaces;
using Notify.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static System.Net.WebRequestMethods;

namespace Ccs.Ppg.NotificationService.Tests.Services
{
    public class EmailProviderServiceTests
    {
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly Mock<IWrapperConfigurationService> _mockWrapperConfigurationService;
        private readonly ApplicationConfigurationInfo _mockApplicationConfigurationInfo;

        public EmailProviderServiceTests()
        {
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockWrapperConfigurationService = new Mock<IWrapperConfigurationService>();
            _mockApplicationConfigurationInfo = new ApplicationConfigurationInfo();
        }

        public static List<string> SetupConfigRoles()
        {
            List<string> configRoles = new List<string>();
            configRoles.AddRange(new List<string> { "sampleRole1", "sampleRole2", "sampleRole3" });
            return configRoles;
        }

        public class SendEmail
        {
          public static IEnumerable<object[]> InvalidEmailTestData => new List<object[]>
          {
            new object[]
            {
                // InvalidToEmail
                DtoHelper.GetEmailInfoRequest("newttestoorg", Guid.NewGuid().ToString(), new Dictionary<string, string>
                {
                    { "link", "https://sample.test.check.com" },
                    { "emailid", "changeuserpwd@yopmail.com" }
                }),
            },
            new object[]
            {
                // InvalidTemplateId
                DtoHelper.GetEmailInfoRequest("abc@yopmail.com", "12345", new Dictionary<string, string>
                {
                    { "link", "https://sample.test.check.com" },
                    { "emailid", "changeuserpwd@yopmail.com" }
                })
            },
            new object[]
            {
                // InvalidEmailAddress
                DtoHelper.GetEmailInfoRequest("newttestoorg", Guid.NewGuid().ToString(), new Dictionary<string, string>
                {
                    { "link", "https://sample.test.check.com" },
                    { "emailid", "changeuserpwd" }
                }),
            },
            new object[]
            {
                // InvalidCcsMessage
                DtoHelper.GetEmailInfoRequest("newttestoorg@yopmail.com", Guid.NewGuid().ToString(), new Dictionary<string, string>
                {
                    { "link", "https://sample.test.check.com" },
                    { "CCSMsg", "InvalidMessage" }
                })
            },
            new object[]
            {
                // InvalidServiceNames
                DtoHelper.GetEmailInfoRequest("newttestoorg@yopmail.com", Guid.NewGuid().ToString(), new Dictionary<string, string>
                {
                    { "orgName", "TestingOrg" },
                    { "serviceNames", "Invalid_Service" },
                    { "link", "https://sample.test.check.com" }
                })
            },
            new object[]
            {
                // InvalidName
                DtoHelper.GetEmailInfoRequest("newttestoorg@yopmail.com", Guid.NewGuid().ToString(), new Dictionary<string, string>
                {
                    { "firstname", "INVALID_NAME_EXCEEDS_MAX_LENGTH" },
                    { "lastname", "TestUser" },
                    { "email", "newtestuser@yopmail.com" }
                })
            },
            new object[]
            {
                // InvalidLink
                DtoHelper.GetEmailInfoRequest("newttestoorg@yopmail.com", Guid.NewGuid().ToString(), new Dictionary<string, string>
                {
                    { "OrgRegistersationlink", "INVALID_LINK" },
                    { "emailaddress", "newtestuser@yopmail.com" }
                })
            },
            new object[]
            {
                // InvalidOrgName
                DtoHelper.GetEmailInfoRequest("newttestoorg@yopmail.com", Guid.NewGuid().ToString(), new Dictionary<string, string>
                {
                    { "orgName", "" },
                    { "link", "https://sample.test.check.com" }
                })
            },
            new object[]
            {
                // InvalidSigninProviders
                DtoHelper.GetEmailInfoRequest("newttestoorg@yopmail.com", Guid.NewGuid().ToString(), new Dictionary<string, string>
                {
                    { "sigininproviders", "Invalid_Providers" },
                    { "link", "https://sample.test.check.com" }
                })
            }
          };

            [Theory]
            [MemberData(nameof(InvalidEmailTestData))]
            public async Task InvalidDataException_WhenSendEmailAsync(EmailInfo mailRequest)
            {
                var mockWrapperConfigurationService = new Mock<IWrapperConfigurationService>();
                mockWrapperConfigurationService.Setup(s => s.GetServices()).ReturnsAsync(SetupConfigRoles());
                ApplicationConfigurationInfo applicationConfigurationInfo = new ApplicationConfigurationInfo()
                {
                    NotificationValidationConfigurations = new NotificationValidationConfigurations()
                    {
                        EnableValidation = true,
                        OrgNameLegnth = 25,
                        EmailRegex = "^\\s?([\\w!#$%+&'*-/=?^_`{|}~][^,]*)@([\\w\\.\\-]+)((\\.(\\w){1,1000})+)\\s?$",
                        FirstNameLength = 10,
                        LastNameLength = 10,
                        SignInProviders = new List<string> { "Twitter,Google,Apple,Amazon" },
                        LinkRegex = "^https?:\\/\\/([-a-zA-Z0-9@._]{1,25}\\.test\\.check\\.com)(?:\\/.*)?$\r\n",
                        CcsMsg = "Check before proceeding with the email link"
                    },
                    EmailSettings = new EmailSettings()
                    {
                        ApiKey = "TestKey"
                    }
                };
                var bodyContent = new Dictionary<string, dynamic>();
                mailRequest.BodyContent.ToList().ForEach(pair => bodyContent.Add(pair.Key, pair.Value));
                var httpClientFactoryMock = new Mock<IHttpClientFactory>();
                var httpClientMock = new Mock<HttpClient>();
                var httpClientWrapperMock = new Mock<HttpClientWrapper>(httpClientMock.Object);
                httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClientMock.Object);
                var emailProviderService = new EmailProviderService(httpClientFactoryMock.Object, mockWrapperConfigurationService.Object, applicationConfigurationInfo);
                var ex = await Assert.ThrowsAsync<CcsSsoException>(() => emailProviderService.SendEmailAsync(mailRequest));
                Assert.Equal(ErrorConstant.ErrorInvalidDetails, ex.Message);
            }

            public class ValidateEmailMessage
            {
              public static IEnumerable<object[]> InvalidEmailContent => new List<object[]>
              {
                new object[]
                {
                    new Dictionary<string, dynamic>
                    {
                        { "link", "https://dev.invalid.domain/reset" }
                    }
                },
                new object[]
                {
                    new Dictionary<string, dynamic>
                    {
                        { "orgname", "INVALID_ORGNAME_LENGTH" }
                    }
                },
                new object[]
                {
                    new Dictionary<string, dynamic>
                    {
                        { "emailid", "testuser" }
                    }
                },
                new object[]
                {
                    new Dictionary<string, dynamic>
                    {
                        { "firstname", "testuserinvalidlength" }
                    }
                },
                new object[]
                {
                    new Dictionary<string, dynamic>
                    {
                        { "lastname", "testuserinvalidlength" }
                    }
                },
                new object[]
                {
                    new Dictionary<string, dynamic>
                    {
                        { "servicename", "INVALID_SERVICE_NAME" }
                    }
                },
                new object[]
                {
                    new Dictionary<string, dynamic>
                    {
                        { "sigininproviders", "NOVA" }
                    }
                },
                new object[]
                {
                    new Dictionary<string, dynamic>
                    {
                        { "ccsmsg", "INVALID_CCS_MESSAGE" }
                    }
                }
              };

                [Theory]
                [MemberData(nameof(InvalidEmailContent))]
                public async Task ReturnsFalse_WhenInvalidEmailContent(Dictionary<string, dynamic> emailContent)
                {
                    var mockWrapperConfigurationService = new Mock<IWrapperConfigurationService>();
                    mockWrapperConfigurationService.Setup(s => s.GetServices()).ReturnsAsync(SetupConfigRoles());
                    ApplicationConfigurationInfo applicationConfigurationInfo = new ApplicationConfigurationInfo()
                    {
                        NotificationValidationConfigurations = new NotificationValidationConfigurations()
                        {
                            EnableValidation = true,
                            OrgNameLegnth = 10,
                            EmailRegex = "^\\s?([\\w!#$%+&'*-/=?^_`{|}~][^,]*)@([\\w\\.\\-]+)((\\.(\\w){1,1000})+)\\s?$",
                            FirstNameLength = 10,
                            LastNameLength = 10,
                            SignInProviders = new List<string> { "Twitter,Google,Apple,Amazon" },
                            LinkRegex = "^https?:\\/\\/([-a-zA-Z0-9@._]{1,25}\\.test\\.check\\.com)(?:\\/.*)?$\r\n",
                            CcsMsg = "Check before proceeding with the email link"
                        }
                    };
                    var emailProviderService = new EmailProviderService(null, mockWrapperConfigurationService.Object, applicationConfigurationInfo);
                    Assert.False(await emailProviderService.ValidateEmailMessage(emailContent));
                }

              public static IEnumerable<object[]> ValidEmailContent => new List<object[]>
              {
                new object[]
                {
                    new Dictionary<string, dynamic>
                    {
                        { "link", "https://sample.test.check.com/" }
                    }
                },
                new object[]
                {
                    new Dictionary<string, dynamic>
                    {
                        { "orgname", "TestOrg" }
                    }
                },
                new object[]
                {
                    new Dictionary<string, dynamic>
                    {
                        { "emailid", "testuser@yopmail.com" }
                    }
                },
                new object[]
                {
                    new Dictionary<string, dynamic>
                    {
                        { "firstname", "testuser" }
                    }
                },
                new object[]
                {
                    new Dictionary<string, dynamic>
                    {
                        { "lastname", "check" }
                    }
                },
                new object[]
                {
                    new Dictionary<string, dynamic>
                    {
                        { "servicename", "sampleRole1,sampleRole2,sampleRole3" }
                    }
                },
                new object[]
                {
                    new Dictionary<string, dynamic>
                    {
                        { "sigininproviders", "Google" }
                    }
                },
                new object[]
                {
                    new Dictionary<string, dynamic>
                    {
                        { "ccsmsg", "Check before proceeding with the email link" }
                    }
                }
              };

                [Theory]
                [MemberData(nameof(ValidEmailContent))]
                public async Task ReturnsTrue_WhenValidEmailContent(Dictionary<string, dynamic> emailContent)
                {
                    var mockWrapperConfigurationService = new Mock<IWrapperConfigurationService>();
                    mockWrapperConfigurationService.Setup(s => s.GetServices()).ReturnsAsync(SetupConfigRoles());
                    ApplicationConfigurationInfo applicationConfigurationInfo = new ApplicationConfigurationInfo()
                    {
                        NotificationValidationConfigurations = new NotificationValidationConfigurations()
                        {
                            EnableValidation = true,
                            OrgNameLegnth = 10,
                            EmailRegex = "^\\s?([\\w!#$%+&'*-/=?^_`{|}~][^,]*)@([\\w\\.\\-]+)((\\.(\\w){1,1000})+)\\s?$",
                            FirstNameLength = 10,
                            LastNameLength = 10,
                            SignInProviders = new List<string> { "Twitter", "Google", "Apple", "Amazon" },
                            LinkRegex = @"^https?:\/\/([-a-zA-Z0-9@._]{1,50}\.test\.check\.com)(?:\/.*)?$",
                            CcsMsg = "Check before proceeding with the email link"
                        }
                    };
                    var emailProviderService = new EmailProviderService(null, mockWrapperConfigurationService.Object, applicationConfigurationInfo);
                    Assert.True(await emailProviderService.ValidateEmailMessage(emailContent));
                }
            }
        }
    }


}
