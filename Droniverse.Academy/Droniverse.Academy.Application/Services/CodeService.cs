using AutoMapper;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.Exceptions;

namespace Droniverse.Academy.Application.Services;

public class CodeService : ICodeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    public CodeService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }
    public async Task<IEnumerable<string>> CreateCodeAsync(Guid courseId, int quantity)
    {
        Course? course = await _unitOfWork.Courses.GetByConditionAsync(c => c.CourseID == courseId, includeProperties:"CurrentVersion");
        if (course is null)
        {
            throw new NotFoundException($"Course with id {courseId} not found");
        }

        List<Code> codes = new List<Code>();
        for (int i = 0; i < quantity; i++)
        {
            Code code = new Code
            {
                CodeID = GenerateCodeId(course.CurrentVersion.TitleEN),
                CourseID = courseId,
                ExpireDate = DateTime.UtcNow.AddHours(7).AddMonths(6),
                Status = CodeStatus.ACTIVE,
            };
            codes.Add(code);
        }

        foreach (var code in codes)
        {
            await _unitOfWork.Codes.AddAsync(code);
        }

        //Code addedCode = await _unitOfWork.Codes.AddAsync(code);
        await _unitOfWork.SaveChangesAsync();

        //CodeResponseDTO response = _mapper.Map<CodeResponseDTO>(addedCode);
        IEnumerable<string> listCodeIds = codes.Select(c => c.CodeID).ToList();
        return listCodeIds;

    }

    private string GenerateCodeId(string courseName)
    {
        // Lấy 4 ký tự đầu tiên trong courseName
        string prefix = courseName.Substring(0, Math.Min(4, courseName.Length)).ToUpper();

        // Nếu courseName < 4 ký tự, pad thêm ký tự
        while (prefix.Length < 4)
        {
            prefix += GenerateRandomChar();
        }

        //Lấy ddMMyyyy
        string datePart = DateTime.UtcNow.AddHours(7).ToString("ddMMyy");

        //Generate 2 phần ngẫu nhiên
        string randomPart1 = GenerateRandomPart();
        string randomPart2 = GenerateRandomPart();

        return $"{prefix}-{datePart}-{randomPart1}-{randomPart2}";

    }

    private string GenerateRandomPart()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Range(0, 4)
        .Select(_ => chars[random.Next(chars.Length)])
        .ToArray());
    }

    private char GenerateRandomChar()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        Random random = new Random();
        return chars[random.Next(chars.Length)];
    }

    public Task<CodeResponseDTO> DeleteCodeAsync(string codeId)
    {
        throw new NotImplementedException();
    }

    public async Task<PaginationResult<IEnumerable<CodeResponseDTO>>> GetAllCodesAsync()
    {
        PaginationResult<IEnumerable<Code>> codes = await _unitOfWork.Codes.GetAllAsync();
        // Map IEnumerable<Code> -> IEnumerable<CodeResponseDTO>
        var mappedCodes = _mapper.Map<IEnumerable<CodeResponseDTO>>(codes.Data);

        // Tạo PaginationResult mới với data đã mapped
        return new PaginationResult<IEnumerable<CodeResponseDTO>>(
            mappedCodes,
            codes.TotalRecords,
            codes.PageIndex,
            codes.PageSize
        );
    }

    public async Task<CodeResponseDTO> GetCodeAsync(string codeId)
    {
        Code? code = await _unitOfWork.Codes.GetByIdAsync(codeId);
        if (code is null)
        {
            throw new NotFoundException($"Code with id {codeId} not found");
        }
        CodeResponseDTO response = _mapper.Map<CodeResponseDTO>(code);
        return response;
    }

    public Task<CodeResponseDTO> UpdateCodeAsync(string codeId)
    {
        throw new NotImplementedException();
    }
}

