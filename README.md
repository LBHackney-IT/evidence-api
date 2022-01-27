# Evidence API

Evidence API is a Platform API to allow services to request and upload evidence from members of the borough.

## Stack

-   .NET Core as a web framework.
-   nUnit as a test framework.

## What does it do?

[**🚀 Swagger**](https://app.swaggerhub.com/apis-docs/Hackney/evidence-api/1.0.0) ([Edit it here](https://app.swaggerhub.com/apis/Hackney/evidence-api/1.0.0))

## Context and history

See the [Architectural Decision Log](/docs/adr).

## Guides
-   [Service Onboarding](/docs/guides/service-onboarding.md)
-   [Tracking requests through environments](/docs/guides/tracking-requests.md)
-   [Diagrams](/docs/diagrams)

## Contributing

### Setup

1. Install [Docker][docker-download].
2. Install [AWS CLI][aws-cli].
3. Clone this repository.
4. Open it in your IDE.

### Development

In order to run the API locally, you will first need access to the environment variables stored in 1Password. Please contact another developer on the Document Evidence Service Team to gain access.

Once you have the environment variables, navigate via the terminal to the root of evidence-api repo and run `touch .env`. This will create an `.env` file where you can store them (following the pattern example in `.env.example`). This file should not be tracked by git, as it has been added to the `.gitignore`, so please do check that this is the case.

To set up the database container, run 
```sh
docker-compose up -d dev-database
``` 
in the root of the repo to get the database container up and running (the container serves the db for both evidence-api and documents-api services).

Once the environment variables have been added for evidence-api and the database is running, update the database by running the migration command 
```sh
dotnet ef --project EvidenceApi database update
```

Then run 
```sh 
dotnet run --project EvidenceApi
``` 
to start the API locally. It will run on `http://localhost:5000`.

### Testing

To run database tests locally the `CONNECTION_STRING` environment variable will need to be populated with: `Host=localhost;Database=testdb;Username=postgres;Password=mypassword"`, which you would have already done when you got the envars from another DES developer.

If changes to the database schema are made then the docker image for the database will have to be removed and recreated. The `restart-db` make command will do this for you (but your locally seeded data will be wiped).

There are two approaches to testing the evidence-api -- for different purposes. Your approach depends on whether you have inserted any data into `evidence-api_dev-datase_1` for manual/UI testing purposes.

> _1. I don't have any data seeded into my local database!_

Then, assuming that you have already followed the steps above to set up the repo and database container, and run the migration, you should have now one image running in Docker called `evidence-api_dev-databse_1`. In order to run the tests, all you need to do is run 
```sh
$ dotnet test
```

> _2. I do have data in my local seeded because I inserted it myself!_

Having locally seeded data in your db will cause tests to fail. In order to save your locally seeded data but run the tests as well, we're going to assume that you have done all the set up steps above already. In order to run the tests the second way, you will need to stop the container called `evidence-api_dev-database_1` by navigating via the UI or by running 
```sh
$ docker stop evidence-api_dev-databse_1
```

**Please note: as long as the attached volume for this container continues to run, you will not lose any manually seeded data when you stop this container.**

The reason why you need to stop it, is because we need to free up that port for a new container. Run 
```sh
$ make test
```
and a script will run to run all the tests on two new containers called `evidence-api_test-database_1` and `evidence-api_evidence-api-test_1`. These will be totally seaparate from your local dev db.

When all the test have passed, to start up the local db again, run 
```sh
$ docker stop evidence-api_test-database_1
```

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

_Prerequsite: Make sure you have your database running—something like `docker-compose up -d dev-database`_

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