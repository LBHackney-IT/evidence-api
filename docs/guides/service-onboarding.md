# Onboard a new service

Onboarding a new service to the Document Evidence Service requires:

1. Setting up the service specific **configuration** by creating a new sheet in [this document](https://docs.google.com/spreadsheets/d/1EYz-bE3EkvgOlQ2beWSXFr8_hzTH4Fo307mRFN88Ff0/edit#gid=232433808)
2. **Authentication**, which will depend on your use case, see section below.

## ⚙️ Configuration

There are a couple of configurations that need to take place:

### Service Areas

Add their team configuration to `document-evidence-store-frontend/teams.json`. Ensure to add:

-   a machine readable ID (this should contain no spaces, and only contain lowercase letters, numbers and hyphens)
-   a human readable group name
-   Google Group that matches configuration in `auth-groups.json`
-   Other fields which the service provided in the onboarding sheet, such as _reasons_ and _landingMessage_

### Document Types

Add any document types that the service deals with to `EvidenceApi/DocumentTypes.json` and `EvidenceAPi/StaffSelectedDocumentTypes.json`. Ensure to add:

-   the team ID that was created in `teams.json`
-   a human readable title
-   a description (this will be shown to the **resident**, so please ensure it describes what they need to provide and any important details)

The easiest way to do this is to use Github to edit the file directly and open a Pull Request for a developer to review. If this is your first time doing that, there is a [handy guide here](https://docs.github.com/en/github/managing-files-in-a-repository/editing-files-in-your-repository). Alternatively, you can ask a developer to help you with this instead.

## 🔐 Authentication

This depends on the team use case:

-   Officers create and manage evidence requests through the DES frontend
    -   Follow the _Document Evidence Service Frontend_ section
-   Programmatically perform some actions with officers managing evidence requests through the DES frontend
    -   Follow _both_ sections
-   Programmatic access only
    -   Follow the _APIs_ section

### Document Evidence Service Frontend

To ensure **officers** of your service can be authenticated properly with the DES frontend, add their Google Groups to `document-evidence-store-frontend/auth-groups.json`.

-   Groups are configured for each environment - `development`, `staging`, and `production` - which allows us to be specific about which group is able to access instances of the service
-   Make sure you're OK with everyone who is in the Google Groups getting access to the service (i.e. only add Google Groups which contain the people you want to give access).

The JSON file is designed to be easy to amend by anyone—all it needs is the Google Group and a human readable name for the service.

⚠️ Do not delete a Google Group — this could break records that exist in the database for that service. If you need to, speak to a developer to update the database records first.

### APIs

#### SwaggerHub

-   [evidence-api](https://app.swaggerhub.com/apis-docs/Hackney/evidence-api/1.0.0)
-   [documents-api](https://app.swaggerhub.com/apis-docs/Hackney/documents-api/1.0.0)

#### HTTP Headers

At the time of writing 2 headers are required for interacting with `evidence-api` endpoints. These are detailed in the _SwaggerHub Docs_ and we also wanted to describe them here.

##### Authorization

The client needs to provide an **Authorization bearer token**. We use the [HackIT Lambda authorizer](https://docs.google.com/document/d/1mpTY-sfYwR2brIF_8KjxiYzW6zgkjbv4Pi-9Y5LRlBA/edit) which means tokens are required for each **HTTP method** and **path**.

To give an example - if a client is wanting to invoke the following endpoints:

-   `POST /api/v1/evidence_requests/{evidenceRequestId}/document_submissions`
-   `GET /api/v1/evidence_requests/{id}`

The service area will need to contact someone from the HackIT team to have these tokens generated for them. A token per path and per environment is needed. Other information that needs to be provided is the AWS account and the service name.

-   `POST evidence_requests`
-   `GET evidence_requests`

##### UserEmail

This field is for audit purposes and should be the name of the service accessing the endpoint.

## Help!

If you're stuck then please reach out on our Slack channel [#document-management-support](https://hackit-lbh.slack.com/archives/C01CR9SM96F)
