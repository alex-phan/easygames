**Email Groups (Admin-only) — Implemented by Veasna**

**What it does**
•	Adds an Email Groups feature for admins (Owner role).
•	Admin can enter Subject + Message, choose Send to all users or target by Tier (optional).
•	Emails are rendered with a Razor template (falls back to simple HTML if the template isn’t present).
•	Works with real SMTP when configured; otherwise runs in simulated mode and shows a clear “Simulated send” status so the flow still demos cleanly.

**How it appears in the app**
•	After logging in as Owner, the navbar shows an Email Groups button.
•	The page has a polished Bootstrap card with the form.
•	After sending, an Email status page confirms the result (Success vs. Simulated) and shows the recipient count.

**Files I added/updated**

**Controllers**
•	Areas/Admin/Controllers/EmailsController.cs
•	Actions: Compose (GET/POST), Sent(count, simulated)
•	Owner-only; handles SMTP configured/not configured gracefully.

**Views**
•	Areas/Admin/Views/Emails/Compose.cshtml — form UI.
•	Areas/Admin/Views/Emails/Sent.cshtml — status card showing success or simulated mode.

**Services**
•	Services/IEmailService.cs — email interface.
•	Services/SmtpEmailService.cs — real SMTP sender (Razor view rendering + send).
•	Services/NullEmailService.cs — safe stub (no send) for demos/marking.
•	Services/SmtpOptions.cs — config model for SMTP settings.

**Startup**
•	Program.cs — conditional DI:
o	If Smtp:Host, Smtp:Username, Smtp:Password, Smtp:FromEmail exist → use SmtpEmailService.
o	Otherwise → use NullEmailService (simulated send, no errors).

No SMTP section → simulated send is used (still shows success UI).

**Notes**
•	Page restricted to Owner.
•	Tier filter currently uses EF.Property<int?>("Tier"); we can switch to User.Tier when that column exists.
•	The flow has been tested end-to-end with the simulated mode (recipient count shown on the status page).

