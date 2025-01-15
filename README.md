# Evidence API

Evidence API is a Platform API to allow services to request and upload evidence from members of the borough.   

## Stack

-   .NET 6.0 as a web framework.
-   nUnit v3.12 as a test framework.

## What does it do?

[**🚀 Swagger**](https://app.swaggerhub.com/apis-docs/Hackney/evidence-api/1.0.0) ([Edit it here](https://app.swaggerhub.com/apis/Hackney/evidence-api/1.0.0))

## Context and history

See the [Architectural Decision Log](/docs/adr).

## Guides
-   [Service Onboarding](/docs/guides/service-onboarding.md)
-   [Tracking requests through environments](/docs/guides/tracking-requests.md)
-   [Diagrams](/docs/diagrams)

## Contributing

### Prerequisites

1. Install [Docker][docker-download] (>v20.10).
2. Install [AWS CLI][aws-cli].
3. Clone this repository.
4. Open it in your IDE of your choice.

### 1. Set up envars

In order to run the API locally, you will first need access to the environment variables stored in 1Password. Please contact another developer on the Document Evidence Service Team to gain access. Once you have the environment variables, navigate via the terminal to the root of evidence-api repo and run `touch .env`. This will create an `.env` file where you can store them (following the pattern example in [.env.example](.env.example)).

In the absence of a developer, you can also populate the values yourself. You will need access to the application's staging environment on AWS. Create a new `.env` file with the values from `.env.example`. Then, retrieve the values from the staging environment in AWS following the variables listed in [serverless.yml](EvidenceApi/serverless.yml).

The only values you do not need to copy from AWS are:
- CONNECTION_STRING -- use the value in `.env.example`
- EVIDENCE_REQUEST_CLIENT_URL -- use the value in `.env.example`
- DOCUMENTS_API_URL -- use the value in `.env.example`
- DOCUMENTS_API_POST_CLAIMS_TOKEN - assign this to `value`
- DOCUMENTS_API_POST_DOCUMENTS_TOKEN - assign this to `value`
- DOCUMENTS_API_GET_CLAIMS_TOKEN - assign this to `value`
- DOCUMENTS_API_PATCH_CLAIMS_TOKEN - assign this to `value`

This `.env` file should not be tracked by git, as it has been added to the `.gitignore`, so please do check that this is the case.

### 2. Set up containers

Next step, to set up the local Evidence API container, `cd` into the root of the project
(same place as the Makefile) and run `make serve-local`. This will set up the database container,
run an automatic migration and stand up the API container. There are other Make recipes in the file;
```
# build the image and start the db, migration and API containers
$ make serve-local

# build the images
$ make build-local

# start the db, migration and API containers
$ make start-local
```

* The API will run on `http://localhost:3001`
* The database will run on `http://localhost:3002`

### 3. Testing

There are two ways to test the application:

1. Run the tests in the test container
2. Run them locally

The simplest and most reliable way is running the tests in the container. You can do this by running `make serve-test`,
which will build the images and run the containers. There are other Make recipes in the file;
```
# build the image and start the db, migration and test containers
$ make serve-test

# build the images
$ make build-test

# start the db, migration and test containers
$ make start-test
```
However, you might want to run the tests locally, in order to debug them through your IDE. The codebase is also set up to allow this.
All you need to do is to make sure that your `CONNECTION_STRING` envar in `.env` is the same as the one in `.env.example`.

Then, run `make start-local` and make sure to clean any local data that you might have added -- you can do this manually through a database application
like TablePlus or DataGrip.

Then you can run your tests by running `dotnet test` or through your IDE.

### Agreed Testing Approach

-   Use nUnit, FluentAssertions and Moq
-   Always follow a TDD approach
-   Tests should be independent of each other
-   Gateway tests should interact with a real test instance of the database
-   Test coverage should never go down
-   All use cases should be covered by E2E tests
-   Optimise when test run speed starts to hinder development
-   Unit tests and E2E tests should run in CI
-   Test database schemas should match up with production database schema
-   Have integration tests which test from the PostgreSQL database to API Gateway

### Database Things

To modify the database schema:

_Prerequsite: Make sure you have your local database running by running `make start-local`_

1. Create or edit the `entity` files corresponding to the models you want to modify
2. Run `bin/dotnet ef --project EvidenceApi migrations add NameOfMigration`  substituting the name of the migration
3. Check the migration to make sure it does what you want (and not other things)—it can be found in `EvidenceApi/Migrations`
4.
    - If the migration looks good, run `bin/dotnet ef --project EvidenceApi database update`  to run the migrations
    - If the migration looks bad, run `bin/dotnet ef --project EvidenceApi migrations remove` to wipe the migration

### Notify

The application uses GOV.UK Notify to send emails and SMS.

When running the application locally we make calls to the Notify service and so you need to:
1. Ask to be invited to the _Hackney Upload_ group so you can access the Notify dashboard
2. Update your local `.env` file with the correct values for the properties `NOTIFY_TEMPLATE_REMINDER_*`, `NOTIFY_TEMPLATE_EVIDENCE_*` and `NOTIFY_API_KEY` (these should use the **staging** API key)

## Release process

We use a pull request workflow, where changes are made on a branch and approved by one or more other maintainers before the developer can merge into `master` branch.

![Circle CI Workflow Example](docs/circle_ci_workflow.png)

Then we have an automated six step deployment process, which runs in CircleCI.

1. Automated tests (nUnit) are run to ensure the release is of good quality.
2. The application is deployed to development automatically, where we check our latest changes work well.
3. We manually confirm a staging deployment in the CircleCI workflow once we're happy with our changes in development.
4. The application is deployed to staging.
5. We manually confirm a production deployment in the CircleCI workflow once we're happy with our changes in staging.
6. The application is deployed to production.

Our staging and production environments are hosted by AWS. We would deploy to production per each feature/config merged into `master` branch.

### Creating A PR

Before you commit or push your code, you will need to run:

```sh
 dotnet tool install dotnet-format --version 5.1.225507
```

Otherwise your PR will automatically fail the CircleCI checks. This will install the formatting tool for the repository. From thereon, you can run:
```sh
dotnet dotnet-format
```
to format your code.

To help with making changes to code easier to understand when being reviewed, we've added a PR template.

When a new PR is created on a repo that uses this API template, the PR template will automatically fill in the `Open a pull request` description textbox.
The PR author can edit and change the PR description using the template as a guide.

## Static Code Analysis

### Using [FxCop Analysers](https://www.nuget.org/packages/Microsoft.CodeAnalysis.FxCopAnalyzers)

FxCop runs code analysis when the Solution is built.

Both the API and Test projects have been set up to **treat all warnings from the code analysis as errors** and therefore, fail the build.

However, we can select which errors to suppress by setting the severity of the responsible rule to none, e.g `dotnet_analyzer_diagnostic.<Category-or-RuleId>.severity = none`, within the `.editorconfig` file.
Documentation on how to do this can be found [here](https://docs.microsoft.com/en-us/visualstudio/code-quality/use-roslyn-analyzers?view=vs-2019).

## Data Migrations

### A good data migration

-   Record failure logs
-   Automated
-   Reliable
-   As close to real time as possible
-   Observable monitoring in place
-   Should not affect any existing databases

## File Uploads

For security reasons, the MIME types that a resident can upload must be whitelisted on both client side and server side. This means that a resident cannot upload a file that does not meet the approved whitelist. For example, a resident cannot upload a file with an extension of `.svg` because the MIME type `image/svg+xml` has not been added to the whitelist. Please see the previous pen-test reports for more information. The following MIME types are blacklisted:

- `image/svg+xml` (could contain scripts)
- `application/octet-stream` (unknown binary-type files)

### Adding a new accepted MIME type

There are two places where a new MIME type needs to be whitelisted; the client (frontend) and the server (evidence-api). To update how the client accepts MIME types, please see the README on [Document Evidence Store Frontend](https://github.com/LBHackney-IT/document-evidence-store-frontend).

To update the accepted MIME types on the server, navigate to [AcceptedMimeTypes.cs](EvidenceApi/AcceptedMimeTypes.cs) and add the MIME types to the `acceptedMimeTypes` property of the class. A list of authoritative MIME types can be found on [MDN](https://developer.mozilla.org/en-US/docs/Web/HTTP/Basics_of_HTTP/MIME_types) and [IANA](https://www.iana.org/assignments/media-types/media-types.xhtml).

### Current accepted MIME types

| MIME type                                                               | File extension |
| ----------------------------------------------------------------------- | -------------- |
| application/msword                                                      | .doc           |
| application/pdf                                                         | .pdf           |
| application/vnd.apple.numbers                                           | .numbers       |
| application/vnd.apple.pages                                             | .pages         |
| application/vnd.ms-excel                                                | .xls           |
| application/vnd.openxmlformats-officedocument.spreadsheetml.sheet       | .xlsx          |
| application/vnd.openxmlformats-officedocument.wordprocessingml.document | .docx          |
| image/bmp                                                               | .bmp           |
| image/gif                                                               | .gif           |
| image/heic                                                              | .heic          |
| image/jpeg                                                              | .jpeg or .jpg  |
| image/png                                                               | .png           |
| text/plain                                                              | .txt           |

## Contacts

### Active Maintainers

-   **Selwyn Preston**, Lead Developer at London Borough of Hackney (selwyn.preston@hackney.gov.uk)
-   **Mirela Georgieva**, Lead Developer at London Borough of Hackney (mirela.georgieva@hackney.gov.uk)
-   **Matt Keyworth**, Lead Developer at London Borough of Hackney (matthew.keyworth@hackney.gov.uk)

### Contributors

-   **Neil Mendum**, Senior Engineer at Made Tech (neil.mendum@hackney.gov.uk)
-   **Bogdan Zaharia**, Engineer at Made Tech (bogdan.zaharia@hackney.gov.uk)

### Other Contacts

-   **Rashmi Shetty**, Product Owner at London Borough of Hackney (rashmi.shetty@hackney.gov.uk)

[docker-download]: https://www.docker.com/products/docker-desktop
[universal-housing-simulator]: https://github.com/LBHackney-IT/lbh-universal-housing-simulator
[made-tech]: https://madetech.com/
[aws-cli]: https://aws.amazon.com/cli/

# License

[MIT](./LICENSE)
