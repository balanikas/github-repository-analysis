
resource "aws_ecs_cluster" "gra-ecs-cluster" {
  name = "ecs-cluster-for-gra"
}

resource "aws_ecs_task_definition" "gra-ecs-task-definition" {
  family                   = "ecs-task-definition-gra"
  network_mode             = "awsvpc"
  requires_compatibilities = ["FARGATE"]
  memory                   = "1024"
  cpu                      = "512"
  task_role_arn            = "arn:aws:iam::686788842590:role/blazorToSqsRole"
  execution_role_arn       = "arn:aws:iam::686788842590:role/ecsTaskExecutionRole"
  container_definitions    = <<EOF
[
  {
    "name": "gra-container",
    "image": "686788842590.dkr.ecr.${var.aws_region}.amazonaws.com/${var.ecr_repository}:latest",
    "memory": 1024,
    "cpu": 512,
    "essential": true,
    "entryPoint": ["dotnet", "BlazorApp.dll"],
    "portMappings": [
      {
        "containerPort": 80,
        "hostPort": 80
      }
    ],
    "logConfiguration": {
      "logDriver": "awslogs",
      "options": {
          "awslogs-region" : "${var.aws_region}",
          "awslogs-group" : "${var.ecs_log_group}",
          "awslogs-stream-prefix" : "project"
      }
      }
  }
]
EOF
}

resource "aws_ecs_service" "gra-ecs-service" {
  name            = "gra-app"
  cluster         = aws_ecs_cluster.gra-ecs-cluster.id
  task_definition = aws_ecs_task_definition.gra-ecs-task-definition.arn
  launch_type     = "FARGATE"
  network_configuration {
    subnets          = ["subnet-09b5d96bad3291cec"]
    assign_public_ip = true
  }
  desired_count = 1
}


resource "aws_cloudwatch_log_group" "gra-log-group" {
  name = var.ecs_log_group
}
