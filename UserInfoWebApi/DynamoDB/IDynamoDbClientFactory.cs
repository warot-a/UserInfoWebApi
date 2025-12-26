using Amazon.DynamoDBv2;

namespace UserInfoWebApi.DynamoDB
{
    public interface IDynamoDbClientFactory
	{
        AmazonDynamoDBClient CreateDynamoDbClient(string? region = null);
    }
}

