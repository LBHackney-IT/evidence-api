provider "aws" {
    region  = "eu-west-2"
    version = "~> 3.0"
}

provider "aws" {
    alias  = "certificate_manager"
    region = "us-east-1"
}

provider "aws" {
    alias  = "route_53"
    region = "eu-west-2"

    assume_role {
        role_arn = "arn:aws:iam::846441805239:role/ce_oidc_acm_verification"
    }
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

# Evidence API
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

# Documents API
data "aws_ssm_parameter" "documents_postgres_port_security_group" {
    name = "/documents-api/production/postgres-port"
}

resource "aws_security_group" "documents_api_db_traffic" {
    vpc_id      = data.aws_vpc.dr_vpc.id
    name_prefix = "allow_documents_api_db_traffic"

    ingress {
        description = "documents_api_db_dr"
        from_port   = data.aws_ssm_parameter.documents_postgres_port_security_group.value
        to_port     = data.aws_ssm_parameter.documents_postgres_port_security_group.value
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
        "Name" = "documents_api_db_traffic-dr"
    }
}

resource "aws_db_subnet_group" "documents_api_subnets" {
    name       = "documents-api-subnet-group-dr"
    subnet_ids = ["subnet-0e6bc9b4ac24493cc","subnet-05e595c59b7d6c8df"]
    lifecycle {
        create_before_destroy = true
    }
}

# Front end
resource "aws_security_group" "frontend_traffic" {
    vpc_id      = data.aws_vpc.dr_vpc.id
    name_prefix = "allow_frontend_traffic"

    egress {
        description = "allow outbound traffic"
        from_port   = 0
        to_port     = 0
        protocol    = "-1"
        cidr_blocks = ["0.0.0.0/0"]
    }
}

# Certificate
module "acm_certificate" {
    source = "github.com/LBHackney-IT/terraform-aws-acm"

    providers = {
        aws.acm = aws.certificate_manager
        aws.r53 = aws.route_53
    }
    certificate_transparency_logging_preference = true
    create_certificate                          = true
    dns_ttl                                     = 60
    domain_name                                 = "evidence-dr.hackney.gov.uk"
    subject_alternative_names                   = []
    validate_certificate                        = true
    validation_allow_overwrite_records          = true
    validation_method                           = "DNS"
    wait_for_validation                         = true
    zone_id                                     = "Z05689131LRP536POAGQN"


    tags = {
        Name        = "DES Frontend DR Certificate"
        Environment = "DR"
    }
}

# Output the certificate ARN
output "acm_certificate_arn" {
    description = "ACM Certificate ARN"
    value       = module.acm_certificate.acm_certificate_arn
}
