using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Backend_Website.Models;
using Newtonsoft.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;

namespace Backend_Website.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class ProductController : Controller
    {
        private readonly WebshopContext _context;
        private readonly ClaimsPrincipal _caller;

        public ProductController(WebshopContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _caller = httpContextAccessor.HttpContext.User;
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

        public class FiltersPage
        {
            public List<dynamic> FiltersList {get;set;}
            public PaginationPage page {get;set;}
            
        }

        public class SearchProduct
        {
            public int totalitems { get; set; }
            public IOrderedQueryable products { get; set; }
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

        [Authorize(Policy = "_IsAdmin")]
        [HttpPost("create")]
        public void CreateProduct(dynamic ProductDetails)
        {
            dynamic ProductDetailsJSON = JsonConvert.DeserializeObject(ProductDetails.ToString());

            int _categoryId;
            int _brandId;
            int _collectionId;
            int _typeid;

            ///////////////CATEGORY////////////////
            dynamic c = 1;
            if (ProductDetailsJSON.CategoryId != null)
            {
                int category = ProductDetailsJSON.CategoryId;
                c = _context.Categories.Find(category);
            }
            if (c == null || ProductDetailsJSON.CategoryId == null)
            {
                Category _Category = new Category()
                {
                    Id = _context.Categories.Select(a => a.Id).Max() + 1,
                    CategoryName = ProductDetailsJSON.CategoryName
                };
                _context.Categories.Add(_Category);
                _categoryId = _Category.Id;
                //_context.SaveChanges();
            }
            else
            {
                _categoryId = ProductDetailsJSON.CategoryId;
            }

            ///////////////TYPE////////////////
            dynamic t = 1;
            if (ProductDetailsJSON.TypeId != null)
            {
                int type = ProductDetailsJSON.TypeId;
                t = _context.Types.Find(type);
            }
            if (t == null || ProductDetailsJSON.TypeId == null)
            {
                _Type _Type = new _Type()
                {
                    Id = _context.Types.Select(a => a.Id).Max() + 1,
                    _TypeName = ProductDetailsJSON.TypeName
                };
                _context.Types.Add(_Type);
                _typeid = _Type.Id;

                Category_Type CT = new Category_Type()
                {
                    CategoryId = _categoryId,
                    _TypeId = _typeid

                };
                _context.CategoryType.Add(CT);
            }
            else
            {
                _typeid = ProductDetailsJSON.TypeId;
            }

            ///////////////BRAND////////////////
            dynamic b = 1;
            if (ProductDetailsJSON.BrandId != null)
            {
                int brand = ProductDetailsJSON.BrandId;
                b = _context.Brands.Find(brand);
            }
            if (b == null || ProductDetailsJSON.BrandId == null)
            {
                Brand Brand = new Brand()
                {
                    Id = _context.Brands.Select(a => a.Id).Max() + 1,
                    BrandName = ProductDetailsJSON.BrandName
                };
                _context.Brands.Add(Brand);
                _brandId = Brand.Id;
                //_context.SaveChanges();
            }
            else
            {
                _brandId = ProductDetailsJSON.BrandId;
            }
            
            ///////////////Collection////////////////
            dynamic co = 1;
            if (ProductDetailsJSON.CollectionId != null)
            {
                int coll = ProductDetailsJSON.CollectionId;
                co = _context.Collections.Find(coll);
            }
            if (co == null || ProductDetailsJSON.CollectionId == null)
            {
                Collection Collection = new Collection()
                {
                    Id = _context.Collections.Select(a => a.Id).Max() + 1,
                    BrandId = _brandId,
                    CollectionName = ProductDetailsJSON.CollectionName
                };
                _context.Collections.Add(Collection);
                _collectionId = Collection.Id;
            }
            else
            {
                _collectionId = ProductDetailsJSON.CollectionId;
            }

            ///////////////STOCK////////////////
            Stock Stock = new Stock()
            {
                //Id = ProductDetailsJSON.StockId,
                Id = _context.Stock.Select(a => a.Id).Max() + 1,
                ProductQuantity = ProductDetailsJSON.Stock
            };
            _context.Stock.Add(Stock);

            ///////////////PRODUCT////////////////
            Product Product = new Product()
            {
                ProductName = ProductDetailsJSON.ProductName,
                _TypeId = _typeid,//ProductDetailsJSON.TypeId,
                CategoryId = _categoryId,  //ProductDetailsJSON.CategoryId,
                CollectionId = _collectionId,//ProductDetailsJSON.CollectionId,
                BrandId = _brandId, //ProductDetailsJSON.BrandId,
                StockId = Stock.Id, //ProductDetailsJSON.StockId,
                Id = _context.Products.Select(a => a.Id).Max() + 1,
                ProductNumber = ProductDetailsJSON.ProductNumber,
                ProductEAN = ProductDetailsJSON.ProductEAN,
                ProductInfo = ProductDetailsJSON.ProductInfo,
                ProductDescription = ProductDetailsJSON.ProductDescription,
                ProductSpecification = ProductDetailsJSON.ProductSpecification,
                ProductPrice = ProductDetailsJSON.ProductPrice,
                ProductColor = ProductDetailsJSON.ProductColor,
            };
            _context.Products.Add(Product);

            ///////////////IMAGE////////////////
            // ProductImage ProductImage = new ProductImage()
            // {
            //     ProductId = Product.Id,
            //     ImageURL = ProductDetailsJSON.ImageURL,
            //     //Id = ProductDetailsJSON.ImageId
            //     Id = _context.ProductImages.Select(a => a.Id).Max() + 1,
            // };
            // _context.ProductImages.Add(ProductImage);

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
            
            List<dynamic> FiltersList = new List<dynamic>();
            foreach (string item in ProductColor){
            var LProductColor = (from p in _context.Products where p.ProductColor == item group p by p.ProductColor into Color select new {Productcolor = Color.First().ProductColor});
            FiltersList.Add(LProductColor);}
            foreach (int item in BrandId){
            var LBrandName = from b in _context.Brands where b.Id == item group b by b.BrandName into BrandName select new {Brandname = BrandName.First().BrandName};
            FiltersList.Add(LBrandName);}
            foreach (int item in _TypeId){
            var LTypeName = from t in _context.Types where t.Id == item group t by t._TypeName into TypeName select new {Typename = TypeName.First()._TypeName};
            FiltersList.Add(LTypeName);}
            foreach (int item in CategoryId){
            var LCategoryName = from cat in _context.Categories where cat.Id == item group cat by cat.CategoryName into CategoryName select new {Categoryname = CategoryName.First().CategoryName};
            FiltersList.Add(LCategoryName);}
            foreach (int item in CollectionId){
            var LCollectionName = from c in _context.Collections where c.Id == item group c by c.CollectionName into CollectionName select new {Collectionname = CollectionName.First().CollectionName};
            FiltersList.Add(LCollectionName);}
            
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
            // string Error = "No product that fullfill these filters";
            // if (res.Count() < 1 | page_index < 1) return Ok(Error);
            //page_index-1 so the first page is 1 and not 0
            page_index = page_index - 1;
            int skip = page_index * page_size;
            res = res.Skip(skip).Take(page_size);
            PaginationPage page = new PaginationPage {totalpages = totalpages, totalitems = totalitems, products = res.ToArray() };
            FiltersPage filterpage = new FiltersPage {FiltersList = FiltersList, page = page};
            return Ok(filterpage);
        }

        // PUT api/product/5
        [Authorize(Policy = "_IsAdmin")]
        [HttpPut("{id}")]
         public void UpdateProduct(dynamic U_product, int id)
        {
            dynamic U_product_JSON = JsonConvert.DeserializeObject(U_product.ToString());
            Product p = _context.Products.Find(id);
            if (U_product_JSON.ProductName != null) { p.ProductName = U_product_JSON.ProductName; }
            if (U_product_JSON.ProductEAN != null) { p.ProductEAN = U_product_JSON.ProductEAN; }
            if (U_product_JSON.ProductNumber != null) { p.ProductNumber = U_product_JSON.ProductNumber; }
            if (U_product_JSON.ProductInfo != null) { p.ProductInfo = U_product_JSON.ProductInfo; }
            if (U_product_JSON.ProductDescription != null) { p.ProductDescription = U_product_JSON.ProductDescription; }
            if (U_product_JSON.ProductSpecification != null) { p.ProductSpecification = U_product_JSON.ProductSpecification; }
            if (U_product_JSON.ProductPrice != null) { p.ProductPrice = U_product_JSON.ProductPrice; }
            if (U_product_JSON.ProductColor != null) { p.ProductColor = U_product_JSON.ProductColor; }
            if (U_product_JSON._TypeId != null) { p._TypeId = U_product_JSON._TypeId; }
            if (U_product_JSON.CategoryId != null) { p.CategoryId = U_product_JSON.CategoryId; }
            if (U_product_JSON.CollectionId != null) { p.CollectionId = U_product_JSON.CollectionId; }
            if (U_product_JSON.BrandId != null) { p.BrandId = U_product_JSON.BrandId; }
            _context.Update(p);
            Stock s = _context.Stock.Find(p.StockId);
            if (U_product_JSON.ProductQuantity != null) { s.ProductQuantity = U_product_JSON.ProductQuantity; }
            _context.Update(s);
            if (U_product_JSON._TypeId != null)
            {
                int TId = U_product_JSON._TypeId;
                _Type t = _context.Types.Find(TId);
                if (U_product_JSON._TypeName != null) { t._TypeName = U_product_JSON._TypeName; }
                _context.Update(t);
            }
            if (U_product_JSON.CollectionId != null)
            {
                int CId = U_product_JSON.CollectionId;
                Collection c = _context.Collections.Find(CId);
                if (U_product_JSON.CollectionName != null) { c.CollectionName = U_product_JSON.CollectionName; }
                _context.Update(c);
            }
            if (U_product_JSON.CategoryId != null)
            {
                int CatId = U_product_JSON.CategoryId;
                Category cat = _context.Categories.Find(CatId);
                if (U_product_JSON.CategoryId != null) { cat.CategoryName = U_product_JSON.CategoryName; }
                _context.Update(cat);
            }
            if (U_product_JSON.BrandId != null)
            {
                int BId = U_product_JSON.BrandId;
                Brand b = _context.Brands.Find(BId);
                if (U_product_JSON.BrandId != null) { b.BrandName = U_product_JSON.BrandName; }
                _context.Update(b);
            }
            int IId = _context.ProductImages.First(a=>a.ProductId == id).Id;
            ProductImage i = _context.ProductImages.Find(IId);
             if (U_product_JSON.ImageURL != null) { i.ImageURL = U_product_JSON.ImageURL; }
             _context.Update(i);
            _context.SaveChanges();
        }

        // DELETE api/product/5
        [Authorize(Policy = "_IsAdmin")]
        [HttpDelete("{id}")]
        public void DeleteProduct(int id)
        {
            //Find all products that has the given id in table Products
            Product Product = _context.Products.Find(id);
            Stock Stock = _context.Stock.Find(id);

            //Delete the found products and save
            _context.Products.Remove(Product);
            _context.Stock.Remove(Stock);
            _context.SaveChanges();
        }

        //GET api/product/stat1
        //Producten die 'out of stock'/uitverkocht zijn
        [Authorize(Policy = "_IsAdmin")]
        [HttpGet("stat1")]
        public IActionResult Stat1()
        {
            var res = (from p in _context.Products from s in _context.Stock where p.StockId == s.Id && s.ProductQuantity < 1 orderby s.ProductQuantity select new {p.Id, p.ProductNumber, p.ProductName, s.ProductQuantity});
            return Ok(res);
        }

        //Aantal users
        [Authorize(Policy = "_IsAdmin")]
        [HttpGet("stat2")]
        public IActionResult Stat2()
        {
            var res = (from u in _context.Users select u).Count();
            return Ok(res);
        }

        //Totale inkomsten
        [Authorize(Policy = "_IsAdmin")]
        [HttpGet("stat3")]
        public IActionResult Stat3()
        {
            var res = (from o in _context.Orders select o.OrderTotalPrice).Sum();
            return Ok(res);
        }

        //De 10 duurste producten uit de shop
        [Authorize(Policy = "_IsAdmin")]
        [HttpGet("stat4")]
        public IActionResult Stat4()
        {
            var res = (from p in _context.Products orderby p.ProductPrice descending select new {p.Id, p.ProductNumber, p.ProductName, Price = p.ProductPrice/100}).Take(10);
            return Ok(res);
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
            //string Error = "Error";
            //if (res.Count() < 1 | page_index < 1) return Ok(Error);
            //page_index-1 so the first page is 1 and not 0
            page_index = page_index - 1;
            int skip = page_index * page_size;
            res = res.Skip(skip).Take(page_size).ToArray();
            PaginationPage page = new PaginationPage { totalpages = totalpages, totalitems = totalitems, products = res };
            return Ok(page);
        }
    }

}