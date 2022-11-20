variable "aws_region" {
  type        = string
  default     = "us-west-2"
}

variable "ecs_log_group" {
  type        = string
  default     = "gra-log-group"
}

variable "ecr_repository" {
  type        = string
  default     = "gra-repo"
}