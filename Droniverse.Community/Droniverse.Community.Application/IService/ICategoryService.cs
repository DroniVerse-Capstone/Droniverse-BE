using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.IService
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryResponseDto>> GetAllCategory();
        Task<CategoryResponseDto> GetCategoryById(Guid id);
        Task<CategoryResponseDto> CreateCategory(CategoryRequestDto request);
        Task<CategoryResponseDto> UpdateCategory(Guid categoryID, CategoryRequestDto request);
        Task<bool> Delete(Guid id);
    }
}
