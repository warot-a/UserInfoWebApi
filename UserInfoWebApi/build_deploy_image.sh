#!/bin/bash

IMAGE_NAME="userinfo-webapi-$ENVIRONMENT"

docker build --no-cache -t $IMAGE_NAME -f ./Dockerfile .
ECR_REGISTRY="$AWS_ACCOUNT.dkr.ecr.ap-southeast-1.amazonaws.com"
aws ecr get-login-password --region ap-southeast-1 | docker login --username AWS --password-stdin $ECR_REGISTRY
docker tag $IMAGE_NAME:latest $AWS_ACCOUNT.dkr.ecr.ap-southeast-1.amazonaws.com/$IMAGE_NAME:$IMAGE_TAG
docker push $ECR_REGISTRY/$IMAGE_NAME:$IMAGE_TAG

aws ecs update-service --region ap-southeast-1 --cluster UserInfoWebApiCluster-$ENVIRONMENT-ap-southeast-1 --service UserInfoWebApiService-$ENVIRONMENT-ap-southeast-1 --force-new-deployment
