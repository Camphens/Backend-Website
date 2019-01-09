using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Backend_Website.Models;
using Backend_Website.Controllers;

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

        [HttpGet("Waterdicht/{page_index}/{page_size}")]
        public IActionResult Waterdicht(int page_index, int page_size)
        {
            var res = (from entries in _context.Products
                        where entries.ProductDescription.Contains("waterdicht")
                        orderby entries
                        let images = (from images in _context.ProductImages where images.ProductId == entries.Id select images.ImageURL).ToArray()
                        let type = (from types in _context.Types where entries._TypeId == types.Id select types._TypeName)
                        let category = (from categories in _context.Categories where categories.Id == entries.CategoryId select categories.CategoryName)
                        let collection = (from collections in _context.Collections where collections.Id == entries.Id select collections.CollectionName)
                        let brand =(from brands in _context.Brands where brands.Id == entries.BrandId select brands.BrandName)
                        let stock = (from stocks in _context.Stock where stocks.Id == entries.StockId select stocks.ProductQuantity)
                        select new Complete_Product(){Product =entries , Images = images, Type = type, Category = category, Collection = collection, Brand = brand, Stock = stock}).ToArray();
            
             int totalitems = res.Count();
            int totalpages = totalitems / page_size;
            //totalpages+1 because the first page is 1 and not 0
            totalpages = totalpages + 1;
            string Error = "Error";
            if (res.Count() < 1 | page_index < 1) return Ok(Error);
            //page_index-1 so the first page is 1 and not 0
            page_index = page_index - 1;
            int skip = page_index * page_size;
            res = res.Skip(skip).Take(page_size).ToArray();
            PaginationPage page = new PaginationPage { totalpages = totalpages, totalitems = totalitems, products = res };
            return Ok(page);
        }

        [HttpGet("Eastpak+zwart/{page_index}/{page_size}")]
        public IActionResult Zwarte_Eastpak_Tassen(int page_index, int page_size)
        {
            var res = (from entries in _context.Products
                         where entries.Brand.BrandName == "Eastpak" && entries.ProductColor == "zwart"
                         orderby entries
                        let images = (from images in _context.ProductImages where images.ProductId == entries.Id select images.ImageURL).ToArray()
                        let type = (from types in _context.Types where entries._TypeId == types.Id select types._TypeName)
                        let category = (from categories in _context.Categories where categories.Id == entries.CategoryId select categories.CategoryName)
                        let collection = (from collections in _context.Collections where collections.Id == entries.Id select collections.CollectionName)
                        let brand =(from brands in _context.Brands where brands.Id == entries.BrandId select brands.BrandName)
                        let stock = (from stocks in _context.Stock where stocks.Id == entries.StockId select stocks.ProductQuantity)
                        select new Complete_Product(){Product =entries , Images = images, Type = type, Category = category, Collection = collection, Brand = brand, Stock = stock}).ToArray();
            
            int totalitems = res.Count();
            int totalpages = totalitems / page_size;
            //totalpages+1 because the first page is 1 and not 0
            totalpages = totalpages + 1;
            string Error = "Error";
            if (res.Count() < 1 | page_index < 1) return Ok(Error);
            //page_index-1 so the first page is 1 and not 0
            page_index = page_index - 1;
            int skip = page_index * page_size;
            res = res.Skip(skip).Take(page_size).ToArray();
            PaginationPage page = new PaginationPage { totalpages = totalpages, totalitems = totalitems, products = res };
            return Ok(page);
            
        }

        [HttpGet("Burkely+blauw/{page_index}/{page_size}")]
        public IActionResult Blauwe_Burkely_Tassen(int page_index, int page_size)
        {
            var res = (from entries in _context.Products
                         where entries.Brand.BrandName == "Burkely" && entries.ProductColor == "blauw"
                         orderby entries
                        let images = (from images in _context.ProductImages where images.ProductId == entries.Id select images.ImageURL).ToArray()
                        let type = (from types in _context.Types where entries._TypeId == types.Id select types._TypeName)
                        let category = (from categories in _context.Categories where categories.Id == entries.CategoryId select categories.CategoryName)
                        let collection = (from collections in _context.Collections where collections.Id == entries.Id select collections.CollectionName)
                        let brand =(from brands in _context.Brands where brands.Id == entries.BrandId select brands.BrandName)
                        let stock = (from stocks in _context.Stock where stocks.Id == entries.StockId select stocks.ProductQuantity)
                        select new Complete_Product(){Product =entries , Images = images, Type = type, Category = category, Collection = collection, Brand = brand, Stock = stock}).ToArray();

            
            int totalitems = res.Count();
            int totalpages = totalitems / page_size;
            //totalpages+1 because the first page is 1 and not 0
            totalpages = totalpages + 1;
            string Error = "Error";
            if (res.Count() < 1 | page_index < 1) return Ok(Error);
            //page_index-1 so the first page is 1 and not 0
            page_index = page_index - 1;
            int skip = page_index * page_size;
            res = res.Skip(skip).Take(page_size).ToArray();
            PaginationPage page = new PaginationPage { totalpages = totalpages, totalitems = totalitems, products = res };
            return Ok(page);
        }

        [HttpGet("harde-koffers+Rimowa/{page_index}/{page_size}")]
        public IActionResult HardeKoffersRimowa(int page_index, int page_size)
        {
            var res = (from entries in _context.Products
                        where entries._Type._TypeName == "harde-koffers" && entries.Brand.BrandName == "Rimowa"
                        orderby entries
                        let images = (from images in _context.ProductImages where images.ProductId == entries.Id select images.ImageURL).ToArray()
                        let type = (from types in _context.Types where entries._TypeId == types.Id select types._TypeName)
                        let category = (from categories in _context.Categories where categories.Id == entries.CategoryId select categories.CategoryName)
                        let collection = (from collections in _context.Collections where collections.Id == entries.Id select collections.CollectionName)
                        let brand =(from brands in _context.Brands where brands.Id == entries.BrandId select brands.BrandName)
                        let stock = (from stocks in _context.Stock where stocks.Id == entries.StockId select stocks.ProductQuantity)
                        select new Complete_Product(){Product =entries , Images = images, Type = type, Category = category, Collection = collection, Brand = brand, Stock = stock}).ToArray();

            int totalitems = res.Count();
            int totalpages = totalitems / page_size;
            //totalpages+1 because the first page is 1 and not 0
            totalpages = totalpages + 1;
            string Error = "Error";
            if (res.Count() < 1 | page_index < 1) return Ok(Error);
            //page_index-1 so the first page is 1 and not 0
            page_index = page_index - 1;
            int skip = page_index * page_size;
            res = res.Skip(skip).Take(page_size).ToArray();
            PaginationPage page = new PaginationPage { totalpages = totalpages, totalitems = totalitems, products = res };
            return Ok(page);
            
        }

        public class Complete_Product
        {
            public Product Product { get; set; }
            public string[] Images { get; set; }
            public IQueryable<string> Type { get; set; }
            public IQueryable<string> Category { get; set; }
            public IQueryable<string> Collection { get; set; }
            public IQueryable<string> Brand { get; set; }
            public IQueryable<int> Stock { get; set; }
        }

        public class PaginationPage
        {
            public int totalpages { get; set; }
            public int totalitems { get; set; }
            public Complete_Product[] products { get; set; }
        }
}
}