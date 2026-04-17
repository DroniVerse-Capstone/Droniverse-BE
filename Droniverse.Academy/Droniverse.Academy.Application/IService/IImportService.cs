using Droniverse.Academy.Application.DTO.Response;
using Microsoft.AspNetCore.Http;

namespace Droniverse.Academy.Application.IService;

public interface IImportService
{
    /// <summary>
    /// Nhập câu hỏi quiz từ file excel đã được tải lên. File phải tuân theo định dạng mẫu đã cung cấp.
    /// </summary>
    Task<ImportResultDTO> ImportQuizAsync(IFormFile file, Guid quizId);
}
