using Ccs.Ppg.NotificationService.Model;
using Ccs.Ppg.NotificationService.Services;
using Ccs.Ppg.NotificationService.Tests.Infrastructure;
using Moq;
using Notify.Client;
using Xunit;

namespace Ccs.Ppg.NotificationService.Tests.Services
{
    public class MessageProviderServiceTests
    {
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly ApplicationConfigurationInfo _mockApplicationConfigurationInfo;

        public MessageProviderServiceTests(IHttpClientFactory httpClientFactory, ApplicationConfigurationInfo applicationConfigurationInfo)
        {
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockApplicationConfigurationInfo = new ApplicationConfigurationInfo();
        }

        public class SendMessage
        {
           MessageInfo msgInfo1 = new MessageInfo
           {
                key = "key",
                Message = "value",
           };

          public static IEnumerable<object[]> InvalidSMSData => new List<object[]>
          {
            new object[]
            {
                DtoHelper.GetSMSInfoRequest("1234567890", "templateId", new List<MessageInfo>
                {
                    new MessageInfo
                    {
                        key = "sms",
                        Message = "INVALID_SMS_MESSAGE_LENGTH",
                    }
                })
            }
          };

            [Theory]
            [MemberData(nameof(InvalidSMSData))]
            public async Task InvalidDetailsException_WhenSendSMS(MessageRequestModel messageRequestInfo)
            {
                ApplicationConfigurationInfo applicationConfigurationInfo = new ApplicationConfigurationInfo()
                {
                    NotificationValidationConfigurations = new NotificationValidationConfigurations()
                    {
                        EnableValidation = true,
                        SmsMsgLength = 10
                    },
                    EmailSettings = new EmailSettings()
                    {
                        ApiKey = "TestKey"
                    }
                };

                var httpClientFactoryMock = new Mock<IHttpClientFactory>();
                var httpClientMock = new Mock<HttpClient>();
                var httpClientWrapperMock = new Mock<HttpClientWrapper>(httpClientMock.Object);
                httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClientMock.Object);

                var smsService = new MessageProviderService(httpClientFactoryMock.Object, applicationConfigurationInfo);
                Assert.False(await smsService.SendMessage(messageRequestInfo));
            }
        }

        public class ValidateMessage
        {
          public static IEnumerable<object[]> InvalidSMSContent => new List<object[]>
          {
            new object[]
            {
                new MessageInfo
                {
                    key = "sms",
                    Message = "INVALID_SMS_MESSAGE_LENGTH",
                }
            },
            new object[]
            {
                new MessageInfo
                {
                    key = "sms",
                    Message = "INVALID_SMS",
                }
            }
          };

            [Theory]
            [MemberData(nameof(InvalidSMSContent))]
            public void ReturnsFalse_WhenInvalidSMSContent(MessageInfo messageContent)
            {
                ApplicationConfigurationInfo applicationConfigurationInfo = new ApplicationConfigurationInfo()
                {
                    NotificationValidationConfigurations = new NotificationValidationConfigurations()
                    {
                        EnableValidation = true,
                        SmsMsgLength = 10
                    }
                };

                var smsProviderService = new MessageProviderService(null, applicationConfigurationInfo);
                Assert.False(smsProviderService.ValidateMessage(messageContent));
            }

          public static IEnumerable<object[]> ValidSMSContent => new List<object[]>
          {
            new object[]
            {
                new MessageInfo
                {
                    key = "sms",
                    Message = "VALID_SMS",
                }
            }
          };

            [Theory]
            [MemberData(nameof(ValidSMSContent))]
            public void ReturnsTrue_WhenValidSMSContent(MessageInfo messageContent)
            {
                ApplicationConfigurationInfo applicationConfigurationInfo = new ApplicationConfigurationInfo()
                {
                    NotificationValidationConfigurations = new NotificationValidationConfigurations()
                    {
                        EnableValidation = true,
                        SmsMsgLength = 10
                    }
                };

                var smsProviderService = new MessageProviderService(null, applicationConfigurationInfo);
                Assert.True(smsProviderService.ValidateMessage(messageContent));
            }
        }
    }

}
