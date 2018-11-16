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
    {
        private readonly WebshopContext _context;

        public ProductController (WebshopContext context)
        {
            _context = context;
        }
        
        public class Complete_Product
        {
            public Product Product{get;set;}
            public string[] Images {get;set;}
            public IQueryable<string> Type{get;set;}
            public IQueryable<string> Category{get;set;}
            public IQueryable<string> Collection{get;set;}
            public IQueryable<string> Brand {get;set;}
            public IQueryable<int> Stock{get;set;}
        }    

        // GET api/product
        [HttpGet]
        public IActionResult GetAllProducts()
        {
            //Get a list of all products from the table Products and order them by Id
            var res = (from p in _context.Products orderby p let images = 
            (from i in _context.ProductImages where p.Id == i.ProductId select i.ImageURL).ToArray()
            let type = (from t in _context.Types where p._TypeId == t.Id select t._TypeName)
            let category = (from cat in _context.Categories where p.CategoryId == cat.Id select cat.CategoryName)
            let collection = (from c in _context.Collections where p.CollectionId == c.Id select c.CollectionName)
            let brand = (from b in _context.Brands where p.BrandId == b.Id select b.BrandName)
            let stock = (from s in _context.Stock where p.StockId == s.Id select s.ProductQuantity)
            select new Complete_Product(){Product = p, Images = images, Type = type, Category = category, Collection = collection, Brand = brand, Stock = stock}).ToArray();
            return Ok(res);
        }

        

        // GET api/product/details/5
        [HttpGet("details/{id}")]
        public IActionResult GetProductDetails(int id)
        {
            //Get a list of all products from the table products with the given id
            var res = (from p in _context.Products  where p.Id == id let images = 
            (from i in _context.ProductImages where p.Id == i.ProductId select i.ImageURL).ToArray()
            let type = (from t in _context.Types where p._TypeId == t.Id select t._TypeName)
            let category = (from cat in _context.Categories where p.CategoryId == cat.Id select cat.CategoryName)
            let collection = (from c in _context.Collections where p.CollectionId == c.Id select c.CollectionName)
            let brand = (from b in _context.Brands where p.BrandId == b.Id select b.BrandName)
            let stock = (from s in _context.Stock where p.StockId == s.Id select s.ProductQuantity)
            select new Complete_Product(){Product = p, Images = images, Type = type, Category = category, Collection = collection, Brand = brand, Stock = stock}).ToArray();
            return Ok(res);
        }

        // GET api/product/imageurl/5
        [HttpGet("imageurl/{id}")]
        public IActionResult GetImageURLs(int id)
        {
            //Get a list of all ImageURLs that belong to the product that has the given id
            var res = (from p in _context.Products from i in _context.ProductImages where p.Id == i.ProductId && p.Id == id select i.ImageURL).ToList();
            return Ok(res);
        }


        // GET api/product/1/10
        // GET api/product/{page number}/{amount of products on a page}
        [HttpGet("{page_index}/{page_size}")]
        public IActionResult GetProductsPerPage(int page_index, int page_size)
        {
            //Get a list of the right products, ordered by id
            //page_index-1 so the first page is 1 and not 0

            var res = (from p in _context.Products orderby p let images = 
            (from i in _context.ProductImages where p.Id == i.ProductId select i.ImageURL).ToArray()
            let type = (from t in _context.Types where p._TypeId == t.Id select t._TypeName)
            let category = (from cat in _context.Categories where p.CategoryId == cat.Id select cat.CategoryName)
            let collection = (from c in _context.Collections where p.CollectionId == c.Id select c.CollectionName)
            let brand = (from b in _context.Brands where p.BrandId == b.Id select b.BrandName)
            let stock = (from s in _context.Stock where p.StockId == s.Id select s.ProductQuantity)
            select new Complete_Product(){Product = p, Images = images, Type = type, Category = category, Collection = collection, Brand = brand, Stock = stock}).ToArray();


            int totalitems = res.Count();
            int totalpages = totalitems / page_size;
            totalpages = totalpages+1;
            page_index = page_index-1;
            int skip = page_index * page_size;
            res = res.Skip(skip).Take(page_size).ToArray();
            PaginationPage page = new PaginationPage {totalpages = totalpages, products = res};
            return Ok(page);
        }

        public class PaginationPage{
            public int totalpages {get;set;}
            public Complete_Product[] products {get;set;}
        }

        // POST api/product
        //verplicht meegeven: _typeid, categoryid, collectionid, brandid, stockid
        [HttpPost]
        //Gets input from the body that is type Product (in Json)
        public void CreateNewProduct([FromBody] Product product)
        {    
            //Add the input to the table Products and save
            _context.Products.Add(product);
            _context.SaveChanges();
        }

        // PUT api/product/5
        [HttpPut("{id}")]
        //Gets input from the body that is type Product (in Json)
        public void UpdateExistingProduct(int id, [FromBody] Product product)
        {
            //Find all products that has the given id in table Products
            Product p = _context.Products.Find(id);
            //Check if there is any input(value) for the attributes
            //If there is input, assign the new value to the attribute
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
            //Update the changes to the table and save
            _context.Update(p);
            _context.SaveChanges();
        }

        // DELETE api/product/5
        [HttpDelete("{id}")]
        public void DeleteProduct(int id)
        {
            //Find all products that has the given id in table Products
            Product Product = _context.Products.Find(id);
            //Delete the found products and save
            _context.Products.Remove(Product);
            _context.SaveChanges();
        }
    }
}
