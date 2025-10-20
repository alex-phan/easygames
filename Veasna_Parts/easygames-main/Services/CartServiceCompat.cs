using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EasyGames.ViewModels;

namespace EasyGames.Services
{
    /// <summary>
    /// Compatibility extensions for ICartService with unknown method names.
    /// Tries common names (Count/Total/Items/Clear). If not found, derives values from Items().
    /// </summary>
    public static class CartServiceCompat
    {
        // ---- Public extension methods the controller will call ----

        public static int CountCompat(this ICartService cart)
        {
            var m = FindZeroArg(cart, "Count", "GetCount", "ItemCount", "Quantity", "Qty");
            if (m != null) return Convert.ToInt32(m.Invoke(cart, null));
            return ItemsCompat(cart).Sum(i => i.Qty);
        }

        public static decimal TotalCompat(this ICartService cart)
        {
            var m = FindZeroArg(cart, "Total", "GetTotal", "Subtotal", "GetSubtotal", "Sum");
            if (m != null) return Convert.ToDecimal(m.Invoke(cart, null));
            return ItemsCompat(cart).Sum(i => i.Price * i.Qty);
        }

        public static List<CartItemVM> ItemsCompat(this ICartService cart)
        {
            var m = FindZeroArg(cart, "Items", "GetItems", "List", "All", "Get");
            if (m == null) return []; // IDE0305: collection expression for empty

            var raw = m.Invoke(cart, null);
            if (raw is IEnumerable<CartItemVM> already) return already.ToList();

            List<CartItemVM> list = []; // IDE0300/0028: prefer collection expression
            if (raw is IEnumerable seq)
            {
                foreach (var o in seq)
                {
                    var mapped = MapToCartItemVM(o);
                    if (mapped != null) list.Add(mapped);
                }
            }
            return list;
        }

        public static void ClearCompat(this ICartService cart)
        {
            var m = FindZeroArg(cart, "Clear", "ClearCart", "Empty", "Reset", "RemoveAll");
            if (m != null)
            {
                m.Invoke(cart, null);
                return;
            }

            // Best-effort fallback: try a Save/Set/Replace that accepts a collection
            var saveLike = FindMethods(cart, "Save", "Set", "Replace")
                .FirstOrDefault(mi =>
                {
                    var ps = mi.GetParameters();
                    return ps.Length == 1 && typeof(IEnumerable).IsAssignableFrom(ps[0].ParameterType);
                });

            if (saveLike != null)
            {
                var paramType = saveLike.GetParameters()[0].ParameterType;
                Array empty;
                if (paramType.IsArray)
                    empty = Array.CreateInstance(paramType.GetElementType()!, 0);
                else
                    empty = Array.CreateInstance(typeof(object), 0);

                saveLike.Invoke(cart, new object?[] { empty });
            }
        }

        // ---- Helpers ----

        private static MethodInfo? FindZeroArg(object target, params string[] names)
            => FindMethods(target, names).FirstOrDefault(mi => mi.GetParameters().Length == 0);

        private static IEnumerable<MethodInfo> FindMethods(object target, params string[] names)
        {
            var t = target.GetType();
            const BindingFlags BF = BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase;
            foreach (var n in names)
            {
                foreach (var mi in t.GetMember(n, MemberTypes.Method, BF).OfType<MethodInfo>())
                    yield return mi;
            }
        }

        private static CartItemVM? MapToCartItemVM(object o)
        {
            var t = o.GetType();

            object? P(string a, params string[] alt)
            {
                foreach (var name in new[] { a }.Concat(alt))
                {
                    var pi = t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                    if (pi != null) return pi.GetValue(o);
                }
                return null;
            }

            try
            {
                var id = Convert.ToInt32(P("ProductId", "Id"));
                var ttl = (P("Title", "Name") ?? "")?.ToString() ?? "";
                var cat = (P("Category") ?? "")?.ToString() ?? "";
                var pr = Convert.ToDecimal(P("Price", "UnitPrice", "Amount") ?? 0m);
                var qty = Convert.ToInt32(P("Qty", "Quantity", "Count") ?? 0);

                return new CartItemVM
                {
                    ProductId = id,
                    Title = ttl,
                    Category = cat,
                    Price = pr,
                    Qty = qty
                };
            }
            catch
            {
                return null;
            }
        }
    }
}
