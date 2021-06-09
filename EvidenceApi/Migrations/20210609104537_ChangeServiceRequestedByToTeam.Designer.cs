﻿// <auto-generated />
using System;
using System.Collections.Generic;
using EvidenceApi.V1.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace EvidenceApi.Migrations
{
    [DbContext(typeof(EvidenceContext))]
    [Migration("20210609104537_ChangeServiceRequestedByToTeam")]
    partial class ChangeServiceRequestedByToTeam
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "3.1.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("EvidenceApi.V1.Domain.AuditEvent", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnName("created_at")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("HttpMethod")
                        .HasColumnName("http_method")
                        .HasColumnType("text");

                    b.Property<string>("RequestBody")
                        .HasColumnName("request_body")
                        .HasColumnType("text");

                    b.Property<string>("UrlVisited")
                        .HasColumnName("url_visited")
                        .HasColumnType("text");

                    b.Property<string>("UserEmail")
                        .HasColumnName("user_email")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("audit_events");
                });

            modelBuilder.Entity("EvidenceApi.V1.Domain.Communication", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnName("created_at")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int>("DeliveryMethod")
                        .HasColumnName("delivery_method")
                        .HasColumnType("integer");

                    b.Property<Guid>("EvidenceRequestId")
                        .HasColumnName("evidence_request_id")
                        .HasColumnType("uuid");

                    b.Property<string>("NotifyId")
                        .HasColumnName("notify_id")
                        .HasColumnType("text");

                    b.Property<int>("Reason")
                        .HasColumnName("reason")
                        .HasColumnType("integer");

                    b.Property<string>("TemplateId")
                        .HasColumnName("template_id")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("EvidenceRequestId");

                    b.ToTable("communications");
                });

            modelBuilder.Entity("EvidenceApi.V1.Domain.DocumentSubmission", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("uuid");

                    b.Property<string>("ClaimId")
                        .HasColumnName("claim_id")
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnName("created_at")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("DocumentTypeId")
                        .HasColumnName("document_type_id")
                        .HasColumnType("text");

                    b.Property<Guid>("EvidenceRequestId")
                        .HasColumnName("evidence_request_id")
                        .HasColumnType("uuid");

                    b.Property<string>("RejectionReason")
                        .HasColumnName("rejection_reason")
                        .HasColumnType("text");

                    b.Property<string>("StaffSelectedDocumentTypeId")
                        .HasColumnName("staff_selected_document_type_id")
                        .HasColumnType("text");

                    b.Property<int>("State")
                        .HasColumnName("state")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("EvidenceRequestId");

                    b.ToTable("document_submissions");
                });

            modelBuilder.Entity("EvidenceApi.V1.Domain.EvidenceRequest", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnName("created_at")
                        .HasColumnType("timestamp without time zone");

                    b.Property<List<string>>("DocumentTypes")
                        .HasColumnName("document_types")
                        .HasColumnType("text[]");

                    b.Property<List<string>>("RawDeliveryMethods")
                        .HasColumnName("delivery_methods")
                        .HasColumnType("text[]");

                    b.Property<string>("Reason")
                        .HasColumnName("reason")
                        .HasColumnType("text");

                    b.Property<Guid>("ResidentId")
                        .HasColumnName("resident_id")
                        .HasColumnType("uuid");

                    b.Property<string>("ResidentReferenceId")
                        .HasColumnName("resident_reference_id")
                        .HasColumnType("text");

                    b.Property<int>("State")
                        .HasColumnName("state")
                        .HasColumnType("integer");

                    b.Property<string>("Team")
                        .HasColumnName("team")
                        .HasColumnType("text");

                    b.Property<string>("UserRequestedBy")
                        .HasColumnName("user_requested_by")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("evidence_requests");
                });

            modelBuilder.Entity("EvidenceApi.V1.Domain.Resident", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnName("created_at")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Email")
                        .HasColumnName("email")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasColumnName("name")
                        .HasColumnType("text");

                    b.Property<string>("PhoneNumber")
                        .HasColumnName("phone_number")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("residents");
                });

            modelBuilder.Entity("EvidenceApi.V1.Domain.Communication", b =>
                {
                    b.HasOne("EvidenceApi.V1.Domain.EvidenceRequest", "EvidenceRequest")
                        .WithMany("Communications")
                        .HasForeignKey("EvidenceRequestId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("EvidenceApi.V1.Domain.DocumentSubmission", b =>
                {
                    b.HasOne("EvidenceApi.V1.Domain.EvidenceRequest", "EvidenceRequest")
                        .WithMany("DocumentSubmissions")
                        .HasForeignKey("EvidenceRequestId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
