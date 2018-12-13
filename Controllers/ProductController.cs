using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Backend_Website.Models;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Linq.Expressions;

namespace Backend_Website.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class ProductController : Controller
    {
        private readonly WebshopContext _context;

        public ProductController(WebshopContext context)
        {
            _context = context;
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

        public class SearchProduct
        {
            public int totalitems { get; set; }
            public IOrderedQueryable products { get; set; }
        }

        // public class FilterV
        // {
        //     public string filter1 {get;set;}
        //     public string filter2 {get;set;}
        //     public string filter3 {get;set;}
        //     public string filter4 {get;set;}
        //     public string filter5 {get;set;}
        // }

        public class Filter
        {
            public int kind { get; set; }
            public string att { get; set; }
            public object value { get; set; }
            public Filter a1 { get; set; }
            public Filter a2 { get; set; }
        }

        // GET api/product
        [HttpGet]
        public IActionResult GetAllProducts()
        {
            //Get a list of all products from the table Products and order them by Id
            var res = (from p in _context.Products
                       orderby p
                       let images =
(from i in _context.ProductImages where p.Id == i.ProductId select i.ImageURL).ToArray()
                       let type = (from t in _context.Types where p._TypeId == t.Id select t._TypeName)
                       let category = (from cat in _context.Categories where p.CategoryId == cat.Id select cat.CategoryName)
                       let collection = (from c in _context.Collections where p.CollectionId == c.Id select c.CollectionName)
                       let brand = (from b in _context.Brands where p.BrandId == b.Id select b.BrandName)
                       let stock = (from s in _context.Stock where p.StockId == s.Id select s.ProductQuantity)
                       select new Complete_Product() { Product = p, Images = images, Type = type, Category = category, Collection = collection, Brand = brand, Stock = stock }).ToArray();
            return Ok(res);
        }



        // GET api/product/details/5
        [HttpGet("details/{id}")]
        public IActionResult GetProductDetails(int id)
        {
            //Get a list of all products from the table products with the given id
            var res = (from p in _context.Products
                       where p.Id == id
                       let images =
(from i in _context.ProductImages where p.Id == i.ProductId select i.ImageURL).ToArray()
                       let type = (from t in _context.Types where p._TypeId == t.Id select t._TypeName)
                       let category = (from cat in _context.Categories where p.CategoryId == cat.Id select cat.CategoryName)
                       let collection = (from c in _context.Collections where p.CollectionId == c.Id select c.CollectionName)
                       let brand = (from b in _context.Brands where p.BrandId == b.Id select b.BrandName)
                       let stock = (from s in _context.Stock where p.StockId == s.Id select s.ProductQuantity)
                       select new Complete_Product() { Product = p, Images = images, Type = type, Category = category, Collection = collection, Brand = brand, Stock = stock }).ToArray();
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
            //Get a list of all products with all related info from other tables, ordered by id
            var res = (from p in _context.Products
                       orderby p
                       let images =
(from i in _context.ProductImages where p.Id == i.ProductId select i.ImageURL).ToArray()
                       let type = (from t in _context.Types where p._TypeId == t.Id select t._TypeName)
                       let category = (from cat in _context.Categories where p.CategoryId == cat.Id select cat.CategoryName)
                       let collection = (from c in _context.Collections where p.CollectionId == c.Id select c.CollectionName)
                       let brand = (from b in _context.Brands where p.BrandId == b.Id select b.BrandName)
                       let stock = (from s in _context.Stock where p.StockId == s.Id select s.ProductQuantity)
                       select new Complete_Product() { Product = p, Images = images, Type = type, Category = category, Collection = collection, Brand = brand, Stock = stock }).ToArray();


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

        // // POST api/product
        // //verplicht meegeven: _typeid, categoryid, collectionid, brandid, stockid
        // [HttpPost]
        // //Gets input from the body that is type Product (in Json)
        // public void CreateNewProduct([FromBody] Product product)
        // {    
        //     //Add the input to the table Products and save
        //     _context.Products.Add(product);
        //     _context.SaveChanges();
        // }

        [HttpPost("CreateC")]
        public void CreateCategory(dynamic Categorydetails)
        {
            dynamic CategorydetailsJSON = JsonConvert.DeserializeObject(Categorydetails.ToString());
            Console.WriteLine(CategorydetailsJSON);

            Category Category = new Category()
            {
                CategoryName = CategorydetailsJSON.CategoryName,
                Id = CategorydetailsJSON.CategoryId
            };
            _context.Categories.Add(Category);

            _Type Type = new _Type()
            {
                _TypeName = CategorydetailsJSON.TypeName,
                Id = CategorydetailsJSON.TypeId

            };
            _context.Types.Add(Type);

            Category_Type CT = new Category_Type()
            {
                CategoryId = Category.Id,
                _TypeId = Type.Id
            };
            _context.CategoryType.Add(CT);
            _context.SaveChanges();
        }

        [HttpGet("filter/{page_index}/{page_size}")]
        public IActionResult GetFilter(
            int page_index,
            int page_size,
            [FromQuery(Name = "BrandId")] int[] BrandId,
            [FromQuery(Name = "ProductColor")] string[] ProductColor,
            [FromQuery(Name = "_TypeId")] int[] _TypeId,
            [FromQuery(Name = "CollectionId")] int[] CollectionId,
            [FromQuery(Name = "CategoryId")] int[] CategoryId
            )
        {
            IQueryable<Complete_Product> res = null;
            var result = _context.Products.Select(m => m);
            if (BrandId.Length != 0)
            {
                result = result.Where(m => BrandId.Contains(m.BrandId));
                res = from p in result
                      let image = (from i in _context.ProductImages where p.Id == i.ProductId select i.ImageURL).ToArray()
                      let type = (from t in _context.Types where p._TypeId == t.Id select t._TypeName)
                      let category = (from cat in _context.Categories where p.CategoryId == cat.Id select cat.CategoryName)
                      let collection = (from c in _context.Collections where p.CollectionId == c.Id select c.CollectionName)
                      let brand = (from b in _context.Brands where p.BrandId == b.Id select b.BrandName)
                      let stock = (from s in _context.Stock where p.StockId == s.Id select s.ProductQuantity)
                      select new Complete_Product() { Product = p, Images = image, Type = type, Category = category, Collection = collection, Brand = brand, Stock = stock };


            }
            if (ProductColor.Length != 0)
            {
                result = result.Where(m => ProductColor.Contains(m.ProductColor));
                res = from p in result
                      let image = (from i in _context.ProductImages where p.Id == i.ProductId select i.ImageURL).ToArray()
                      let type = (from t in _context.Types where p._TypeId == t.Id select t._TypeName)
                      let category = (from cat in _context.Categories where p.CategoryId == cat.Id select cat.CategoryName)
                      let collection = (from c in _context.Collections where p.CollectionId == c.Id select c.CollectionName)
                      let brand = (from b in _context.Brands where p.BrandId == b.Id select b.BrandName)
                      let stock = (from s in _context.Stock where p.StockId == s.Id select s.ProductQuantity)
                      select new Complete_Product() { Product = p, Images = image, Type = type, Category = category, Collection = collection, Brand = brand, Stock = stock };
            }
            if (_TypeId.Length != 0)
            {
                result = result.Where(m => _TypeId.Contains(m._TypeId));
                res = from p in result
                      let image = (from i in _context.ProductImages where p.Id == i.ProductId select i.ImageURL).ToArray()
                      let type = (from t in _context.Types where p._TypeId == t.Id select t._TypeName)
                      let category = (from cat in _context.Categories where p.CategoryId == cat.Id select cat.CategoryName)
                      let collection = (from c in _context.Collections where p.CollectionId == c.Id select c.CollectionName)
                      let brand = (from b in _context.Brands where p.BrandId == b.Id select b.BrandName)
                      let stock = (from s in _context.Stock where p.StockId == s.Id select s.ProductQuantity)
                      select new Complete_Product() { Product = p, Images = image, Type = type, Category = category, Collection = collection, Brand = brand, Stock = stock };
            }
             if (CategoryId.Length != 0)
            {
                result = result.Where(m => CategoryId.Contains(m.CategoryId));
                res = from p in result
                      let image = (from i in _context.ProductImages where p.Id == i.ProductId select i.ImageURL).ToArray()
                      let type = (from t in _context.Types where p._TypeId == t.Id select t._TypeName)
                      let category = (from cat in _context.Categories where p.CategoryId == cat.Id select cat.CategoryName)
                      let collection = (from c in _context.Collections where p.CollectionId == c.Id select c.CollectionName)
                      let brand = (from b in _context.Brands where p.BrandId == b.Id select b.BrandName)
                      let stock = (from s in _context.Stock where p.StockId == s.Id select s.ProductQuantity)
                      select new Complete_Product() { Product = p, Images = image, Type = type, Category = category, Collection = collection, Brand = brand, Stock = stock };
            }
            if (CollectionId.Length != 0)
            {
                 result = result.Where(m => CollectionId.Contains(m.CollectionId));
                res = from p in result
                      let image = (from i in _context.ProductImages where p.Id == i.ProductId select i.ImageURL).ToArray()
                      let type = (from t in _context.Types where p._TypeId == t.Id select t._TypeName)
                      let category = (from cat in _context.Categories where p.CategoryId == cat.Id select cat.CategoryName)
                      let collection = (from c in _context.Collections where p.CollectionId == c.Id select c.CollectionName)
                      let brand = (from b in _context.Brands where p.BrandId == b.Id select b.BrandName)
                      let stock = (from s in _context.Stock where p.StockId == s.Id select s.ProductQuantity)
                      select new Complete_Product() { Product = p, Images = image, Type = type, Category = category, Collection = collection, Brand = brand, Stock = stock };
            }
                        
            int totalitems = res.Count();
            int totalpages = totalitems / page_size;
            //totalpages+1 because the first page is 1 and not 0
            totalpages = totalpages + 1;
            string Error = "No product that fullfill these filters";
            if (res.Count() < 1 | page_index < 1) return Ok(Error);
            //page_index-1 so the first page is 1 and not 0
            page_index = page_index - 1;
            int skip = page_index * page_size;
            res = res.Skip(skip).Take(page_size);
            PaginationPage page = new PaginationPage { totalpages = totalpages, totalitems = totalitems, products = res.ToArray()};
            return Ok(page);
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
            if (product.ProductNumber != null) { p.ProductNumber = product.ProductNumber; }
            if (product.ProductEAN != null) { p.ProductEAN = product.ProductEAN; }
            if (product.ProductInfo != null) { p.ProductInfo = product.ProductInfo; }
            if (product.ProductDescription != null) { p.ProductDescription = product.ProductDescription; }
            if (product.ProductSpecification != null) { p.ProductSpecification = product.ProductSpecification; }
            if (product.ProductPrice != 0) { p.ProductPrice = product.ProductPrice; }
            if (product.ProductColor != null) { p.ProductColor = product.ProductColor; }
            if (product._TypeId != 0) { p._TypeId = product._TypeId; }
            if (product.CategoryId != 0) { p.CategoryId = product.CategoryId; }
            if (product.CollectionId != 0) { p.CollectionId = product.CollectionId; }
            if (product.BrandId != 0) { p.BrandId = product.BrandId; }
            if (product.StockId != 0) { p.StockId = product.StockId; }
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

        [HttpGet("search/{page_index}/{page_size}/{searchstring}")]
        public IActionResult Search(int page_index, int page_size, string searchstring)
        {
            var res = (from p in _context.Products
                       where p.ProductName.Contains(searchstring) | p.ProductNumber.Contains(searchstring) | p.Brand.BrandName.Contains(searchstring)
                       orderby p.Id
                       let images =
(from i in _context.ProductImages where p.Id == i.ProductId select i.ImageURL).ToArray()
                       let type = (from t in _context.Types where p._TypeId == t.Id select t._TypeName)
                       let category = (from cat in _context.Categories where p.CategoryId == cat.Id select cat.CategoryName)
                       let collection = (from c in _context.Collections where p.CollectionId == c.Id select c.CollectionName)
                       let brand = (from b in _context.Brands where p.BrandId == b.Id select b.BrandName)
                       let stock = (from s in _context.Stock where p.StockId == s.Id select s.ProductQuantity)
                       select new Complete_Product() { Product = p, Images = images, Type = type, Category = category, Collection = collection, Brand = brand, Stock = stock }).ToArray();

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
    }

}