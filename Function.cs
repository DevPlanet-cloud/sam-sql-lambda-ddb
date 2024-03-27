using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Microsoft.VisualBasic;
using System.Diagnostics.Metrics;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace sam_sqs_lambda_ddb;

public class Function
{
    private string _ddbTableName = Environment.GetEnvironmentVariable("TableName");

    /// <summary>
    /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
    /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
    /// region the Lambda function is executed in.
    /// </summary>
    public Function()
    {

    }


    /// <summary>
    /// This method is called for every Lambda invocation. This method takes in an SQS event object and can be used 
    /// to respond to SQS messages.
    /// </summary>
    /// <param name="evnt">The event for the Lambda function handler to process.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
    {
        foreach(var message in evnt.Records)
        {
            await ProcessMessageAsync(message, context);
        }
    }

    private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
    {
        context.Logger.LogInformation($"Processing message {message.Body}");
        Console.WriteLine($"DDB Table: {_ddbTableName}");

        // TODO: Do interesting work based on the new message
        await SaveItemToDDB(message.Body);
    }

    private async Task SaveItemToDDB(string message)
    {
        AmazonDynamoDBClient ddbClient = new AmazonDynamoDBClient();

        var item = new Dictionary<string, AttributeValue>(2)
        {
            { "MessageID", new AttributeValue(Guid.NewGuid().ToString()) },
            { "Message", new AttributeValue(message) }
        };

        var request = new PutItemRequest
        {
            TableName = _ddbTableName,
            Item = item
        };

        await ddbClient.PutItemAsync(request);

        Console.Write($"Message {message} has been written to DynamoDB table successfully!");
    }
}