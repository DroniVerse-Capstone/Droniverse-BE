using AutoMapper;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Exceptions;
using System.Linq.Expressions;

namespace Droniverse.Academy.Application.Services;

public class CourseVersionCategoryService : ICourseVersionCategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CourseVersionCategoryService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task AddCategoriesAsync(Guid courseId, Guid versionId, AssignCategoriesRequestDTO request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (request.CategoryIDs.Count == 0)
            throw new ValidationException("Danh sách categoryID là bắt buộc.");

        var categoryIds = request.CategoryIDs.Distinct().ToList();
        if (categoryIds.Any(id => id == Guid.Empty))
            throw new ValidationException("CategoryID không hợp lệ.");

        var cv = await _unitOfWork.CourseVersions.GetByConditionAsync(v => v.CourseVersionID == versionId && v.CourseID == courseId);
        if (cv == null)
            throw new BaseException("Không tìm thấy phiên bản khóa học.", "NOT_FOUND");

        var existing = await _unitOfWork.CourseVersionCategories.GetAllAsync(
            filter: x => x.CourseVersionID == versionId,
            pageIndex: 1,
            pageSize: int.MaxValue);

        foreach (var item in existing.Data)
        {
            await _unitOfWork.CourseVersionCategories.DeleteAsync(item);
        }

        foreach (var categoryId in categoryIds)
        {
            await _unitOfWork.CourseVersionCategories.AddAsync(new CourseVersionCategory
            {
                CategoryID = categoryId,
                CourseVersionID = versionId
            });
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RemoveCategoryAsync(Guid courseId, Guid versionId, Guid categoryId)
    {
        var cvc = await _unitOfWork.CourseVersionCategories.GetByConditionAsync(x => x.CourseVersionID == versionId && x.CategoryID == categoryId);
        if (cvc == null)
            throw new BaseException("Không tìm thấy thông tin gán danh mục.", "NOT_FOUND");

        await _unitOfWork.CourseVersionCategories.DeleteAsync(cvc);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<PaginationResult<IEnumerable<CategoryResponseDTO>>> GetCategoriesAsync(Guid courseId, Guid versionId, int pageIndex = 1, int pageSize = 50)
    {
        var result = await _unitOfWork.CourseVersionCategories.GetAllAsync(c => c.CourseVersionID == versionId, null, pageIndex, pageSize);
        var mapped = result.Data.Select(c => _mapper.Map<CategoryResponseDTO>(c)).ToList();
        return new PaginationResult<IEnumerable<CategoryResponseDTO>>(mapped, result.TotalRecords, result.PageIndex, result.PageSize);
    }

    public async Task<PaginationResult<IEnumerable<CourseVersionResponseDTO>>> GetCourseVersionsByCategoryAsync(Guid categoryId, int pageIndex = 1, int pageSize = 50, bool activeOnly = true)
    {
        Expression<Func<CourseVersionCategory, bool>>? filter = c => c.CategoryID == categoryId;
        var cvcResult = await _unitOfWork.CourseVersionCategories.GetAllAsync(filter, null, pageIndex, pageSize);

        var ids = cvcResult.Data.Select(x => x.CourseVersionID).ToList();
        Expression<Func<CourseVersion, bool>> courseFilter = cv => ids.Contains(cv.CourseVersionID);
        if (activeOnly)
            courseFilter = cv => ids.Contains(cv.CourseVersionID) && cv.Status == CourseVersionStatus.ACTIVE;

        var courseResult = await _unitOfWork.CourseVersions.GetAllAsync(courseFilter, null, pageIndex, pageSize, includeProperties: "Course");
        var mapped = courseResult.Data.Select(cv => _mapper.Map<CourseVersionResponseDTO>(cv)).ToList();

        //try
        //{
        //    var user = await _client.GetUserByUserID(Guid.Parse("7b58f729-ec26-48c0-93c4-29884afaa6ee"));
        //    Console.WriteLine(user.FirstName + user.LastName);
        //}
        //catch (Exception ex)
        //{
        //    Console.WriteLine(ex.Message);
        //}

        return new PaginationResult<IEnumerable<CourseVersionResponseDTO>>(mapped, courseResult.TotalRecords, courseResult.PageIndex, courseResult.PageSize);
    }
}
