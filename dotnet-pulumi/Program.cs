using Pulumi;
using Pulumi.Aws.S3;
using Pulumi.Aws.DynamoDB;
using System.Collections.Generic;
using Pulumi.Aws.DynamoDB.Inputs;
using Pulumi.Aws.Lambda;
using Pulumi.Aws.Iam;
using ApiGateway = Pulumi.Aws.ApiGateway;

return await Deployment.RunAsync(() =>
{
    // Create an AWS resource (S3 Bucket)
    var bucket = new BucketV2("dotnetgt-bucket", new BucketV2Args
    {
        BucketPrefix = "dotnetgt-bucket"
    });


    // Create a DynamoDB table
    var table = new Table("dotnet-gt-dynamo-table", new TableArgs
    {
        Attributes = 
        {
            new TableAttributeArgs
            {
                Name = "Id",
                Type = "S"
            }
        },
        HashKey = "Id",
        BillingMode = "PAY_PER_REQUEST"
    });



        // 3. Crear el rol IAM para la Lambda Function
        var lambdaRole = new Role("lambdaRole", new RoleArgs
        {
            AssumeRolePolicy = @"{
                ""Version"": ""2012-10-17"",
                ""Statement"": [
                    {
                        ""Effect"": ""Allow"",
                        ""Principal"": { ""Service"": ""lambda.amazonaws.com"" },
                        ""Action"": ""sts:AssumeRole""
                    }
                ]
            }"
        });

        // 4. Adjuntar políticas de permisos para S3, DynamoDB y ejecución básica de Lambda
        new RolePolicyAttachment("lambdaS3Policy", new RolePolicyAttachmentArgs
        {
            Role = lambdaRole.Name,
            PolicyArn = "arn:aws:iam::aws:policy/AmazonS3FullAccess"
        });

        new RolePolicyAttachment("lambdaDynamoPolicy", new RolePolicyAttachmentArgs
        {
            Role = lambdaRole.Name,
            PolicyArn = "arn:aws:iam::aws:policy/AmazonDynamoDBFullAccess"
        });

        new RolePolicyAttachment("lambdaBasicExecutionPolicy", new RolePolicyAttachmentArgs
        {
            Role = lambdaRole.Name,
            PolicyArn = "arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole"
        });
    

        //5. Especificar el código de la Lambda Function
        var lambda = new Function("imageUploaderLambda", new FunctionArgs
        {
            Runtime = "dotnet8",
            Role = lambdaRole.Arn,
            Handler = "ImageUploaderLambda::ImageUploaderLambda.Function::FunctionHandler",
            Code = new FileArchive("/Users/marloncoti/dotneconf2024/Lambda/ImageUploaderLambda/bin/Release/net9.0/publish/") // Ruta al código Lambda
            
        });


    // 6. Crear API Gateway y vincularlo a la Lambda Function
        var api = new ApiGateway.RestApi("uploadApi", new ApiGateway.RestApiArgs
        {
            Description = "API para subir imágenes a S3 y registrar rutas en DynamoDB"
        });

        // 7. Crear un recurso en el API Gateway para la ruta /upload
        var resource = new ApiGateway.Resource("uploadResource", new ApiGateway.ResourceArgs
        {
            ParentId = api.RootResourceId,
            PathPart = "upload",
            RestApi = api.Id
        });

    // 8. Crear el método POST para /upload
        var method = new ApiGateway.Method("uploadMethod", new ApiGateway.MethodArgs
        {
            RestApi = api.Id,
            ResourceId = resource.Id,
            HttpMethod = "POST",
            Authorization = "NONE"
        });

       // 9. Crear la integración de Lambda con el recurso /upload
    var integration = new ApiGateway.Integration("lambdaIntegration", new ApiGateway.IntegrationArgs
    {
        RestApi = api.Id,
        ResourceId = resource.Id,
        HttpMethod = "POST",
        IntegrationHttpMethod = "POST",
        Type = "AWS_PROXY",
        Uri = lambda.InvokeArn
    });


    // 10. Configurar la etapa del API Gateway
    var stage = new ApiGateway.Stage("apiStage", new ApiGateway.StageArgs
    {
        RestApi = api.Id,
        Deployment = new ApiGateway.Deployment("apiDeployment", new ApiGateway.DeploymentArgs
        {
            RestApi = api.Id
        }).Id,
        StageName = "prod"
    });

        // 6. Output de los recursos
     return new Dictionary<string, object?>
    {
        ["bucketName"] = bucket.BucketDomainName,
        ["tableName"] = table.Name,
        ["lambdaName"] = lambda.Name,
        ["apiEndpoint"] = stage.InvokeUrl
    };
});



