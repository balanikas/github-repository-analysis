docker build src/ -t 686788842590.dkr.ecr.us-west-2.amazonaws.com/repo
aws ecr get-login-password --region us-west-2 | docker login --username AWS --password-stdin 686788842590.dkr.ecr.us-west-2.amazonaws.com
docker push 686788842590.dkr.ecr.us-west-2.amazonaws.com/repo