using AutoMapper;
using Data.Dto;
using Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Mapper
{
    public class AppMapperProfile:Profile
    {
        public AppMapperProfile()
        {
            // Product
            CreateMap<ProductDtoPost, Product>().ReverseMap();
            CreateMap<ProductDtoGet, Product>().ReverseMap();

            // Category
            CreateMap<CategoryDto, Category>().ReverseMap();

            // Images
            CreateMap<ImagesDto, Images>().ReverseMap();
        }
    }
}
