using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using EasyGames.ViewModels;

namespace EasyGames.Services
{
    public interface ICartService
    {
        // Core, parameterless (use IHttpContextAccessor internally)
        List<CartItemVM> Items();
        int Count();
        decimal Total();
        void Clear();

        void Add(int productId, int qty = 1);
        void Increase(int productId);
        void Decrease(int productId);
        void Remove(int productId);

        // Legacy-compatible overloads (ignore ctx – call parameterless versions)
        List<CartItemVM> Items(HttpContext _);
        int Count(HttpContext _);
        decimal Total(HttpContext _);
        void Clear(HttpContext _);
    }
}

