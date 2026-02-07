using AutoMapper;
using E_Commerce.DTOs.CartItemDTO;
using E_Commerce.DTOs.CategoryDTO;
using E_Commerce.DTOs.OrderDTO;
using E_Commerce.DTOs.OrderItemDTO;
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
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
                .ReverseMap();
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

            //CartItem Mappings
            CreateMap<CartItem, CartItemDto>().ReverseMap();
            CreateMap<CartItem, CreateCartItemDto>().ReverseMap();
            CreateMap<CartItem, UpdateCartItemDto>().ReverseMap();

            //OrderItem Mappings
            CreateMap<OrderItem, OrderItemDto>().ReverseMap();
            CreateMap<OrderItem, OrderItemWithProductNameDto>()
            .ForMember(dest => dest.ProductName,
                       opt => opt.MapFrom(src => src.Product.Name)).ReverseMap();
            CreateMap<OrderItem,BuyNowOrderItemDto>().ReverseMap();
            CreateMap<OrderItem, CreateOrderItemDto>().ReverseMap();
            CreateMap<OrderItem, UpdateOrderItemDto>().ReverseMap();

            //Order Mappings
            CreateMap<Order, OrderDto>().ReverseMap();
            CreateMap<Order, CreateOrderDto>().ReverseMap();
            CreateMap<Order, UpdateOrderDto>().ReverseMap();
                



        }
    }

}
