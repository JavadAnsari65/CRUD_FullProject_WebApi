using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Dto
{
    public class ImagesDto
    {
        //[Key]
        public int Id { get; set; }
        public string Caption { get; set; }
        public string Image { get; set; }
        //public int ProductId { get; set; }
    }
}
