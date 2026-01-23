using AutoMapper;
using E_Commerce.DTOs.CategoryDTO;
using E_Commerce.DTOs.ProductDTO;
using E_Commerce.DTOs.ReviewDTO;
using E_Commerce.Models;

namespace E_Commerce.Mapper
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            //Product Mappings
            CreateMap<Product, ProductDto>().ReverseMap();
            CreateMap<Product, CreateProductDto>().ReverseMap();
            CreateMap<Product, UpdateProductDto>().ReverseMap();



            //Category Mappings
            CreateMap<Category, CategoryDto>().ReverseMap();
            CreateMap<Category, CreateCategoryDto>().ReverseMap();
            CreateMap<Category, UpdateCategoryDto>().ReverseMap();


            //Review Mappings
            CreateMap<Review, ReviewDto>().ReverseMap();
            CreateMap<Review, CreateReviewDto>().ReverseMap();
            CreateMap<Review, UpdateReviewDto>().ReverseMap();


        }
    }

}
