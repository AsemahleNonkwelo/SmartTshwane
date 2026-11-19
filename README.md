# SmartTshwane — Work Integrated Learning Task 2

**Project:** SmartTshwane, a student demonstration of a municipal service-request tracking system (not an official City of Tshwane service).

## Requirements
- Visual Studio 2022 with **ASP.NET and web development** workload
- .NET 8 SDK
- Internet connection for first-time NuGet restore; Supabase mode also needs Internet access

## Option A: Run immediately using local SQLite (recommended for marking)
1. Extract the entire ZIP to a writable folder (do not run from inside the ZIP).
2. Open `SmartTshwane.sln` in Visual Studio 2022.
3. Wait for NuGet restore. Build > Build Solution.
4. Confirm `appsettings.json` has `UsePostgres: false` and `ConnectionStrings.Default: Data Source=smarttshwane.db` (these are the supplied defaults).
5. Press F5 or Ctrl+F5; accept the local development HTTPS certificate if prompted.
6. Register a new citizen, sign in, submit a report, open My Reports and View details. The app creates the local SQLite database and initial categories automatically.

**Note:** Local SQLite is a functional offline fallback, not evidence of cloud hosting. The SQLite file is intentionally not packaged because it can contain test users and their password hashes; it will be created on first launch.

## Option B: Connect to Supabase PostgreSQL (cloud demonstration)
This source includes EF Core's Npgsql PostgreSQL provider. Supabase cloud integration was manually verified during development: the citizen submitted a report, the admin changed its status and note, and the updated record appeared in Supabase Table Editor and on the citizen details page. The database password is **not** included in this ZIP.

1. Obtain connection details for an authorized demonstration Supabase database. If using Supabase's IPv4 network, choose its **Session pooler** connection parameters.
2. In Visual Studio, right-click the SmartTshwane project > **Manage User Secrets**. Enter a JSON object such as:

```json
{
  "UsePostgres": true,
  "ConnectionStrings": {
    "Default": "Host=YOUR_POOLER_HOST;Port=5432;Database=postgres;Username=YOUR_POOLER_USERNAME;Password=YOUR_PASSWORD;SSL Mode=Require"
  }
}
```

3. Replace placeholders with your actual connection parameters; do not include `postgresql://` when using the Npgsql key/value format above. Alternatively set environment variables `UsePostgres=true` and `ConnectionStrings__Default=...`.
4. Start the app. On a **fresh, empty demonstration database**, EF Core `EnsureCreated()` creates the application tables and seeds categories. Existing schema/tables need appropriate preparation; this project does not include migrations.
5. Register a new citizen and submit a test report. Verify the row in Supabase > Table Editor > `Requests`; changes made by the admin should appear there too.

**Credentials:** A database password permits direct database access, not merely app sign-in. Share credentials separately with authorized teammates if required; never commit them to GitHub or embed them in a widely distributed submission ZIP. Do not give the lecturer your personal Supabase account password.

## Admin demonstration
1. Start the app in **Development** mode. Register a separate test user intended to be the administrator.
2. Stop the app. In **Manage User Secrets**, add `"DevelopmentAdminEmail": "admin-test-email@example.com"` to the existing JSON (preserve any other settings).
3. Restart the app. The development-only setup in `Program.cs` changes an **existing** matching user's role to `Admin`.
4. Log out and sign back in as that user to refresh their authentication cookie. The **Admin** menu will appear. Update a report's status and note.
5. Log out and sign in as the original citizen to verify the updated status and note. Attempt `/Admin` as a citizen to confirm access denied.

**Security note:** Development admin promotion is for a controlled demonstration only. Remove the `DevelopmentAdminEmail` setting when no longer needed; do not use this mechanism for a production deployment.

## Architecture and features
- ASP.NET Core MVC (.NET 8), Razor views, EF Core; SQLite local fallback or PostgreSQL cloud mode.
- Citizen registration/login, PBKDF2 salted password hashes, authentication cookies.
- Citizen report creation, listing, detail view and category selection.
- Admin-only dashboard, report status and note updates.
- Antiforgery validation on form POSTs, model validation and per-user report access checks.

## Implementation scope and Task 1 deviations
- Task 1 proposed a Next.js frontend and a separate ASP.NET Core Web API; Task 2 uses ASP.NET Core MVC with Razor views in one solution.
- Authentication uses ASP.NET Core cookies and role-based authorization rather than the originally proposed JWT approach.
- The enhanced application includes citizen phone registration, optional report coordinates, department and category administration, user-role management, and in-app notifications.
- The application is a student demonstration with Supabase PostgreSQL integration and a local SQLite fallback; it is not an officially deployed municipal service.
- Automated test coverage and production deployment are outside the demonstrated scope. The accompanying evidence document records manual application and cloud database testing.

## Submission evidence
See `SmartTshwane_Task2_Evidence_Final.docx` in the submission package for application screenshots, technical explanations, references, and the video demonstration link. Any additional project notes and testing files are supplementary.

## Academic attribution
This is a student project. Describe actual individual contributions and include the rubric/other deliverables requested by the lecturer. Do not claim automated tests or deployment that have not been completed.

## Task 1 requirement enhancements (16 October 2026)
The final application extends the initial report-management workflow with a phone field during citizen registration, optional latitude and longitude for reports, municipal department management, category-to-department assignments, administrator user-role management, and in-app notifications. Existing Pending, In Progress, Resolved, and Rejected statuses remain available.

## Operational notes
- New notifications appear within the application rather than through email, SMS, or push messaging.
- Report coordinates are manually entered where available; browser-based geolocation is not part of the demonstrated workflow.
- Existing databases may require schema updates when moving between versions. Back up important data before changing database configuration or schema.
- Changes to a user's role may require signing out and signing back in so authentication claims are refreshed.
- Database credentials belong in user secrets or environment variables and must not be committed to source control.
