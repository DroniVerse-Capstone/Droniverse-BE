using AutoMapper;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.HttpClients;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Exceptions;
using Droniverse.Shared.Services;
using System.Linq.Expressions;

namespace Droniverse.Academy.Application.Services;

public class CourseVersionCategoryService : ICourseVersionCategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly IdentityMicroserviceClient _client;

    public CourseVersionCategoryService(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService current, IdentityMicroserviceClient client)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUser = current;
        _client = client;
    }

    public async Task AddCategoryAsync(Guid courseId, Guid versionId, AssignCategoryRequestDTO request)
    {
        var cv = await _unitOfWork.CourseVersions.GetByConditionAsync(v => v.CourseVersionID == versionId && v.CourseID == courseId);
        if (cv == null)
            throw new BaseException("Course version not found.", "NOT_FOUND");

        // prevent duplicate
        if (cv.CourseVersionCategories.Any(c => c.CategoryID == request.CategoryID))
            throw new ValidationException("Category already assigned to course version.");

        var cvc = new CourseVersionCategory
        {
            CategoryID = request.CategoryID,
            CourseVersionID = versionId
        };

        await _unitOfWork.CourseVersionCategories.AddAsync(cvc);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RemoveCategoryAsync(Guid courseId, Guid versionId, Guid categoryId)
    {
        var cvc = await _unitOfWork.CourseVersionCategories.GetByConditionAsync(x => x.CourseVersionID == versionId && x.CategoryID == categoryId);
        if (cvc == null)
            throw new BaseException("Category assignment not found.", "NOT_FOUND");

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
