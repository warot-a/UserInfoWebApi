# User Info Web API

## Overview
This project is a proof-of-concept (PoC) web API service designed to provide secure, controlled access to AWS data stores (OpenSearch and ElastiCache Redis) while addressing security and observability concerns.

### Problem Statement
Previously, clients accessed AWS OpenSearch and ElastiCache Redis instances directly via network exposure using third-party SDKs. This approach introduced several challenges:

Security risks from exposing data instances directly to external networks
Lack of observability when troubleshooting client issues, as there were no logs of client interactions
Difficulty gathering diagnostic information since clients often didn't provide HTTP request details unless specifically asked

### Solution
This service acts as a secure intermediary API layer by:

1. Centralizing access control - The service runs on AWS ECS Fargate and is the only endpoint allowed to access OpenSearch and Redis instances, eliminating direct network exposure
1. Enabling comprehensive logging - Since the service implementation is fully controlled, it captures detailed information about all client interactions:
    * Client name (via x-clientname header)
    * Timestamp of requests
    * HTTP request paths, query strings, and payloads
    * Trace IDs for request tracking
1. Providing standardized API endpoints - Clients interact with this service instead of directly querying data stores, making debugging and support easier
1. Implementing authentication - The service authenticates client requests by checking the `x-application-id` HTTP header against a DynamoDB table. Only clients with registered application IDs are authorized to access the service. This requires communication with clients about this breaking change.

The PoC version focuses on establishing this architecture with basic logging capabilities, providing a foundation for more advanced features and monitoring in future versions.

## Project structure, framework and technologies

### Architecture Overview

This service follows a layered architecture pattern with clear separation of concerns:

```
HTTP Request
    ↓
Authentication (ApplicationIdAuthenticationHandler)
    ↓
Custom Middleware (Logging, Context Management)
    ↓
Controllers (UserInfoController, HealthCheckController, ErrorController)
    ↓
Service Factories (DynamoDB, Redis, ElasticSearch)
    ↓
AWS Data Stores (DynamoDB, ElastiCache Redis, OpenSearch)
```

### Frameworks and Technologies

- **Framework**: ASP.NET Core (.NET 8.0) - for building the web API
- **Authentication**: Custom authentication handler using HTTP headers
- **Data Access**:
  - **AWS DynamoDB** - for storing application registrations and user metadata
  - **AWS ElastiCache Redis** - for checking if users exist and providing basic user information (email, firstname, lastname)
  - **AWS OpenSearch** - for full-text search on user information and providing richer data than Redis
- **Logging**: Custom logger provider (`UserInfoLoggerProvider`) for structured logging
- **Dependency Injection**: Built-in ASP.NET Core DI container via `ServicesConfiguration`

### Project Structure

- **Controllers/** - API endpoints handling HTTP requests
  - `UserInfoController.cs` - Main endpoint for user information queries
  - `HealthCheckController.cs` - Health check endpoint for monitoring
  - `ErrorController.cs` - Centralized error handling

- **AuthenticationHandlers/** - Custom authentication logic
  - `ApplicationIdAuthenticationHandler.cs` - Validates client requests via `x-application-id` header

- **DynamoDB/** - Database client factory and interface
  - `DynamoDbClientFactory.cs` - Creates and configures DynamoDB client

- **Redis/** - Cache client factory and interface
  - `RedisFactory.cs` - Creates and configures Redis client

- **Search/** - OpenSearch/Elasticsearch client factory
  - `ElasticClientFactory.cs` - Creates and configures Elasticsearch client

- **ServiceFactory/** - Service configuration and dependency registration
  - `ServicesConfiguration.cs` - Configures all services and factories

- **Logger/** - Custom logging implementation
  - `UserInfoLoggerProvider.cs` - Custom logger provider for structured logging
  - `MiddlewareContextAccessor.cs` - Provides context to middleware components

- **Model/** - Request and response data models
  - **Request/** - API request models
  - **Response/** - API response models and DTOs

## How to develop

### Prerequisites

- **.NET 8.0 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Docker** (optional, for containerized development)
- **AWS Account** with configured credentials
- **AWS CLI** (optional, for local AWS service testing)

### Local Development Setup

#### 1. Clone the repository
```bash
git clone <repository-url>
cd UserInfoWebApi
```

#### 2. Configure AWS credentials
Set up your AWS credentials using one of these methods:
- **AWS CLI**: `aws configure`
- **Environment variables**: Set `AWS_ACCESS_KEY_ID` and `AWS_SECRET_ACCESS_KEY`
- **IAM Role**: If running on AWS, use the instance role

#### 3. Configure environment variables
Set below mandatory environment variables:
- `APPLICATION_TABLE_NAME` - DynamoDB table name for applications
- `APPLICATION_INDEX_NAME` - DynamoDB index name for querying applications
- `REDIS_HOST` - ElastiCache Redis endpoint (e.g., localhost:6379 for local development)
- `OPENSEARCH_ENDPOINT` - OpenSearch domain endpoint

#### 4. Restore dependencies and build
```bash
dotnet restore
dotnet build
```

#### 5. Run the application
```bash
dotnet run
```

The API will be available at `https://localhost:5001` or `http://localhost:5000`.

### Docker Development

#### Build the Docker image
```bash
docker build -t userinfo-api:latest .
```

#### Run the container
```bash
docker run -e AWS_ACCESS_KEY_ID=<key> \
           -e AWS_SECRET_ACCESS_KEY=<secret> \
           -e APPLICATION_TABLE_NAME=<table> \
           -e APPLICATION_INDEX_NAME=<index> \
           -e REDIS_HOST=<redis-host> \
           -e OPENSEARCH_ENDPOINT=<opensearch-endpoint> \
           -p 8080:8080 \
           userinfo-api:latest
```

### API Documentation

Once the application is running, access the Swagger UI at:
```
http://localhost:5000/swagger
```

### Testing

#### Health Check Endpoint
```bash
curl http://localhost:5000/health
```

#### User Info Query Example
```bash
curl -X GET "http://localhost:5000/api/userinfo/getUserByUuid/{uuid}" \
     -H "x-application-id: <your-app-id>" \
     -H "Content-Type: application/json"
```

### Build and Deploy

#### Prerequisites for Deployment
- AWS Account with ECR repository created
- AWS CLI configured with appropriate permissions
- GitLab CI/CD pipeline configured with AWS credentials
- Docker installed locally (for manual builds)

#### Local Docker Build

Build the Docker image locally:
```bash
docker build -t userinfo-api:latest .
docker tag userinfo-api:latest <aws-account-id>.dkr.ecr.ap-southeast-1.amazonaws.com/userinfo-api:latest
```

#### CI/CD Pipeline with GitLab and Amazon ECR

The deployment process is automated through GitLab CI/CD and Amazon ECR. The pipeline is defined in `.gitlab-ci.yml` and uses the `build_deploy_image.sh` script.

##### Pipeline Steps

1. **Build Docker Image** - GitLab CI builds the Docker image from the repository
   - Runs on every commit to the configured branches
   - Uses the Dockerfile in the repository root

2. **Authenticate to Amazon ECR** - The CI runner authenticates Docker to your ECR registry
   - Uses AWS credentials (AWS_ACCESS_KEY_ID and AWS_SECRET_ACCESS_KEY)
   - Validates the ECR repository exists, creates it if necessary

3. **Tag and Push to ECR** - The image is tagged with multiple identifiers and pushed
   - Tags: commit SHA, branch name, and "latest"
   - Full URI: `<aws-account-id>.dkr.ecr.<region>.amazonaws.com/userinfo-api:<tag>`
   - All tags point to the same image for easy tracking and rollback

4. **Update ECS Service** - After successful push, the ECS service is updated
   - Forces a new task definition deployment
   - Rolling update ensures zero-downtime deployment
   - New container instances pull the latest image from ECR

##### Required Environment Variables in GitLab CI

Configure these in your GitLab project's CI/CD variables:
- `AWS_REGION` - AWS region (e.g., us-east-1)
- `AWS_ACCOUNT_ID` - Your AWS account ID
- `AWS_ACCESS_KEY_ID` - AWS credentials for ECR access
- `AWS_SECRET_ACCESS_KEY` - AWS credentials for ECR access
- `ECR_REPOSITORY` - Repository name (e.g., userinfo-api)
- `ECS_CLUSTER` - ECS cluster name
- `ECS_SERVICE` - ECS service name
- `IMAGE_TAG` - Image tag (usually the commit SHA or branch name)

##### Manual Deployment

If you need to push an image manually:

```bash
# Authenticate to ECR
aws ecr get-login-password --region <region> | docker login --username AWS --password-stdin <aws-account-id>.dkr.ecr.<region>.amazonaws.com

# Build and push
docker build -t userinfo-api:latest .
docker tag userinfo-api:latest <aws-account-id>.dkr.ecr.<region>.amazonaws.com/userinfo-api:latest
docker push <aws-account-id>.dkr.ecr.<region>.amazonaws.com/userinfo-api:latest

# Update ECS service
aws ecs update-service --cluster <cluster-name> --service <service-name> --force-new-deployment --region <region>
```
