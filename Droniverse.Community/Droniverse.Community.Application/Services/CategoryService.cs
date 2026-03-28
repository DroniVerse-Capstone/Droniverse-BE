using AutoMapper;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CategoryService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<CategoryResponseDto> CreateCategory(CategoryRequestDto request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request), "Category request can not be null !");

            Category category = _mapper.Map<Category>(request);
            category.CategoryID = Guid.NewGuid();
            await _unitOfWork.Categories.Add(category);
            await _unitOfWork.SaveChangeAsync();
            return _mapper.Map<CategoryResponseDto>(category);
        }

        public async Task<bool> Delete(Guid id)
        {
            Category? category = await _unitOfWork.Categories.GetByCondition(c => c.CategoryID == id);
            if (category == null)
                throw new KeyNotFoundException($"Category with ID {id} not found !");

            await _unitOfWork.Categories.Delete(category);
            await _unitOfWork.SaveChangeAsync();
            return true;
        }

        public async Task<IEnumerable<CategoryResponseDto>> GetAllCategory()
        {
            IEnumerable<Category> categories = await _unitOfWork.Categories.GetAll();
            IEnumerable<CategoryResponseDto> response = _mapper.Map<IEnumerable<CategoryResponseDto>>(categories);
            return response;
        }

        public async Task<CategoryResponseDto> GetCategoryById(Guid id)
        {
            Category? category = await _unitOfWork.Categories.GetByCondition(c => c.CategoryID == id);
            if (category == null)
            {
                throw new KeyNotFoundException($"Category with ID {id} not found !");
            }
            return _mapper.Map<CategoryResponseDto>(category);
        }

        public async Task<CategoryResponseDto> UpdateCategory(Guid categoryID, CategoryRequestDto request)
        {
            Category? category = await _unitOfWork.Categories.GetByCondition(c => c.CategoryID == categoryID);

            if (category == null)
            {
                throw new KeyNotFoundException($"Category with ID {categoryID} not found !");
            }

            _mapper.Map(request, category);
            await _unitOfWork.Categories.Update(category);
            await _unitOfWork.SaveChangeAsync();
            return _mapper.Map<CategoryResponseDto>(category);
        }

    }
}
