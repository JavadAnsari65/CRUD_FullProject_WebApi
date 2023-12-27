using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Entities
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        [Display(Name = "نام محصول")]
        public string Name { get; set; }
        [Display(Name = "توضیحات")]
        public string Description { get; set; }
        [Display(Name = "قیمت")]
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public bool IsApproved { get; set; }=false;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public DateTime UpdateDate { get; set; }
        public int Stock {  get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public List<Images> Images { get; set; }
    }
}
