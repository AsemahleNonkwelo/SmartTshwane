# SmartTshwane — Task 2 evidence and rubric mapping

## Evidence demonstrated during development (16 October 2026)
Screenshots were shared in the development conversation, but the image files are **not embedded in this ZIP**. Attach your originals separately in the evidence document if required.

1. **Cloud database:** Supabase Table Editor displayed `Categories`, `Requests`, and `Users`; `Requests` contained a submitted Water Leaks report.
2. **Admin functionality:** Dashboard displayed the report and allowed a status change from Pending to In Progress with an admin note.
3. **Cloud write persistence:** Supabase `Requests` table displayed `Status = In Progress` and the admin note.
4. **Citizen tracking:** My Reports showed report #TSH-1 as In Progress.
5. **Role security:** A citizen navigating to `/Admin` was shown Access denied.
6. **Citizen detail:** `/Requests/Details/1` displayed In Progress and the admin's saved note.

## Task 2 rubric assessment (not a guaranteed mark)
| Criterion | Evidence and remaining work |
|---|---|
| UI/UX | Functional, clean Razor pages shown; accessibility/mobile review not demonstrated |
| Addresses user requirements | Core citizen/admin reporting implemented; some Task 1 requirements remain unimplemented |
| Back and front-end solution | ASP.NET Core MVC controller + Razor view workflow demonstrated |
| Cloud database | Supabase PostgreSQL connected; data stored and updated |
| Database connectivity | Cloud create/read/update confirmed in screenshots |
| Development according to design | MVC differs from originally proposed Next.js + API architecture; document this explicitly |
| Executes without errors | Core workflows ran; no claim of exhaustive error-free operation |
| Error handling | Data annotations, model validation, invalid-login response, 404 and invalid-status checks in code; negative tests still needed |
| Security | Role-based Admin restriction demonstrated; PBKDF2, CSRF and ownership checks in source; further negative testing needed |
| System files | Structured MVC project, README, testing checklist, project and solution files; cleaned build artifacts |

## Before handing in
- Capture evidence screenshots from your lab (cloud table, admin page, citizen details, access denied, successful build).
- Attach the actual Task 2 rubric and any lecturer-requested document.
- Confirm your submission requires a ZIP and whether GitHub URL, collaborator access, or automated tests are separately mandated.
- Do not place database passwords, personal user secrets, or private test-user data in a public repository.
- Verify a fresh extraction and local fallback run on Visual Studio 2022.
