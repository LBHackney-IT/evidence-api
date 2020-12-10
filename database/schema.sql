CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

CREATE TABLE evidence_request (
    id uuid NOT NULL,
    created_at timestamp without time zone NOT NULL,
    resident_id uuid NOT NULL,
    delivery_methods text[] NULL,
    document_types text[] NULL,
    service_requested_by text NULL,
    CONSTRAINT "PK_evidence_request" PRIMARY KEY (id)
);

CREATE TABLE resident (
    id uuid NOT NULL,
    created_at timestamp without time zone NOT NULL,
    name text NULL,
    email text NULL,
    phone_number text NULL,
    CONSTRAINT "PK_resident" PRIMARY KEY (id)
);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20201209135358_InitialCreate', '3.1.7');
