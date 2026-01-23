using System.ComponentModel.DataAnnotations;

namespace E_Commerce.DTOs.CategoryDTO
{
    public class UpdateCategoryDto
    {
        [MaxLength(50)]
        public string Name { get; set; }
    }
}
