using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebGradu.Models;
using System.Linq;
using System.Threading.Tasks;
using CloudinaryDotNet;
using WebGradu.Data;
using Microsoft.AspNetCore.Authorization;

namespace WebGradu.Controllers
{
    [Authorize]
    public class NotificacionesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly Cloudinary _cloudinary; // Añadir Cloudinary

        public NotificacionesController(ApplicationDbContext context, Cloudinary cloudinary) // Inyectar Cloudinary
        {
            _context = context;
            _cloudinary = cloudinary;
        }
    [HttpGet]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // 1) Filtra solo el ÚLTIMO movimiento de cada producto con subconsulta correlacionada
            var bajos = await _context.Stocks
                .Where(s => s.FechaMovimiento ==
                            _context.Stocks
                                .Where(x => x.Fk_Producto == s.Fk_Producto)
                                .Max(x => x.FechaMovimiento))
                // 2) Regla de bajo stock (ajústala a tu gusto)
                .Where(s => s.StockActual <= s.StockMinimo || s.StockActual == 0)
                // 3) Carga el Producto en la misma query
                .Include(s => s.Producto)
                .AsNoTracking()
                .ToListAsync();

            // 4) Si usas Cloudinary para la URL pública, solo transforma la propiedad Foto (no toques otras)
            foreach (var s in bajos)
                if (s.Producto != null && !string.IsNullOrWhiteSpace(s.Producto.Foto))
                    s.Producto.Foto = ObtenerUrlImagen(s.Producto.Foto);

            return View(bajos); // @model IEnumerable<WebGradu.Models.Stock>
        }





        // Método para generar la URL segura de la imagen usando el public_id
        private string ObtenerUrlImagen(string publicId)
        {
            if (string.IsNullOrEmpty(publicId))
            {
                return "/path/to/default/image.jpg"; // Devuelve una imagen predeterminada si no hay imagen
            }

            var url = _cloudinary.Api.UrlImgUp
                        .Secure(true) // Para generar una URL segura (https)
                        .BuildUrl(publicId);
            return url;
        }
    }
}
