using Data.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRUD_FullProject_WebApi.Controllers
{
    [Route("[action]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ProductDbContext _context;

        public CategoryController(ProductDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetCats()
        {
            var cats =  await _context.Categories.Include(c=>c.Products).ToListAsync();
            return Ok(cats);
        }
    }
}
