﻿// <auto-generated />
using System;
using System.Collections.Generic;
using EvidenceApi.V1.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EvidenceApi.Migrations
{
    [DbContext(typeof(EvidenceContext))]
    [Migration("20230206153010_BackfillResidentsGroupIdWithExistingResidents")]
    partial class BackfillResidentsGroupIdWithExistingResidents
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("EvidenceApi.V1.Domain.AuditEvent", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("created_at");

                    b.Property<string>("HttpMethod")
                        .HasColumnType("text")
                        .HasColumnName("http_method");

                    b.Property<string>("RequestBody")
                        .HasColumnType("text")
                        .HasColumnName("request_body");

                    b.Property<string>("UrlVisited")
                        .HasColumnType("text")
                        .HasColumnName("url_visited");

                    b.Property<string>("UserEmail")
                        .HasColumnType("text")
                        .HasColumnName("user_email");

                    b.HasKey("Id");

                    b.ToTable("audit_events");
                });

            modelBuilder.Entity("EvidenceApi.V1.Domain.Communication", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("created_at");

                    b.Property<int>("DeliveryMethod")
                        .HasColumnType("integer")
                        .HasColumnName("delivery_method");

                    b.Property<Guid>("EvidenceRequestId")
                        .HasColumnType("uuid")
                        .HasColumnName("evidence_request_id");

                    b.Property<string>("NotifyId")
                        .HasColumnType("text")
                        .HasColumnName("notify_id");

                    b.Property<int>("Reason")
                        .HasColumnType("integer")
                        .HasColumnName("reason");

                    b.Property<string>("TemplateId")
                        .HasColumnType("text")
                        .HasColumnName("template_id");

                    b.HasKey("Id");

                    b.HasIndex("EvidenceRequestId");

                    b.ToTable("communications");
                });

            modelBuilder.Entity("EvidenceApi.V1.Domain.DocumentSubmission", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<DateTime?>("AcceptedAt")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("accepted_at");

                    b.Property<string>("ClaimId")
                        .HasColumnType("text")
                        .HasColumnName("claim_id");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("created_at");

                    b.Property<string>("DocumentTypeId")
                        .HasColumnType("text")
                        .HasColumnName("document_type_id");

                    b.Property<Guid?>("EvidenceRequestId")
                        .HasColumnType("uuid")
                        .HasColumnName("evidence_request_id");

                    b.Property<DateTime?>("RejectedAt")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("rejected_at");

                    b.Property<string>("RejectionReason")
                        .HasColumnType("text")
                        .HasColumnName("rejection_reason");

                    b.Property<Guid>("ResidentId")
                        .HasColumnType("uuid")
                        .HasColumnName("resident_id");

                    b.Property<string>("StaffSelectedDocumentTypeId")
                        .HasColumnType("text")
                        .HasColumnName("staff_selected_document_type_id");

                    b.Property<int>("State")
                        .HasColumnType("integer")
                        .HasColumnName("state");

                    b.Property<string>("Team")
                        .HasColumnType("text")
                        .HasColumnName("team");

                    b.Property<string>("UserUpdatedBy")
                        .HasColumnType("text")
                        .HasColumnName("user_updated_by");

                    b.Property<bool>("isHidden")
                        .HasColumnType("boolean")
                        .HasColumnName("is_hidden");

                    b.HasKey("Id");

                    b.HasIndex("EvidenceRequestId");

                    b.HasIndex("ResidentId");

                    b.ToTable("document_submissions");
                });

            modelBuilder.Entity("EvidenceApi.V1.Domain.EvidenceRequest", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("created_at");

                    b.Property<List<string>>("DocumentTypes")
                        .HasColumnType("text[]")
                        .HasColumnName("document_types");

                    b.Property<string>("NoteToResident")
                        .HasColumnType("text")
                        .HasColumnName("note_to_resident");

                    b.Property<string>("NotificationEmail")
                        .HasColumnType("text")
                        .HasColumnName("notification_email");

                    b.Property<List<string>>("RawDeliveryMethods")
                        .HasColumnType("text[]")
                        .HasColumnName("delivery_methods");

                    b.Property<string>("Reason")
                        .HasColumnType("text")
                        .HasColumnName("reason");

                    b.Property<Guid>("ResidentId")
                        .HasColumnType("uuid")
                        .HasColumnName("resident_id");

                    b.Property<string>("ResidentReferenceId")
                        .HasColumnType("text")
                        .HasColumnName("resident_reference_id");

                    b.Property<int>("State")
                        .HasColumnType("integer")
                        .HasColumnName("state");

                    b.Property<string>("Team")
                        .HasColumnType("text")
                        .HasColumnName("team");

                    b.Property<string>("UserRequestedBy")
                        .HasColumnType("text")
                        .HasColumnName("user_requested_by");

                    b.HasKey("Id");

                    b.ToTable("evidence_requests");
                });

            modelBuilder.Entity("EvidenceApi.V1.Domain.Resident", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("created_at");

                    b.Property<string>("Email")
                        .HasColumnType("text")
                        .HasColumnName("email");

                    b.Property<string>("Name")
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("text")
                        .HasColumnName("phone_number");

                    b.HasKey("Id");

                    b.ToTable("residents");
                });

            modelBuilder.Entity("EvidenceApi.V1.Domain.ResidentsTeamGroupId", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("created_at");

                    b.Property<Guid>("GroupId")
                        .HasColumnType("uuid")
                        .HasColumnName("group_id");

                    b.Property<Guid>("ResidentId")
                        .HasColumnType("uuid")
                        .HasColumnName("resident_id");

                    b.Property<string>("Team")
                        .HasColumnType("text")
                        .HasColumnName("team");

                    b.HasKey("Id");

                    b.HasIndex("ResidentId");

                    b.ToTable("residents_team_group_id");
                });

            modelBuilder.Entity("EvidenceApi.V1.Domain.Communication", b =>
                {
                    b.HasOne("EvidenceApi.V1.Domain.EvidenceRequest", "EvidenceRequest")
                        .WithMany("Communications")
                        .HasForeignKey("EvidenceRequestId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("EvidenceRequest");
                });

            modelBuilder.Entity("EvidenceApi.V1.Domain.DocumentSubmission", b =>
                {
                    b.HasOne("EvidenceApi.V1.Domain.EvidenceRequest", "EvidenceRequest")
                        .WithMany("DocumentSubmissions")
                        .HasForeignKey("EvidenceRequestId");

                    b.HasOne("EvidenceApi.V1.Domain.Resident", "Resident")
                        .WithMany()
                        .HasForeignKey("ResidentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("EvidenceRequest");

                    b.Navigation("Resident");
                });

            modelBuilder.Entity("EvidenceApi.V1.Domain.ResidentsTeamGroupId", b =>
                {
                    b.HasOne("EvidenceApi.V1.Domain.Resident", "Resident")
                        .WithMany()
                        .HasForeignKey("ResidentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Resident");
                });

            modelBuilder.Entity("EvidenceApi.V1.Domain.EvidenceRequest", b =>
                {
                    b.Navigation("Communications");

                    b.Navigation("DocumentSubmissions");
                });
#pragma warning restore 612, 618
        }
    }
}
