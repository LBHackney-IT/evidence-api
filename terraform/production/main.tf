provider "aws" {
    region  = "eu-west-2"
    version = "~> 3.0"
}
data "aws_caller_identity" "current" {}

data "aws_region" "current" {}

data "aws_vpc" "dr_vpc" {
    tags = {
        Name = "disaster-recovery-prod"
    }
}

terraform {
    backend "s3" {
        bucket  = "terraform-state-disaster-recovery"
        encrypt = true
        region  = "eu-west-2"
        key     = "services/des/state"
    }
}

data "aws_ssm_parameter" "evidence_postgres_port_security_group" {
    name = "/evidence-api/production/postgres-port"
}

resource "aws_security_group" "evidence_api_db_traffic" {
    vpc_id      = data.aws_vpc.dr_vpc.id
    name_prefix = "allow_evidence_api_db_traffic"

    ingress {
        description = "evidence_api_db_dr"
        from_port   = data.aws_ssm_parameter.evidence_postgres_port_security_group.value
        to_port     = data.aws_ssm_parameter.evidence_postgres_port_security_group.value
        protocol    = "tcp"

        cidr_blocks = [data.aws_vpc.dr_vpc.cidr_block]
    }

    egress {
        description = "allow outbound traffic"
        from_port   = 0
        to_port     = 0
        protocol    = "-1"
        cidr_blocks = ["0.0.0.0/0"]
    }

    tags = {
        "Name" = "evidence_api_db_traffic-dr"
    }
}

resource "aws_db_subnet_group" "evidence_api_subnets" {
    name       = "evidence-api-subnet-group-dr"
    subnet_ids = ["subnet-0e6bc9b4ac24493cc","subnet-05e595c59b7d6c8df"]
    lifecycle {
        create_before_destroy = true
    }
}
