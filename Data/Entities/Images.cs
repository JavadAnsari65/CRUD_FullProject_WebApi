using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Entities
{
    public class Images
    {
        [Key]
        public int Id { get; set; }
        public string Caption{ get; set; }
        public string Image { get; set; }

        [ForeignKey("ProductId")]
        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
}
