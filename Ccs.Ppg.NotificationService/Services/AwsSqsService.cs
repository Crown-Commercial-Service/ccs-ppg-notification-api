using Amazon.SQS;
using Amazon.SQS.Model;
using Ccs.Ppg.NotificationService.Model;
using Ccs.Ppg.NotificationService.Services.IServices;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Ccs.Ppg.NotificationService.Services
{
  public class AwsSqsService : IAwsSqsService
  {
    private const string StringValueType = "String";
    private const string NumberValueType = "Number";

    private readonly AmazonSQSClient _sqsClient;
    private readonly IConfiguration _configuration;
    public AwsSqsService(SqsConfiguration sqsConfiguration, IConfiguration configuration)
    {
      var sqsConfig = new AmazonSQSConfig
      {
        ServiceURL = sqsConfiguration.ServiceUrl
      };
      _sqsClient = new AmazonSQSClient(sqsConfiguration.AccessKeyId, sqsConfiguration.AccessSecretKey, sqsConfig);
      _configuration = configuration;
    }

    /// <summary>
    /// Send message with additional attributes
    /// </summary>
    /// <param name="queueUrl"></param>
    /// <param name="sqsMessageDto"></param>
    /// <returns></returns>
    public async Task SendMessageAsync(string queueUrl, string messageGroupId, SqsMessageDto sqsMessageDto)
    {
      SendMessageRequest messageRequest = CreateMessage(queueUrl, sqsMessageDto, true, messageGroupId);

      // TODO check how the socket connection managed (whether its managed with AmazonSQSClient or SendMessageRequest)
      // AmazonSQSClient has the service url
      // SendMessageRequest has the queue url
      var result = await _sqsClient.SendMessageAsync(messageRequest);
    }

    /// <summary>
    /// Create message send request object
    /// </summary>
    /// <param name="queueUrl"></param>
    /// <param name="sqsMessageDto"></param>
    /// <returns></returns>
    private SendMessageRequest CreateMessage(string queueUrl, SqsMessageDto sqsMessageDto, bool isFifoQueue = true, string messageGroupId = "")
    {
      if (string.IsNullOrWhiteSpace(sqsMessageDto.MessageBody))
      {
        throw new Exception("ERROR_NULL_OR_EMPTY_SQS_MESSAGE_BODY");
      }

      SendMessageRequest messageRequest = new SendMessageRequest
      {
        QueueUrl = queueUrl,
        MessageBody = sqsMessageDto.MessageBody
      };

      if (isFifoQueue)
      {
        messageRequest.MessageGroupId = messageGroupId;
        messageRequest.MessageDeduplicationId = Guid.NewGuid().ToString();
      }

      // Handle string attributes
      if (sqsMessageDto.StringCustomAttributes != null)
      {
        GetStringMessageAttributes(sqsMessageDto.StringCustomAttributes).ForEach((atttributeKeyValue) =>
        {
          messageRequest.MessageAttributes.Add(atttributeKeyValue.Key, atttributeKeyValue.Value);
        });
      }

      // Handle number attributes
      if (sqsMessageDto.NumberCustomAttributes != null)
      {
        GetNumberMessageAttributes(sqsMessageDto.NumberCustomAttributes).ForEach((atttributeKeyValue) =>
        {
          messageRequest.MessageAttributes.Add(atttributeKeyValue.Key, atttributeKeyValue.Value);
        });
      }

      // TODO Handle other data type attributes

      return messageRequest;
    }

    /// <summary>
    /// Get message attributes with string values (DataType = String)
    /// </summary>
    /// <param name="stringCustomAttributes"></param>
    /// <returns></returns>
    private List<KeyValuePair<string, MessageAttributeValue>> GetStringMessageAttributes(Dictionary<string, string> stringCustomAttributes)
    {
      List<KeyValuePair<string, MessageAttributeValue>> messageAttributeValues = new();

      foreach (KeyValuePair<string, string> property in stringCustomAttributes)
      {
        messageAttributeValues.Add(
          new KeyValuePair<string, MessageAttributeValue>(property.Key, new MessageAttributeValue
          {
            DataType = StringValueType,
            StringValue = property.Value
          }));
      }

      return messageAttributeValues;
    }

    /// <summary>
    /// Get message attributes with numeric values (DataType = Number)
    /// </summary>
    /// <param name="numberCustomAttributes"></param>
    /// <returns></returns>
    private List<KeyValuePair<string, MessageAttributeValue>> GetNumberMessageAttributes(Dictionary<string, int> numberCustomAttributes)
    {
      List<KeyValuePair<string, MessageAttributeValue>> messageAttributeValues = new();

      foreach (KeyValuePair<string, int> property in numberCustomAttributes)
      {
        messageAttributeValues.Add(
          new KeyValuePair<string, MessageAttributeValue>(property.Key, new MessageAttributeValue
          {
            DataType = NumberValueType,
            StringValue = property.Value.ToString()
          }));
      }

      return messageAttributeValues;
    }

    public async Task PushUserConfirmFailedEmailToDataQueueAsync(object emailInfoRequest)
    {
      if (Convert.ToBoolean(_configuration["QueueInfo:EnableDataQueue"]))
      {
        var emailInfo = JsonConvert.DeserializeObject<EmailInfo>(emailInfoRequest.ToString());
        try
        {
          SqsMessageDto sqsMessageDto = new()
          {
            MessageBody = JsonConvert.SerializeObject(emailInfo.BodyContent),
            StringCustomAttributes = new Dictionary<string, string>
              {
                { "Destination", "Notification" },
                { "Action", "POST" },
              }
          };

          await SendMessageAsync(_configuration["QueueInfo:DataQueueUrl"], $"EmailId-{emailInfo.To}", sqsMessageDto);
        }
        catch (Exception ex)
        {
          Console.WriteLine($"Error sending message to queue. EmailId: {emailInfo?.To}, Error: {ex.Message}");
        }
      }
    }

  }
}
