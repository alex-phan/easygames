using System;
using System.Collections.Generic;
using System.Linq;
using EasyGames.Models;
using Microsoft.EntityFrameworkCore;

namespace EasyGames.Data
{

    /// <summary>
    /// seed demo data and owner; apply optional one-time Owner fixups via env vars (see Notepad seed1)
    /// env keys (set → run once → clear):
    ///   EG_OWNER_EMAIL_PRIMARY          = canonical Owner email (default: owner@easygames.local)
    ///   EG_BOOTSTRAP_OWNER_FROM         = current email to upgrade/rename (default: EG_OWNER_EMAIL_PRIMARY)
    ///   EG_BOOTSTRAP_OWNER_TO           = new Owner email (optional)
    ///   EG_BOOTSTRAP_SET_OWNER_PASSWORD = new Owner password (optional; hashed on save)
    /// notes:
    ///   - creates Owner + one sample customer if missing
    ///   - runs migrations before seeding
    /// </summary>

    public class DataSeeder
    {
        private readonly ApplicationDbContext _db;
        public DataSeeder(ApplicationDbContext db) => _db = db;

        public async Task SeedAsync()
        {
            await _db.Database.MigrateAsync();

            // canonical Owner email can be overridden so we don't recreate an old address later.
            // chatGPT Prompt BOOT-002 start
            // Purpose: env-var name used to set canonical Owner email for one-time bootstrap/ownership.
            var ownerEmailPrimary = (Environment.GetEnvironmentVariable("EG_OWNER_EMAIL_PRIMARY")
                                     ?? "owner@easygames.local").Trim().ToLowerInvariant();
            // chatGPT Prompt BOOT-002 end

            // --- users ---
            await EnsureOwnerAsync(
                name: "Owner",
                email: ownerEmailPrimary,
                password: "Owner123!",
                address: "Admin St");

            await EnsureCustomerAsync(
                name: "Alice WOLLIES",
                email: "alice@example.com",
                password: "Password123!",
                address: "1 Sample Rd");

            // One-time owner fixups: promote, rename, or reset password if env vars are present
            await ApplyOwnerFixupsAsync(ownerEmailPrimary);
            await _db.SaveChangesAsync();

            // --- images map for safe backfill ---
            var img = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["ASP.NET Core: Secrets of Session"] = "/images/products/aspnet-core-secrets-of-session.png",
                ["AldiGrocerie Tycoon"] = "/images/products/aldigrocerie-tycoon.png",
                ["C# in a Weekend"] = "/images/products/csharp-in-a-weekend.png",
                ["ColesPoints Quest"] = "/images/products/colespoints-quest.png",
                ["Design Patterns with Rubber Duck Debugging"] = "/images/products/design-patterns-with-rubber-duck-debugging.png",
                ["DI Snap-Together Kit"] = "/images/products/di-snap-together-kit.png",
                ["EF Core for Sleep-Deprived Students"] = "/images/products/ef-core-for-sleep-deprived-students.png",
                ["HIT 339 Thunder Bolt"] = "/images/HIT_339_Thunder_Bolt.png",
                ["HIT339 — Rohan: A Very Cool Lecturer (Collector’s Edition)"] = "/images/HIT339_Rohan_A_Very_Cool_Teacher_Collectors_Edition.png",
                ["LINQ Cubes"] = "/images/products/linq-cubes.png",
                ["Mildeware Mayhem"] = "/images/products/mildeware-mayhem.png",
                ["Pixel Racer"] = "/images/products/pixel-racer.png",
                ["Robo Dino"] = "/images/products/robo-dino.png",
                ["Rubber Duck Debugger"] = "/images/products/rubber-duck-debugger.png",
                ["Tuesday Late Night Show — Study & Chill"] = "/images/products/tuesday-late-night-show-study-&-chill.png",
                ["XuXuPillow"] = "/images/products/xuxupillow.png",
                ["WolliesPunk"] = "/images/WolliesPunk.png",
                ["C# Bear"] = "/images/CSharp_Bear.png",

                // note: animated product uses the GIF in /wwwroot/animations

            };

            // If I already have products, clean + backfill only (do NOT reseed)
            if (await _db.Products.AnyAsync())
            {
                // remove titles I know I don't want
                var removeTitles = new[]
                {
                    "HIT339 — Rohan: A Very Nice Teacher (Collector’s Edition)", // duplicate I decided to drop but it did not worked 
                     // typo of "Middleware"I fix it later 
                };

                var toRemove = await _db.Products
                    .Where(p => removeTitles.Contains(p.Title))
                    .ToListAsync();

                if (toRemove.Count > 0)
                {
                    _db.Products.RemoveRange(toRemove);
                    await _db.SaveChangesAsync();
                }

                // de-duplicate exact same titles (keep the oldest Id)
                var dupTitles = await _db.Products
                    .GroupBy(p => p.Title)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToListAsync();

                foreach (var t in dupTitles)
                {
                    var rows = await _db.Products
                        .Where(p => p.Title == t)
                        .OrderBy(p => p.Id)
                        .ToListAsync();

                    var keep = rows.First();
                    var drop = rows.Skip(1).ToList();
                    if (drop.Count > 0)
                        _db.Products.RemoveRange(drop);
                }

                // backfill missing ImageUrl from the map
                var all = await _db.Products.ToListAsync();
                foreach (var p in all)
                {
                    if (string.IsNullOrWhiteSpace(p.ImageUrl) && img.TryGetValue(p.Title, out var url))
                        p.ImageUrl = url;

                    // --- new: owner fields backfill after migration ---
                    // note: keep cost non-negative and default to 0 for older rows
                    if (p.CostPrice < 0m) p.CostPrice = 0m;

                    // note to self: normalise empty supplier strings to null for cleaner display
                    if (p.Supplier != null && string.IsNullOrWhiteSpace(p.Supplier))
                        p.Supplier = null;
                }

                await _db.SaveChangesAsync();
                return; // important: do not reseed when rows existed
            }

            // --- table empty: seed canonical list once ---
            EnsureProduct("Tuesday Late Night Show — Study & Chill",
                Category.Book, 14.95m, 18, img.GetValueOrDefault("Tuesday Late Night Show — Study & Chill"));

            EnsureProduct("EF Core for Sleep-Deprived Students",
                Category.Book, 19.90m, 10, img.GetValueOrDefault("EF Core for Sleep-Deprived Students"));

            EnsureProduct("ASP.NET Core: Secrets of Session",
                Category.Book, 21.50m, 8, img.GetValueOrDefault("ASP.NET Core: Secrets of Session"));

            EnsureProduct("Design Patterns with Rubber Duck Debugging",
                Category.Book, 17.75m, 9, img.GetValueOrDefault("Design Patterns with Rubber Duck Debugging"));

            EnsureProduct("HIT 339 Thunder Bolt",
                Category.Game, 39.00m, 20, img.GetValueOrDefault("HIT 339 Thunder Bolt"));

            EnsureProduct("HIT339 — Rohan: A Very Cool Lecturer (Collector’s Edition)",
                Category.Book, 2350.00m, 22, img.GetValueOrDefault("HIT339 — Rohan: A Very Cool Lecturer (Collector’s Edition)"));

            EnsureProduct("WolliesPunk",
                Category.Game, 29.50m, 30, img.GetValueOrDefault("WolliesPunk"));

            EnsureProduct("AldiGrocerie Tycoon",
                Category.Game, 34.99m, 16, img.GetValueOrDefault("AldiGrocerie Tycoon"));

            EnsureProduct("ColesPoints Quest",
                Category.Game, 27.49m, 22, img.GetValueOrDefault("ColesPoints Quest"));




            EnsureProduct("C# Bear",
                Category.Toy, 12.50m, 25, img.GetValueOrDefault("C# Bear"));

            EnsureProduct("LINQ Cubes",
                Category.Toy, 9.99m, 40, img.GetValueOrDefault("LINQ Cubes"));

            EnsureProduct("Rubber Duck Debugger",
                Category.Toy, 7.95m, 60, img.GetValueOrDefault("Rubber Duck Debugger"));

            EnsureProduct("EF Core Blocks (Migration Edition)",
                Category.Toy, 15.00m, 18, null);

            EnsureProduct("DI Snap-Together Kit",
                Category.Toy, 11.25m, 26, img.GetValueOrDefault("DI Snap-Together Kit"));

            EnsureProduct("C# in a Weekend",
                Category.Book, 49.99m, 8, img.GetValueOrDefault("C# in a Weekend"));

            EnsureProduct("Pixel Racer",
                Category.Game, 39.00m, 12, img.GetValueOrDefault("Pixel Racer"));

            EnsureProduct("Robo Dino",
                Category.Toy, 29.50m, 20, img.GetValueOrDefault("Robo Dino"));



            await _db.SaveChangesAsync();
        }

        // ---------- helpers ----------

        private async Task EnsureOwnerAsync(string name, string email, string password, string address)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                _db.Users.Add(new User
                {
                    Name = name,
                    Email = email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                    Address = address,
                    Role = Role.Owner
                });
            }
            else if (user.Role != Role.Owner)
            {
                user.Role = Role.Owner; // keep admin access solid
            }
        }

        private async Task EnsureCustomerAsync(string name, string email, string password, string address)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                _db.Users.Add(new User
                {
                    Name = name,
                    Email = email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                    Address = address,
                    Role = Role.Customer
                });
            }
        }

        private void EnsureProduct(string title, Category category, decimal price, int stock, string? imageUrl)
        {
            // add product to catalog (no duplicates by title)
            var exists = _db.Products.Any(p => p.Title.ToLower() == title.ToLower());
            if (exists) return;

            _db.Products.Add(new Product
            {
                Title = title,
                Category = category,
                Price = price,
                StockQty = stock,
                ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl,

                // note to self: explicit owner fields for clarity (default cost 0, no supplier)
                CostPrice = 0m,
                Supplier = null
            });
        }

        /// <summary>
        /// One-time Owner fixups via environment variables.

        private async Task ApplyOwnerFixupsAsync(string ownerEmailPrimary)
        {
            // chatGPT Prompt BOOT-002 start (added to report)
            // Purpose: env-var names used for one-time Owner bootstrap (promote/rename/set password).
            string fromEmail = (Environment.GetEnvironmentVariable("EG_BOOTSTRAP_OWNER_FROM")
                               ?? ownerEmailPrimary).Trim().ToLowerInvariant();
            string? toEmail = Environment.GetEnvironmentVariable("EG_BOOTSTRAP_OWNER_TO")?.Trim().ToLowerInvariant();
            string? newPassword = Environment.GetEnvironmentVariable("EG_BOOTSTRAP_SET_OWNER_PASSWORD");
            // chatGPT Prompt BOOT-002 end

            // Find the "from" account (or the "to" account if it's already been renamed)
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == fromEmail);
            if (user == null && !string.IsNullOrWhiteSpace(toEmail))
                user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == toEmail);

            if (user == null) return;

            // Promote to Owner
            user.Role = Role.Owner;

            // Rename email if requested and not conflicting
            if (!string.IsNullOrWhiteSpace(toEmail) &&
                !string.Equals(user.Email, toEmail, StringComparison.OrdinalIgnoreCase))
            {
                var conflict = await _db.Users.AnyAsync(u => u.Email.ToLower() == toEmail);
                if (!conflict)
                    user.Email = toEmail;
            }

            // Update password if provided
            if (!string.IsNullOrWhiteSpace(newPassword))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            }
        }
    }
}
