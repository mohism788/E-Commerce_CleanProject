using System.ComponentModel.DataAnnotations;

namespace E_Commerce.DTOs.CategoryDTO
{
    public class CreateCategoryDto
    {
        //constraint less than 50 characters

        [MaxLength(50)]
        public string Name { get; set; }
    }
}
