using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ImageUploaderLambda
{
    public class Function
    {
        private readonly IAmazonS3 _s3Client;
        private readonly IAmazonDynamoDB _dynamoDbClient;

        private const string BucketName = "dotnetgt-bucket20250125170228357800000001"; // 
        private const string TableName = "dotnet-gt-dynamo-table-27c5da2"; // Cambia este nombre según tu tabla

        public Function()
        {
            _s3Client = new AmazonS3Client();
            _dynamoDbClient = new AmazonDynamoDBClient();
        }

        public async Task<string> FunctionHandler(Stream input, ILambdaContext context)
        {
            try
            {
                // Generar un nombre único para el archivo
                var fileName = $"images/{Guid.NewGuid()}.jpg";

                // Subir el archivo a S3
                var putRequest = new Amazon.S3.Model.PutObjectRequest
                {
                    BucketName = BucketName,
                    Key = fileName,
                    InputStream = input,
                    ContentType = "image/jpeg"
                };

                await _s3Client.PutObjectAsync(putRequest);

                // Guardar la ruta en DynamoDB
                var s3Path = $"s3://{BucketName}/{fileName}";
                var putItemRequest = new PutItemRequest
                {
                    TableName = TableName,
                    Item = new System.Collections.Generic.Dictionary<string, AttributeValue>
                    {
                        { "FileId", new AttributeValue { S = Guid.NewGuid().ToString() } },
                        { "S3Path", new AttributeValue { S = s3Path } },
                        { "Timestamp", new AttributeValue { S = DateTime.UtcNow.ToString("o") } }
                    }
                };

                await _dynamoDbClient.PutItemAsync(putItemRequest);

                // Retornar éxito
                return $"File uploaded successfully to {s3Path}";
            }
            catch (Exception ex)
            {
                context.Logger.LogLine($"Error: {ex.Message}");
                throw;
            }
        }
    }
}
