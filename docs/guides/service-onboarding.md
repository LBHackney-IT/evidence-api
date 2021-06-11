# Onboard a new service

Onboarding a new service to the Case Evidence Service means getting **configuration** setup, and ensuring the new officers will be **authenticated** correctly.

The easiest way to do this is to use Github to edit the file directly and open a Pull Request for a developer to review. If this is your first time doing that, there is a [handy guide here](https://docs.github.com/en/github/managing-files-in-a-repository/editing-files-in-your-repository).

## 🔐 Authentication

To ensure the new service can authenticate properly, add their Google Groups to `auth-groups.json`. Make sure you're OK with everyone who is in the Google Goups getting access to the service (i.e. only add Google Groups which contain the people you want to give access).

The JSON file is designed to be easy to ammend by anyone—all it needs is the Google Group and a human readable name for the service.

⚠️ Do not delete a Google Group — this could break records that exist in the database for that service. If you need to, speak to a developer to update the database records first.

## ⚙️  Configuration

There are a couple of configurations that need to take place:

### Document Types

Add any document types that the service deals with to `EvidenceApi/DocumentTypes.json`. Ensure to add:
- a human readable title
- a description (this will be shown to the **resident**, so please ensure it describes what they need to provide and any important details)
- a machine readable ID (this should contain no spaces, and only contain lowercase letters, numbers and hyphens)

### Service Areas

Add their team configuration to `document-evidence-store-frontend/teams.json`. Ensure that the `id` is unique
