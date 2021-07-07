# Onboard a new service

Onboarding a new service to the Document Evidence Service means getting **configuration** setup, and ensuring the new officers will be **authenticated** correctly.

The easiest way to do this is to use Github to edit the file directly and open a Pull Request for a developer to review. If this is your first time doing that, there is a [handy guide here](https://docs.github.com/en/github/managing-files-in-a-repository/editing-files-in-your-repository).

## 🔐 Authentication

To ensure the new service can authenticate properly, add their Google Groups to `document-evidence-store-frontend/auth-groups.json`.
- Groups are configured for each environment - `development`, `staging`, and `production` - which allows us to be specific about which group is able to access instances of the service
- Make sure you're OK with everyone who is in the Google Groups getting access to the service (i.e. only add Google Groups which contain the people you want to give access).

The JSON file is designed to be easy to amend by anyone—all it needs is the Google Group and a human readable name for the service.

⚠️ Do not delete a Google Group — this could break records that exist in the database for that service. If you need to, speak to a developer to update the database records first.

## ⚙️ Configuration

There are a couple of configurations that need to take place:

### Service Areas

Add their team configuration to `document-evidence-store-frontend/teams.json`. Ensure to add:
- a machine readable ID (this should contain no spaces, and only contain lowercase letters, numbers and hyphens)
- a human readable group name
- Google Group that matches configuration in `auth-groups.json`
- Other fields which the service provided in the onboarding sheet, such as _reasons_ and _landingMessage_

### Document Types

Add any document types that the service deals with to `EvidenceApi/DocumentTypes.json` and `EvidenceAPi/StaffSelectedDocumentTypes.json`. Ensure to add:
- the team ID that was created in `teams.json`
- a human readable title
- a description (this will be shown to the **resident**, so please ensure it describes what they need to provide and any important details)
