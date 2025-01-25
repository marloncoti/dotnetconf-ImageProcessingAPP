
# **Guía: Creación de un Proyecto con Pulumi, C# y AWS**

Este proyecto crea una Lambda Function que permite subir imágenes a S3 y guardar referencias en DynamoDB. 
La infraestructura será gestionada con Pulumi y el código se implementará en C#.

---

## **Requisitos previos**
1. **Pulumi** instalado ([Instrucciones](https://www.pulumi.com/docs/get-started/install/)).
2. **AWS CLI** configurado ([Instrucciones](https://docs.aws.amazon.com/cli/latest/userguide/install-cliv2.html)).
3. **.NET SDK** instalado ([Descargar](https://dotnet.microsoft.com/)).
4. Una cuenta de AWS con credenciales configuradas.

---

## **Pasos para crear el proyecto**

### **1. Inicializar el proyecto**
1. Crea un nuevo directorio:
   ```bash
   mkdir pulumi-csharp-lambda-s3-dynamo
   cd pulumi-csharp-lambda-s3-dynamo
   ```

2. Inicializa un proyecto Pulumi con C#:
   ```bash
   pulumi new aws-csharp
   ```

3. Instala los paquetes NuGet necesarios:
   ```bash
   dotnet add package Pulumi.Aws
   dotnet add package Amazon.Lambda.Core
   dotnet add package Amazon.Lambda.Serialization.SystemTextJson
   dotnet add package Amazon.DynamoDBv2
   dotnet add package Amazon.S3
   dotnet add package Amazon.Lambda.APIGatewayEvents
   ```

---

### **2. Configurar la Lambda Function**
1. Crea un directorio `LambdaHandler` para la función:
   ```bash
   mkdir LambdaHandler
   touch LambdaHandler/Function.cs
   ```

2. Añade el código necesario en `LambdaHandler/Function.cs` (proporcionado en la descripción previa).

---

### **3. Configurar Pulumi**
1. Configura `Program.cs` para definir la infraestructura, incluyendo S3, DynamoDB, la función Lambda y el API Gateway.

---

### **4. Desplegar la infraestructura**
1. Compila el proyecto:
   ```bash
   dotnet build
   ```

2. Despliega los recursos con Pulumi:
   ```bash
   pulumi up
   ```

---

### **5. Prueba la API**
1. Realiza un `POST` a la URL del API con una imagen codificada en Base64.
