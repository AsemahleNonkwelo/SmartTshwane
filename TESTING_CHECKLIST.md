# SmartTshwane Task 2 — Manual testing checklist

**Evidence status:** Entries labelled VERIFIED were demonstrated in screenshots during this chat; other entries require confirmation on the lecturer's machine. This is not an automated test suite.

| Test | Status | How to verify |
|---|---|---|
| App runs and renders Razor UI | VERIFIED | Open home, My Reports |
| Citizen creates a report | VERIFIED | Create Water Leaks report #TSH-1 |
| PostgreSQL cloud persistence | VERIFIED | Supabase Requests table shows the created record |
| Admin dashboard displays report | VERIFIED | Admin > dashboard |
| Admin status and note update | VERIFIED | Change Pending to In Progress and save note |
| Cloud reflects admin changes | VERIFIED | Supabase Requests table Status and AdminNote |
| Citizen sees updated status and note | VERIFIED | Citizen > My Reports > View details |
| Citizen blocked from /Admin | VERIFIED | Browser redirected to Access denied |
| Empty/invalid registration rejected | NEEDS TEST | Submit invalid registration form |
| Duplicate email rejected | NEEDS TEST | Register with an existing email |
| Invalid login rejected | NEEDS TEST | Enter incorrect password |
| Empty report fields rejected | NEEDS TEST | Submit blank form |
| Citizen cannot read another user's report | NEEDS TEST | Sign in as another citizen; open first user's details URL |
| Local SQLite fallback after extraction | NEEDS TEST | Use default settings, launch from clean extraction |
| Restart retains records | NEEDS TEST | Stop and restart; sign in again |
| Mobile responsiveness and accessibility | NEEDS TEST | Resize browser and test keyboard navigation |
| Automated tests/CI | NOT PROVIDED | Add separate test project and CI if required |
