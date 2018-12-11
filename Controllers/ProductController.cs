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

        public class PaginationPage{
            public int totalpages {get;set;}
            public int totalitems {get;set;}
            public Complete_Product[] products {get;set;}
        }

        public class SearchProduct{
            public int totalitems{get;set;}
            public IOrderedQueryable products {get;set;}
        }

        // public class FilterV
        // {
        //     public string filter1 {get;set;}
        //     public string filter2 {get;set;}
        //     public string filter3 {get;set;}
        //     public string filter4 {get;set;}
        //     public string filter5 {get;set;}
        // }

        public class Filter{
            public int kind {get;set;}
            public string att{get;set;}
            public object value{get;set;}
            public Filter a1 {get;set;}
            public Filter a2 {get;set;}
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
            //Get a list of all products with all related info from other tables, ordered by id
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
            //totalpages+1 because the first page is 1 and not 0
            totalpages = totalpages+1;
            string Error = "Error";
            if (res.Count() < 1 | page_index < 1) return Ok(Error);
            //page_index-1 so the first page is 1 and not 0
            page_index = page_index-1;
            int skip = page_index * page_size;
            res = res.Skip(skip).Take(page_size).ToArray();
            PaginationPage page = new PaginationPage {totalpages = totalpages, totalitems = totalitems, products = res};
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

        [HttpGet("search/{page_index}/{page_size}/{searchstring}")]
        public IActionResult Search(int page_index, int page_size, string searchstring)
        {
            var res = (from p in _context.Products where p.ProductName.Contains(searchstring) | p.ProductNumber.Contains(searchstring) | p.Brand.BrandName.Contains(searchstring) orderby p.Id let images = 
            (from i in _context.ProductImages where p.Id == i.ProductId select i.ImageURL).ToArray()
            let type = (from t in _context.Types where p._TypeId == t.Id select t._TypeName)
            let category = (from cat in _context.Categories where p.CategoryId == cat.Id select cat.CategoryName)
            let collection = (from c in _context.Collections where p.CollectionId == c.Id select c.CollectionName)
            let brand = (from b in _context.Brands where p.BrandId == b.Id select b.BrandName)
            let stock = (from s in _context.Stock where p.StockId == s.Id select s.ProductQuantity)
            select new Complete_Product(){Product = p, Images = images, Type = type, Category = category, Collection = collection, Brand = brand, Stock = stock}).ToArray();

             int totalitems = res.Count();
            int totalpages = totalitems / page_size;
            //totalpages+1 because the first page is 1 and not 0
            totalpages = totalpages+1;
            string Error = "Error";
            if (res.Count() < 1 | page_index < 1) return Ok(Error);
            //page_index-1 so the first page is 1 and not 0
            page_index = page_index-1;
            int skip = page_index * page_size;
            res = res.Skip(skip).Take(page_size).ToArray();
            PaginationPage page = new PaginationPage {totalpages = totalpages, totalitems = totalitems, products = res};
            return Ok(page);
        }

//         [HttpGet("filter")]
// public IActionResult GetFilter([FromBody] Filter f_p)
// {
//     if(f_p == null){
//         return NotFound();
//     }
//     Expression<Func<Product,bool>> e_p = FilterToExpr<Product>(f_p);
//     return Ok(_context.Products.Where(e_p).ToArray());
// }

// Expression<Func<T,bool>> FilterToExpr<T>(Filter f){
//     var type = typeof(T);
//     var parameter = Expression.Parameter(type, "P");
//     return FilterToExpr_AUX<T>(f, parameter);
// }
// Expression<Func<T,bool>> FilterToExpr_AUX<T>(Filter f, ParameterExpression parameter){
//     switch(f.kind){
//         case 0:{
//             var propertyReference = Expression.Property(parameter, f.att);
//             var constantReference = Expression.Constant(f.value);
//             return Expression.Lambda<Func<T,bool>>(Expression.Equal(propertyReference, constantReference), parameter);
//         }
//         case 1:{
//             Expression<Func<T,bool>> expr1 = FilterToExpr_AUX<T>(f.a1, parameter);
//             Expression<Func<T,bool>> expr2 = FilterToExpr_AUX<T>(f.a2, parameter);
//             var body = Expression.And(expr1.Body, expr2.Body);
//             var Lambda = Expression.Lambda<Func<T,bool>>(body, parameter);
//             return Lambda;
//         }
//         default: return null;
//     }
// }
// }
        

        // [HttpGet("filter/{page_index}/{page_size}")]
        // public IActionResult Filtering(int page_index, int page_size, dynamic filter)
        // {
        //     dynamic FilterString = JsonConvert.DeserializeObject(filter.ToString());
            
        //     Filter filterclass = new Filter{filter1 = FilterString.filter1, filter2 = FilterString.filter2, filter3 = FilterString.filter3, filter4 = FilterString.filter4, filter5 = FilterString.filter5};
        //     var filter_brandname = (from b in _context.Brands where b.BrandName == filterclass.filter1 select b);
        //     var filter_collectiename = (from col in _context.Collections where col.CollectionName == filterclass.filter2 select col);
        //     var filter_type = (from type in _context.Types where type._TypeName == filterclass.filter3 select type);
        //     var filter_category = (from cat in _context.Categories where cat.CategoryName == filterclass.filter4 select cat); 
        //     var res = (from p in _context.Products from bn in filter_brandname from cn in filter_collectiename from tn in filter_type from catn in filter_category where (p._TypeId == tn.Id) && (p.CategoryId == catn.Id) && (p.BrandId == bn.Id) && (p.CollectionId == cn.Id) && (p.ProductColor == filterclass.filter5) orderby p.Id let images = 
        //     (from i in _context.ProductImages where p.Id == i.ProductId select i.ImageURL).ToArray()
        //     let type = (from t in _context.Types where p._TypeId == t.Id select t._TypeName)
        //     let category = (from cat in _context.Categories where p.CategoryId == cat.Id select cat.CategoryName)
        //     let collection = (from c in _context.Collections where p.CollectionId == c.Id select c.CollectionName)
        //     let brand = (from b in _context.Brands where p.BrandId == b.Id select b.BrandName)
        //     let stock = (from s in _context.Stock where p.StockId == s.Id select s.ProductQuantity)
        //     select new Complete_Product(){Product = p, Images = images, Type = type, Category = category, Collection = collection, Brand = brand, Stock = stock}).ToArray();

        //     int totalitems = res.Count();
        //     int totalpages = totalitems / page_size;
        //     //totalpages+1 because the first page is 1 and not 0
        //     totalpages = totalpages+1;
        //     string Error = "Error";
        //     if (res.Count() < 1 | page_index < 1) return Ok(Error);
        //     //page_index-1 so the first page is 1 and not 0
        //     page_index = page_index-1;
        //     int skip = page_index * page_size;
        //     res = res.Skip(skip).Take(page_size).ToArray();
        //     PaginationPage page = new PaginationPage {totalpages = totalpages, totalitems = totalitems, products = res};
        //     return Ok(page);
        // }
        
        [HttpGet("filter")]
               public IActionResult Filters(Dictionary<string,string> filters)
        {
            //  foreach (var item in filterarray){
            //     OrderProduct product = new OrderProduct(){
            //         OrderId = Order.Id, 
            //         ProductId = item};
            //     _context.OrderProduct.Add(product);}
                             
            // Console.WriteLine($"HERE {filters}");

        //      foreach( KeyValuePair<string, string> kvp in filters)
        // {
        //     Console.WriteLine("HERE Key = {0}, Value = {1}", 
        //         kvp.Key, kvp.Value);
            
        // }

        // IOrderedQueryable x = null;
       Object[] x= new Object[]{};
        //IQueryable<int> x = null;
        //Product[] x = new Product[]{};
                List<object> List = new List<object>{};
        List<object> Values = new List<object>{};
        List<string> Keys = new List<string>{};
        
        foreach (KeyValuePair<string,string> item in filters)
        {
            
            Values.Add(item.Value);
            Keys.Add(item.Key);
            Console.WriteLine($"HERE {item.Key} & {item.Value}");
           x = (from p in _context.Products.Where(f => f.GetProperty(item.Key) == item.Value) select p).ToArray();
            List.Add(x);
            //return Ok(x);
        }
// foreach (string key in Keys){
//     foreach (object value in Values){
//               x = (from p in _context.Products.Where(f => f.GetProperty(key) == value) select p);  
//               Console.WriteLine($"HERE {key} & {value}");}
//             }
        // foreach (object item in Keys)
        // {
           
        // }
        
        // foreach (KeyValuePair<string,object> item in filters)
        // {
        // //    if (item.Key == "BrandId"){
        // //      var res = from p in _context.Products where p.BrandId == item.Value orderby p select p.BrandId;

        //     //  var x = res.Where(f => f.GetProperty("BrandId") == item.Value);
        //     //  List.Add(res);}
              
        //     //  var res = from p in _context.Products where p.CollectionId == item.Value orderby p select p;
        //    x = (from p in _context.Products.Where(f => f.GetProperty("BrandId") == item.Value) select p).ToArray();
        //     //  var x = res.Where(f => f.GetProperty("BrandId") == item.Value);
        //      List.Add(x);


          
        // }
         //var filter = filters.Keys;
        //   Console.WriteLine($"HERE {filters.Keys}");
       // Console.WriteLine($"HERE {x}");
            return Ok(x);

            
        }

        
    }
  public static class MyExtensions
{
    public static string GetProperty<T>(this T obj, string name)
    {
        Type t = typeof(T);
        return t.GetProperty(name).GetValue(obj, null).ToString();
    } 
 
}
    
} 