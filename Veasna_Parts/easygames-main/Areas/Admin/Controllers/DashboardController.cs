using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using EasyGames.Repositories;
using EasyGames.Filters;

namespace EasyGames.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AuthorizeOwner] // or [Authorize(Roles = "Owner")]
    public class DashboardController : Controller
    {
        private readonly IUnitOfWork _uow;
        public DashboardController(IUnitOfWork uow) => _uow = uow;

        public async Task<IActionResult> Index()
        {
            ViewBag.ProductCount = await _uow.ProductCountAsync();
            ViewBag.UserCount = await _uow.UserCountAsync();
            ViewBag.LowStock = await _uow.Products.CountAsync(p => p.StockQty <= 5);

            return View();
        }
    }
}

