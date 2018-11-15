using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Backend_Website.Models;
using ExtensionMethods;
using restserver.Paginator;

namespace Backend_Website.Controllers
{
    [Route("api/[controller]")]

    public class ProductController : Controller
    {private readonly WebshopContext _context;

        public ProductController (WebshopContext context){ _context = context;}
    
        
    

        // GET api/product
         [HttpGet]
        public IActionResult GetAllProducts()
        {
            var res = (from p in _context.Products orderby p.Id select p).ToList();

            return Ok(res);
        }


        // GET api/product/details/5
        [HttpGet("details/{id}")]
        public IActionResult GetProductDetails(int id)
        {
            var res = (from p in _context.Products  where p.Id == id select p);
            return Ok(res);
                    }


        // GET api/product/1/10
        // GET api/product/{page number}/{amount of products on a page}
        [HttpGet("{page_index}/{page_size}")]
        public IActionResult GetProductsPerPage(int page_index, int page_size){
            var res = _context.Products.GetPage<Product>(page_index-1, page_size, p => p.Id);
            return Ok(res);
        }

        // POST api/product
        //verplicht meegeven: id, _typeid, categoryid, collectionid, brandid, stockid
        [HttpPost]
        public void CreateNewProduct([FromBody] Product product)
        {    
            _context.Products.Add(product);
            _context.SaveChanges();
        }

        // PUT api/product/5
              [HttpPut("{id}")]
        public void UpdateExistingProduct(int id, [FromBody] Product product)
        {
           Product p = _context.Products.Find(id);
           if(product.ProductNumber != null){p.ProductNumber = product.ProductNumber;}
           if(product.ProductEAN != null){p.ProductEAN = product.ProductEAN;}
           if(product.ProductInfo != null){p.ProductInfo = product.ProductInfo;}
           if(product.ProductDescription != null){p.ProductDescription = product.ProductDescription;}
           if(product.ProductSpecification != null){p.ProductSpecification = product.ProductSpecification;}
           if(product.ProductPrice != 0){p.ProductPrice = product.ProductPrice;}
           if(product.ProductColor != null){p.ProductColor = product.ProductColor;}
           if(product._TypeId!= 0){p._TypeId = product._TypeId;}
           if(product.CategoryId != 0){p.CategoryId = product.CategoryId;}
           if(product.CollectionId != 0){p.CollectionId = product.CollectionId;}
           if(product.BrandId != 0){p.BrandId = product.BrandId;}
           if(product.StockId != 0){p.StockId = product.StockId;}

            _context.Update(p);
            _context.SaveChanges();
        }

        // DELETE api/product/5
        [HttpDelete("{id}")]
        public void DeleteProduct(int id)
        {
           Product Product = _context.Products.Find(id);
           _context.Products.Remove(Product);
           _context.SaveChanges();
        }
    }
}
