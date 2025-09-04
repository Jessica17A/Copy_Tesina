using Microsoft.AspNetCore.Mvc;
using WebGradu.Models;
using Microsoft.EntityFrameworkCore;
using WebGradu.Data;
using System.Linq;

namespace WebGradu.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                // Último movimiento por producto
                var ultimosMovimientos = _context.Stocks
                    .GroupBy(s => s.Fk_Producto)
                    .Select(g => g.OrderByDescending(s => s.FechaMovimiento).FirstOrDefault())
                    .AsNoTracking()
                    .ToList();

                // Considera bajo stock (<= StockMinimo) e incluye agotados (0)
                var productosConStockBajo = ultimosMovimientos
                    .Where(s => s != null && s.StockActual <= (s.StockMinimo ?? 0))
                    .ToList();

                // Settea un MENSAJE de alerta si hay productos con bajo stock
                // (si quieres limitar a Admin, vuelve a añadir && User.IsInRole("Admin"))
                if (productosConStockBajo.Any())
                {
                    TempData["StockBajo"] = $"Hay {productosConStockBajo.Count} producto(s) con stock bajo o agotado.";
                }

                // La vista puede (o no) usar el modelo. Lo dejamos por si lo quieres mostrar.
                return View(productosConStockBajo);
            }

            return RedirectToAction("Inicio", "Home");
        }

        public IActionResult Privacy() => View();
        public IActionResult Inicio() => View();
    }
}
