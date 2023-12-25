using AutoMapper;
using Data.Context;
using Data.Dto;
using Data.Entities;
using wwwroot.StaticFiles;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace CRUD_FullProject_WebApi.Controllers
{
    [Route("[action]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ProductDbContext _context;
        private readonly IMapper _mapper;
        public ProductsController(ProductDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetProductsWithPagination(int pageNumber = 1, int pageSize = 5)
        {
            var totalItems = await _context.Products.CountAsync();

            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .OrderBy(p => p.Id) // یا می‌توانید بر اساس فیلد دیگری مانند CreateDate یا یک فیلد دلخواه دیگر مرتب‌سازی کنید.
                .Skip((pageNumber - 1) * pageSize)   // از ایتدای رکوردها به تعداد Skip رکورد صرفنظر کن برو جلو
                .Take(pageSize)     // از جایی که اشاره گر رکوردها هست به تعداد Take رکورد رو برگردون
                .ToListAsync();

            foreach (var product in products)
            {
                product.ImageUrl = StaticUrls.ProductImagesUrl + product.ImageUrl;
            }

            var productDtos = _mapper.Map<List<ProductDto>>(products);
            
            var paginationHeader = new
            {
                totalItems,
                pageSize,
                pageNumber,
                totalPages = (int)Math.Ceiling((double)totalItems / pageSize)
            };

            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationHeader));

            return Ok(productDtos);
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .ToListAsync();

            foreach (var product in products)
            {
                product.ImageUrl = StaticUrls.ProductImagesUrl + product.ImageUrl;
            }

            var productDtos = _mapper.Map<List<ProductDto>>(products);
            return Ok(productDtos);
        }

        [HttpGet]
        public async Task<ActionResult<ProductDto>> GetProductById(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            product.ImageUrl = StaticUrls.ProductImagesUrl + product.ImageUrl;

            if (product == null)
            {
                return NotFound();
            }

            var productDto = _mapper.Map<ProductDto>(product);
            return Ok(productDto);
        }

        [HttpPost]
        public async Task<ActionResult<ProductDto>> AddProduct(ProductDto productDto)
        {
            //upload Image base64 to server
            var base64 = productDto.ImageUrl.Split(',')[1];
            var bytes = System.Convert.FromBase64String(base64);    //تیدیل رشته بیس64 به رشته بایت جهت تبدیل به عکس
            var randName = Guid.NewGuid().ToString();   //تولید نام تصادفی برای عکس
            productDto.ImageUrl = randName + ".png";    //مقدار دهی فیلد عکس با نام تولید شده و پسوند موردنظر
            var path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", StaticUrls.ProductImagesUrl + randName + ".png");    //ادغام مسیر و نام عکس موردنظر : ساخت مسیر نهایی
            //var path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot","StaticFiles", "Images", "Products", randName + ".png");
            System.IO.File.WriteAllBytes(path, bytes);  //ذخیره عکس در مسیر موردنظر

            var product = _mapper.Map<Product>(productDto);
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            //return CreatedAtAction("GetProductById", new { id = product.Id }, _mapper.Map<ProductDto>(product));
            return Ok(product);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProduct(int id, ProductDto productDto)
        {

            if (id != productDto.Id)
            {
                return BadRequest();
            }

            var existingProduct = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (existingProduct == null)
            {
                return NotFound();
            }

            _mapper.Map(productDto, existingProduct);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok($"Product with Id {id} Updated.");
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return Ok($"Product with Id {id} has Deleted.");
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }

        [HttpGet]
        public async Task<IActionResult> SearchProducts([FromQuery] ProductSearchModel searchModel, int pageNumber = 1, int pageSize = 5)
        {

            // ساخت یک کوئری پایه برای استخراج محصولات
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .AsQueryable();

            // در شرط های زیر به ازای هر فیلد فرم جستجو که مقدار داشته باشد یک شرط به کوئری پایه مان اضافه می شود

            if (!string.IsNullOrEmpty(searchModel.Name))
            {
                query = query.Where(p => p.Name.Contains(searchModel.Name));
            }

            if (!string.IsNullOrEmpty(searchModel.Description))
            {
                query = query.Where(p => p.Description.Contains(searchModel.Description));
            }

            if (searchModel.Price.HasValue)
            {
                query = query.Where(p => p.Price == searchModel.Price);
            }

            if (searchModel.IsApproved.HasValue)
            {
                query = query.Where(p => p.IsApproved == searchModel.IsApproved);
            }

            if (searchModel.IsDeleted.HasValue)
            {
                query = query.Where(p => p.IsDeleted == searchModel.IsDeleted);
            }

            if (searchModel.CreateDate.HasValue)
            {
                query = query.Where(p => p.CreateDate == searchModel.CreateDate);
            }

            if (searchModel.UpdateDate.HasValue)
            {
                query = query.Where(p => p.UpdateDate == searchModel.UpdateDate);
            }

            if (searchModel.Stock.HasValue)
            {
                query = query.Where(p => p.Stock == searchModel.Stock);
            }

            if (searchModel.CategoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == searchModel.CategoryId);
            }

            // این دستور براساس کوئری ایجاد شده رکوردها را استخراج و تعداد آنها را برمی گرداند
            var totalItems = await query.CountAsync();

            // این دستور بر اساس کوئری ایجاد شده رکوردها را استحراج و براساس فیلد ]ی دی مرتب کرده و برمی گرداند
            var products = await query
                .OrderBy(p => p.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            foreach (var product in products)
            {
                product.ImageUrl = StaticUrls.ProductImagesUrl + product.ImageUrl;
            }

            // برای اینکه دوبار عملیات استخراج رکوردها از دیتابیس انجام نشود برای شمارش تعداد رکوردها به جای دستور قبلی
            //  از این دستور استفاده می کنیم ولی در بخاطر صفجه بندی در شمارش تعداد کل رکوردها به مشکل میخوریم
            //var totalItems = products.Count();

            var productDtos = _mapper.Map<List<ProductDto>>(products);

            var paginationHeader = new
            {
                totalItems,
                pageSize = pageSize,
                pageNumber = pageNumber,
                totalPages = totalItems/pageSize+1,
                //Text = "Welcome Javad"
            };

            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationHeader));

            return Ok(productDtos);
        }


    }
}
