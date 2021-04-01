CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

CREATE TABLE evidence_requests (
    id uuid NOT NULL,
    created_at timestamp without time zone NOT NULL,
    resident_id uuid NOT NULL,
    delivery_methods text[] NULL,
    document_types text[] NULL,
    service_requested_by text NULL,
    CONSTRAINT "PK_evidence_requests" PRIMARY KEY (id)
);

CREATE TABLE residents (
    id uuid NOT NULL,
    created_at timestamp without time zone NOT NULL,
    name text NULL,
    email text NULL,
    phone_number text NULL,
    CONSTRAINT "PK_residents" PRIMARY KEY (id)
);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20201209135358_InitialCreate', '3.1.7');

CREATE TABLE communications (
    id uuid NOT NULL,
    created_at timestamp without time zone NOT NULL,
    delivery_method text NULL,
    notify_id text NULL,
    template_id text NULL,
    reason text NULL,
    evidence_request_id uuid NOT NULL,
    CONSTRAINT "PK_communications" PRIMARY KEY (id),
    CONSTRAINT "FK_communications_evidence_requests_evidence_request_id" FOREIGN KEY (evidence_request_id) REFERENCES evidence_requests (id) ON DELETE CASCADE
);

CREATE INDEX "IX_communications_evidence_request_id" ON communications (evidence_request_id);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20201214150433_CreateCommunications', '3.1.7');

ALTER TABLE evidence_requests ADD user_requested_by text NULL;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20210104115523_AddUserRequestedByToEvidenceApi', '3.1.7');

CREATE TABLE document_submissions (
    id uuid NOT NULL,
    created_at timestamp without time zone NOT NULL,
    claim_id text NULL,
    rejection_reason text NULL,
    state integer NOT NULL,
    evidence_request_id uuid NOT NULL,
    document_type_id text NULL,
    CONSTRAINT "PK_document_submissions" PRIMARY KEY (id),
    CONSTRAINT "FK_document_submissions_evidence_requests_evidence_request_id" FOREIGN KEY (evidence_request_id) REFERENCES evidence_requests (id) ON DELETE CASCADE
);

CREATE INDEX "IX_document_submissions_evidence_request_id" ON document_submissions (evidence_request_id);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20210106110856_CreateDocumentSubmission', '3.1.7');

ALTER TABLE evidence_requests ADD state int NULL;

ALTER TABLE communications ALTER COLUMN reason TYPE integer USING reason::integer;
ALTER TABLE communications ALTER COLUMN reason SET NOT NULL;
ALTER TABLE communications ALTER COLUMN reason DROP DEFAULT;

ALTER TABLE communications ALTER COLUMN delivery_method TYPE integer USING delivery_method::integer;
ALTER TABLE communications ALTER COLUMN delivery_method SET NOT NULL;
ALTER TABLE communications ALTER COLUMN delivery_method DROP DEFAULT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20210204164221_AddStateColumnToEvidenceRequest', '3.1.7');

ALTER TABLE evidence_requests ADD reason text NULL;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20210302123801_AddReasonColumnToEvidenceRequest', '3.1.7');

ALTER TABLE document_submissions ADD staff_selected_document_type_id text NULL;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20210318104225_AddStaffSelectedDocumentTypeId', '3.1.7');

ALTER TABLE evidence_requests ADD resident_reference_id text NULL;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20210329144435_AddResidentReferenceId', '3.1.7');
