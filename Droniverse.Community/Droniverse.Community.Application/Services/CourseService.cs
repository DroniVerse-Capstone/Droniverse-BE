using AutoMapper;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.IRepository;

namespace Droniverse.Community.Application.Services;
internal class CourseService : ICourseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    public CourseService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }
    public async Task<IEnumerable<CourseResponse>> GetAllCourses()
    {
        //IEnumerable<Course> courses = await _unitOfWork.Courses.GetAll();
        //IEnumerable<CourseResponse> responses = _mapper.Map<IEnumerable<CourseResponse>>(courses);
        return null;
    }
}

