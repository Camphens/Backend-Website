using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Backend_Website.Models;

namespace Backend_Website.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchTermController : Controller
    {
        private readonly WebshopContext _context;

        public SearchTermController(WebshopContext context)
        {
            _context = context;
        }

        [HttpGet("Waterdicht")]
        public IActionResult Waterdicht()
        {
            var query = (from products in _context.Products
                        where products.ProductDescription.Contains("waterdicht")
                        select products).ToArray();
            return Ok(query);
        }

        [HttpGet("Eastpak+zwart")]
        public IActionResult Zwarte_Eastpak_Tassen()
        {
            var query = (from entries in _context.Products
                         where entries.Brand.BrandName == "Eastpak" && entries.ProductColor == "zwart"
                         select entries).ToArray();
            return Ok(query);
        }

        [HttpGet("Burkely+blauw")]
        public IActionResult Blauwe_Burkely_Tassen()
        {
            var query = (from entries in _context.Products
                         where entries.Brand.BrandName == "Burkely" && entries.ProductColor == "blauw"
                         select entries).ToArray();
            return Ok(query);
        }

        [HttpGet("harde-koffers+Rimowa")]
        public IActionResult HardeKoffersRimowa()
        {
            var query = (from entries in _context.Products
                        where entries._Type._TypeName == "harde-koffers" && entries.Brand.BrandName == "Rimowa" 
                        select entries).ToArray();
            return Ok(query);
        }
}
}