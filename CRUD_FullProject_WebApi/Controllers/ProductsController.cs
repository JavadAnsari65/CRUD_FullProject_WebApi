using AutoMapper;
using Data.Context;
using Data.Dto;
using Data.Entities;
using Data.SearchClasses;
using wwwroot.StaticFiles;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel;

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
                foreach (var image in product.Images)
                {
                    image.Image = StaticUrls.ProductImagesUrl + image.Image;
                }
            }

            var productDtos = _mapper.Map<List<ProductDtoGet>>(products);

            //برای محاسبه درست تعداد صفحات
            int totalPage = 0;
            if (totalItems % pageSize == 0)
            {
                totalPage = totalItems / pageSize;
            }
            else
            {
                totalPage = totalItems / pageSize + 1;
            }

            //برای نمایش محصولات به همراه اطلاعات صفحه بندی در انتهای آن
            return Ok(new { Products = productDtos, TotalItems = totalItems, PageSize = pageSize, TotalPages = totalPage, PageNumber = pageNumber });

        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDtoGet>>> GetProducts()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .ToListAsync();

            foreach (var product in products)
            {
                product.ImageUrl = StaticUrls.ProductImagesUrl + product.ImageUrl;

                foreach (var image in product.Images)
                {
                    image.Image = StaticUrls.ProductImagesUrl + image.Image;
                }
            }

            var productDtos = _mapper.Map<List<ProductDtoGet>>(products);
            return Ok(productDtos);
        }

        [HttpGet]
        public async Task<ActionResult<ProductDtoGet>> GetProductById(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            product.ImageUrl = StaticUrls.ProductImagesUrl + product.ImageUrl;
            foreach (var image in product.Images)
            {
                image.Image = StaticUrls.ProductImagesUrl + image.Image;
            }

            var productDto = _mapper.Map<ProductDtoGet>(product);
            return Ok(productDto);
        }

        [HttpPost]
        public async Task<ActionResult<ProductDtoPost>> AddProduct(ProductDtoPost productDto)
        {
            //upload Image base64 to server
            var base64 = productDto.ImageUrl.Split(',')[1];
            //تیدیل رشته بیس64 به رشته بایت جهت تبدیل به عکس
            var bytes = System.Convert.FromBase64String(base64);
            //تولید یک نام تصادفی برای عکس
            var randName = Guid.NewGuid().ToString();
            //مقدار دهی فیلد عکس با نام تولید شده و پسوند موردنظر
            productDto.ImageUrl = randName + ".png";
            //ادغام مسیر و نام عکس موردنظر : ساخت مسیر نهایی
            var path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", StaticUrls.ProductImagesUrl + randName + ".png");                                                                                                                                                 
            //var path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot","StaticFiles", "Images", "Products", randName + ".png");
            //ساخت و ذخیره عکس در مسیر موردنظر
            System.IO.File.WriteAllBytes(path, bytes);


            // بارگیری و ذخیره تصاویر بیشتر در جدول Images 
            var additionalImages = new List<Images>();
            for (int i = 0; i < productDto.Images.Count; i++)
            {
                var base64Ex = productDto.Images[i].Image.Split(',')[1];
                var bytesEx = System.Convert.FromBase64String(base64Ex);
                var randNameEx = Guid.NewGuid().ToString();
                var imageUrlEx = randNameEx + ".png";
                var pathEx = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", StaticUrls.ProductImagesUrl + imageUrlEx);
                System.IO.File.WriteAllBytes(pathEx, bytesEx);

                // اضافه کردن تصاویر به لیست
                additionalImages.Add(new Images
                {
                    Caption = productDto.Images[i].Caption,
                    Image = imageUrlEx
                });
            }


            var product = _mapper.Map<Product>(productDto);

            // افزودن تصاویر بیشتر به محصول
            product.Images = additionalImages;

            //اینجا میتوانیم فیلدهایی از محصول که در productDto نیست را مقداردهی کنیم

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            //return CreatedAtAction("GetProductById", new { id = product.Id }, _mapper.Map<ProductDto>(product));
            return Ok(product);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProduct(int id, ProductDtoPost productDto)
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
                foreach (var image in product.Images)
                {
                    image.Image = StaticUrls.ProductImagesUrl + image.Image;
                }
            }


            var productDtos = _mapper.Map<List<ProductDtoGet>>(products);

            //برای محاسبه درست تعداد صفحات
            int totalPage = 0;
            if (totalItems % pageSize == 0)
            {
                totalPage = totalItems / pageSize;
            }
            else
            {
                totalPage = totalItems / pageSize + 1;
            }

            //برای نمایش محصولات به همراه اطلاعات صفحه بندی در انتهای آن
            return Ok(new { Products = productDtos, TotalItems = totalItems, PageSize = pageSize, TotalPages = totalPage, PageNumber = pageNumber });

            /*
            //برای ساخت یک شی جهت نمایش اطلاعات صفحه بندی در هدر
            var paginationHeader = new
            {
                totalItems,
                pageSize = pageSize,
                pageNumber = pageNumber,
                totalPages = totalItems / pageSize + 1,
                //Text = "Welcome Javad"
            };
            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationHeader));
            return Ok(productDtos);
            */
        }

        //[HttpGet("FilterEntities/{fieldName}/{searchValue}")]
        [HttpGet]
        public async Task<IActionResult> FilterEntities(string fieldName, string searchValue)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .AsQueryable();

            var filterService = new EntityFilterService<Product>(query);

            var parameter = Expression.Parameter(typeof(Product), "x");
            var property = Expression.Property(parameter, fieldName);

            // تبدیل مقدار به نوع مورد نظر (این مثال برای عدد است)
            object convertedValue = Convert.ChangeType(searchValue, property.Type);

            var constant = Expression.Constant(convertedValue);
            var equals = Expression.Equal(property, constant);
            var lambda = Expression.Lambda<Func<Product, bool>>(equals, parameter);

            query = filterService.ApplyFilter(lambda);

            var filteredEntities = await query.ToListAsync();
            foreach (var product in filteredEntities)
            {
                product.ImageUrl = StaticUrls.ProductImagesUrl + product.ImageUrl;
                foreach (var image in product.Images)
                {
                    image.Image = StaticUrls.ProductImagesUrl + image.Image;
                }
            }

            var productDtos = _mapper.Map<List<ProductDtoGet>>(filteredEntities);

            // باقی‌مانده کد

            return Ok(productDtos);
        }





    }
}
