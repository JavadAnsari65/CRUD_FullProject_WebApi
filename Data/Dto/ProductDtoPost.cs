using Data.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Dto
{
    //این کلاس فیلد جزئیات دسته بندی را ندارد و برای افزودن و ویرایش محصول استفاده می شود.
    public class ProductDtoPost
    {
        //[Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public int Stock { get; set; }
        public int CategoryId { get; set; }
        public List<ImagesDto> Images { get; set; }

    }
}
