# EasyGames — Overview Project README (Gislene)

> **Lecturer Admin Access**
>
> **Email:** `owner.admin@easygames.local`  
> **Password:** `GiveThisToLecturer123!`  
> *(Public pages allow self-registration. You may also create a customer account from the UI.)*

**Generated on:** 2025-09-13 08:53  

Hi, this is my **HIT339 Assessment 2** project. Below is my README and overview of my work.

---

## What I built (quick tour)
- **Public:** Landing → Store (**search + category**) → Cart → Checkout → Done  
- **Admin (Owner only):** Dashboard + **Products CRUD** + **Users CRUD**  
- **Cart:** Session-backed, DI service, **navbar cart badge** via ViewComponent  
- **Auth:** Custom **cookie authentication**; users stored in **EF Core** with **BCrypt** password hashes  
- **Data:** EF Core + **SQLite**, **idempotent seeding**, one-time Owner bootstrap via **env-vars**  
- **Styling:** **Bootstrap** + custom CSS (glass, gradients); **consistent image ratios**  
- **Motion:** a subtle **dot-wave canvas** behind the **Top Featured Collectibles** section only; respects **`prefers-reduced-motion`**

---

## Why I used the unit material **and** external resources (and exactly how)

- **Microsoft Learn — .NET Fundamentals** (DI, options, logging)  
  https://learn.microsoft.com/en-us/dotnet/fundamentals/

- **Microsoft Learn — ASP.NET Core MVC tutorial** (routing, tag helpers, MVC patterns)  
  https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-mvc-app/start-mvc?view=aspnetcore-9.0&preserve-view=true&tabs=visual-studio

- **YouTube: ASP.NET Core MVC eCommerce Made Easy (NET 8)** — I **followed this tutorial** to understand the complete shop flow (list → cart → checkout → admin) and to see a fuller example than a minimal “hello world”.  
  https://www.youtube.com/watch?v=rTQgaQ5N9ZA&list=TLPQMDQwODIwMjXhJJGLB5okMQ&index=45

- **Books**
  - Turtschi, Werry, Hack, & Albahari (2002) *C# .NET Developer's Guide* — fundamentals & language depth.  
  - Nagel (2021) *Professional C# and .NET* — DI lifetimes, layering, EF Core patterns.

- **Excalidraw** — wireframes for Landing → Store → Cart → Checkout.  
  https://excalidraw.com/

- **AI product images** — **Gemini / “Google Nano Banana”** to create coursework-only assets that match the **vintage neon** vibe; I also used **ChatGPT** to clarify concepts while building.

---

## Software Requirements Compliance (HIT339)

**Owner functionalities**
- **Stock screen (CRUD):** Implemented at `/Admin/Products` (Admin Area). Owners can create, read, update, and delete products. Search by Title; Category parse supported.
- **User screen (CRUD):** Implemented at `/Admin/Users`. Owners can create, read, update, and delete users. Protected with `AuthorizeOwner`.

**User functionalities**
- **Create account:** Implemented at `/Account/Register` and linked from the public UI. Registration stores users in EF Core with **BCrypt** password hashes.
- **View stock:** Implemented at `/Store`. Users can browse/search by Title and filter by Category (Book/Game/Toy). Cards use stable image ratios to avoid layout shifts.
- **Purchase items:** Implemented via **Cart** and **Checkout**. Users can add to cart, adjust quantities, and complete a demo checkout. An **Order** with code `EG-YYYYMMDD-####` is created and the cart is cleared.

> **Note:** Checkout is intentionally a **demo flow** (no payment gateway), appropriate for this assessment.
## note## “All product/diagram images in this coursework are **AI-generated (Google Nano Banana, 2025)**

---

## Design choices — my reasoning

### Dot-wave behind featured cards only
- Adds life to the product area **without competing** with the hero.
- Sits **behind** content; uses theme colours; respects **`prefers-reduced-motion`**.
- File: `/wwwroot/js/landing-wave-grid.js`.

### Toast on “Add to cart”
- Immediate feedback after POST; **navbar cart badge** updates via the ViewComponent.
- If it’s quiet on your machine, see the **tiny fix list** at the end.

### Product cards & imagery
- Ratio container + `object-fit: cover` to prevent layout jumps.
- Soft shadows maintain the **glass** and **vintage** style.

### Search & Category
- **Title LIKE** + **Category enum** filter; EF Core composes LINQ → SQL and runs it in the DB.

### Hovers & micro-interactions
- Subtle focus/colour states; accessible and not distracting.

---

## Authentication choice (custom cookie vs Identity)
For this coursework I implemented a **custom authentication system** using **cookie auth** and an EF Core `Users` table with **BCrypt** password hashes.  
It supports **register**, **login**, **logout**, **change password**, and **Owner-only** admin access.  
If this app grows, I can migrate to **ASP.NET Core Identity** to add built-in email confirmation, password-reset tokens, lockout, 2FA, and external providers.

---

## What clicked for me about **DbContext**
- `DbContext` is the **unit of work**: it tracks changes; `SaveChanges()` persists atomically.
- **Scoped** lifetime (one per request) fits MVC.
- LINQ queries execute **in the database**.
- Migrations shape schema; my **DataSeeder** is **idempotent** and supports Owner recovery via env-vars.

---

## Challenges & how I solved them

### CS1061 (“does not contain a definition …”)
This appeared multiple times.  
**Fix:** verify the **type** at the call site, `using` directives, and extension-method namespaces.  
Reference: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-messages/cs1061

### Analyzer hints (style rules)
- **IDE0290** (Primary constructor) — **avoided** on EF entities/controllers for clarity and compatibility.  
- **IDE0028** (Collection initializer) — **adopted** where it clarified seeding/list creation.  
- **IDE0300/IDE0305** (Collection expressions) — **evaluated**; kept classic initializers for consistency in this unit.

References:  
IDE0290 https://learn.microsoft.com/en-au/dotnet/fundamentals/code-analysis/style-rules/ide0290  
IDE0028 https://learn.microsoft.com/en-au/dotnet/fundamentals/code-analysis/style-rules/ide0028  
IDE0300 https://learn.microsoft.com/en-au/dotnet/fundamentals/code-analysis/style-rules/ide0300  
IDE0305 https://learn.microsoft.com/en-au/dotnet/fundamentals/code-analysis/style-rules/ide0305

### Error page / diagnostics setup (was I correct?)
**Yes.**  
- In **Development**, ASP.NET Core shows the **Developer Exception Page** automatically.  
- In **Production**, I route to `/Home/Error` and enable **HSTS**.  
This standard template pattern helped me see detailed errors during development and a friendly page otherwise.

### Other fixes during the build
- **Image 404s** slowing first paint → normalized `/wwwroot/images/**` and backfilled `ImageUrl`.  
- “**Deleted product came back**” → seeder made **idempotent** + a small **dedupe** step.  
- **Admin 404s & search quirks** → conventional routing; strongly-typed views; enum-safe parsing.  
- **Toast regression** → noted; one-liner controller `TempData` + small `_Layout` snippet re-enables it.

---

## Technical work by stages

**Stage 1 — Foundation:** packages, connection string, DI, session, auth, folders.  
**Stage 1.2 — Domain & Data:** `User`, `Product`, `Order`, `OrderItem`; `ApplicationDbContext`; migrations; `DataSeeder`.  
**Stage 1.3 — Repos/Services:** `IRepository<T>`, `IUnitOfWork`; `AuthService` (BCrypt), `CartService`.  
**Stage 1.4 — Auth & Public:** `AccountController`, `HomeController`; purple glass theme.  
**Stage 1.5 — Cart & Checkout:** cart actions; checkout creates **EG-YYYYMMDD-####**; clears cart; Done page.  
**Stage 1.6 — Admin:** `/Admin` Area; Dashboard; **Products CRUD**; **Users CRUD**; `AuthorizeOwner`.  
**Stage 2–6 — Fixes & polish:** admin table, routing anchors, footer gradient, image hygiene, idempotent seed, CTAs that pre-filter Store.  
**Stage 7 — Micro-interaction:** **dot-wave** behind product cards only.

---

## `Program.cs` mapping (what each block is for and notes reference)

- `AddControllersWithViews` — MVC with Razor/tag helpers.  
- `AddDbContext<ApplicationDbContext>(UseSqlite(...))` — EF Core, **Scoped** lifetime.  
- `AddDistributedMemoryCache`, `AddSession`, `AddHttpContextAccessor` — session cart infrastructure.  
- `AddAuthentication().AddCookie(...)` — minimal cookie auth.  
- **DI:** `IUnitOfWork`, `IAuthService` (BCrypt), `ICartService`, `DataSeeder`.  
- **Middleware order:** static → routing → **session** → **authentication** → **authorization**.  
- **Routes:** Areas first; `/` → `Home.Landing`; `/Store` → `Home.Index`; then default conventional.

---

## Data model

- **Product** `{ Id, Title, Category (enum), Price, StockQty, ImageUrl, ... }`  
- **User** `{ Id, Email, DisplayName, PasswordHash, Role }`  
- **Order / OrderItem** (for checkout record)

**Seeder** is idempotent and supports Owner recovery with env-vars:
- `EG_OWNER_EMAIL_PRIMARY`
- `EG_BOOTSTRAP_OWNER_FROM`
- `EG_BOOTSTRAP_OWNER_TO`
- `EG_BOOTSTRAP_SET_OWNER_PASSWORD`

---

## How to run

1) Build the **EasyGames** profile. First run creates **easygames.db** and seeds data.  
2) Navigate: `/` (Landing) → `/Store` → `/Cart` → `/Checkout` → **Done** → `/Admin` (Owner only).  
3) **Admin login for marking:**  
   - Email: `owner.admin@easygames.local`  
   - Password: `GiveThisToLecturer123!`

---

## Tiny fix list (if marking live)

- **Add-to-cart toast:** set `TempData["Toast"]="Added to cart!"` in `CartController.Add`; read once in `_Layout` to render a small toast (auto-dismiss).  
- Optional: unique index on **Products.Title**.  
- Optional: migrate to **ASP.NET Core Identity** if the app grows.

---

## External sources (citations)

- Microsoft Learn — .NET Fundamentals: https://learn.microsoft.com/en-us/dotnet/fundamentals/  
- Microsoft Learn — ASP.NET Core MVC tutorial:  
  https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-mvc-app/start-mvc?view=aspnetcore-9.0&preserve-view=true&tabs=visual-studio  
- YouTube — *ASP.NET Core MVC eCommerce Made Easy (NET 8)*:  
  https://www.youtube.com/watch?v=rTQgaQ5N9ZA&list=TLPQMDQwODIwMjXhJJGLB5okMQ&index=45  
- Books:  
  - Turtschi, A., Werry, J., Hack, G., & Albahari, J. (2002). *C# .NET Developer's Guide*. Elsevier.  
  - Nagel, C. (2021). *Professional C# and .NET*. Wiley.  
- Excalidraw — https://excalidraw.com/  
- AI images — Gemini / “Google Nano Banana” coursework-only assets.

---

## Appendix — direct answer to my question about the error/diagnostics setup
**Yes, the setup is correct.**  
In **Development**, ASP.NET Core shows the **Developer Exception Page** automatically;  
in **Production**, it routes to `/Home/Error` and enables **HSTS**.  
That’s how I could see detailed errors while building and fall back to a friendly page otherwise.
