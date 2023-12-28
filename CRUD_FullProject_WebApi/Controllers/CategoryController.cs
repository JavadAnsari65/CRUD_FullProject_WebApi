using AutoMapper;
using Data.Context;
using Data.Dto;
using Data.Entities;
using Data.Mapper;
using Data.SearchClasses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CRUD_FullProject_WebApi.Controllers
{
    [Route("[action]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ProductDbContext _context;
        private readonly IMapper _mapper;

        public CategoryController(ProductDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetCategoriesWithProducts()
        {
            var cats =  await _context.Categories.Include(c=>c.Products).ToListAsync();
            return Ok(cats);
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var cats = await _context.Categories.ToListAsync();
            var catsDto = _mapper.Map<List<CategoryDto>>(cats); //<List> حتما باید باشد وگرنه خطای کانفیگ مپ می دهد
            return Ok(catsDto);
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory(CategoryDto categoryDto)
        {
            if (categoryDto == null)
            {
                return BadRequest("Invalid Data!");
            }
            var newCat = _mapper.Map<Category>(categoryDto);
            _context.Categories.AddAsync(newCat);
            await _context.SaveChangesAsync();
            return Ok(newCat);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateCategory(int id, CategoryDto categoryDto)
        {
            if (categoryDto.Id != id)
            {
                return BadRequest($"The Id field for Category incorrect!. Please Do Id = {id}");
            }
            
            var updateCat = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if (updateCat == null)
            {
                return NotFound($"The Category with Id = {id} Not Found!");
            }
            else
            {
                _mapper.Map(categoryDto, updateCat);
                await _context.SaveChangesAsync();
                return Ok($"Category with Id {id} Updated.");
            }

        }

        [HttpDelete]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var delCat = await _context.Categories.FirstOrDefaultAsync(cat => cat.Id == id);
            if(delCat == null)
            {
                return NotFound($"The Category with Id={id} Not Found!");
            }
            else
            {
                _context.Categories.Remove(delCat);
                await _context.SaveChangesAsync();
                return Ok($"The Category with Id={id} Deleted!");
            }
        }

        [HttpGet]
        public async Task<IActionResult> SearchCategory(string fieldName, string searchValue)
        {
            var query = _context.Categories.AsQueryable();

            var filterService = new EntityFilterService<Category>(query);

            var parameter = Expression.Parameter(typeof(Category), "x");
            var property = Expression.Property(parameter, fieldName);
            object convertedValue = Convert.ChangeType(searchValue, property.Type);
            var constant = Expression.Constant(convertedValue);
            var equals = Expression.Equal(property, constant);
            var lambada = Expression.Lambda<Func<Category, bool>>(equals, parameter);

            query = filterService.ApplyFilter(lambada);

            var filteredEntities = await query.ToListAsync();
            var catDto = _mapper.Map<List<CategoryDto>>(filteredEntities);
            return Ok(catDto);
        }
    }
}
