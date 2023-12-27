using Data.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Dto
{
    //این کلاس فیلد جزئیات دسته بندی را هم دارد و برای نمایش اطلاعات کامل محصول استفاده می شود.
    public class ProductDtoGet
    {
        //[Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public int Stock { get; set; }
        public int CategoryId { get; set; }
        public CategoryDto Category { get; set; }
        public List<ImagesDto> Images { get; set; }
    }
}
