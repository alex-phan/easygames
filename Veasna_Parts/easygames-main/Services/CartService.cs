using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using EasyGames.Data;
using EasyGames.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace EasyGames.Services
{
    // Session cart service: JSON in session, tolerant to a couple of key names.
    public class CartService : ICartService
    {
        private const string KeyPrimary = "eg.cart";
        private static readonly string[] LegacyKeys = { "cart", "Cart", "EasyGames.Cart" };

        private readonly IHttpContextAccessor _http;
        private readonly ApplicationDbContext _db;

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };

        public CartService(IHttpContextAccessor http, ApplicationDbContext db)
        {
            _http = http;
            _db = db;
        }

        // ---------- Public API (parameterless) ----------
        public List<CartItemVM> Items()
        {
            var session = _http.HttpContext?.Session;
            if (session == null) return new();

            // Try primary key first; then fall back to legacy keys.
            string? json = session.GetString(KeyPrimary);
            if (string.IsNullOrWhiteSpace(json))
            {
                foreach (var k in LegacyKeys)
                {
                    json = session.GetString(k);
                    if (!string.IsNullOrWhiteSpace(json)) break;
                }
            }

            if (string.IsNullOrWhiteSpace(json)) return new();

            try
            {
                var list = JsonSerializer.Deserialize<List<CartItemVM>>(json!, JsonOpts) ?? new();
                // guard: sanitize negatives/nulls
                foreach (var i in list) { if (i.Qty < 0) i.Qty = 0; }
                return list;
            }
            catch
            {
                return new();
            }
        }

        public int Count() => Items().Sum(i => i.Qty);
        public decimal Total() => Items().Sum(i => i.Price * i.Qty);

        public void Clear()
        {
            var session = _http.HttpContext?.Session;
            if (session == null) return;

            // Persist empty list to the primary key; also remove legacy keys.
            session.SetString(KeyPrimary, JsonSerializer.Serialize(new List<CartItemVM>(), JsonOpts));
            foreach (var k in LegacyKeys) session.Remove(k);
        }

        public void Add(int productId, int qty = 1)
        {
            if (qty < 1) qty = 1;
            var list = Items();

            var existing = list.FirstOrDefault(i => i.ProductId == productId);
            if (existing != null)
            {
                existing.Qty += qty;
                Save(list);
                return;
            }

            var p = _db.Products.AsNoTracking().FirstOrDefault(x => x.Id == productId);
            if (p == null) return;

            list.Add(new CartItemVM
            {
                ProductId = p.Id,
                Title = p.Title,
                Category = p.Category.ToString(),
                Price = p.Price,
                Qty = qty
            });

            Save(list);
        }

        public void Increase(int productId)
        {
            var list = Items();
            var it = list.FirstOrDefault(i => i.ProductId == productId);
            if (it != null)
            {
                it.Qty++;
                Save(list);
            }
        }

        public void Decrease(int productId)
        {
            var list = Items();
            var it = list.FirstOrDefault(i => i.ProductId == productId);
            if (it != null)
            {
                it.Qty--;
                if (it.Qty <= 0)
                    list.Remove(it);

                Save(list);
            }
        }

        public void Remove(int productId)
        {
            var list = Items();
            list.RemoveAll(i => i.ProductId == productId);
            Save(list);
        }

        // ---------- Legacy-compatible overloads (ignore HttpContext param) ----------
        public List<CartItemVM> Items(HttpContext _) => Items();
        public int Count(HttpContext _) => Count();
        public decimal Total(HttpContext _) => Total();
        public void Clear(HttpContext _) => Clear();

        // ---------- Internals ----------
        private void Save(List<CartItemVM> list)
        {
            var session = _http.HttpContext?.Session;
            if (session == null) return;

            var json = JsonSerializer.Serialize(list, JsonOpts);
            session.SetString(KeyPrimary, json);

            // keep storage tidy
            foreach (var k in LegacyKeys) session.Remove(k);
        }
    }
}


