using AutoMapper;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using System.Globalization;

namespace Droniverse.Academy.Application.Services;

public class ImportService : IImportService
{
    private readonly ILogger<ImportService> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ImportService(ILogger<ImportService> logger, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ImportResultDTO> ImportQuizAsync(IFormFile file, Guid quizId)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File is empty.", nameof(file));
        }

        var rawList = ReadQuizQuestion(file, quizId);

        var validList = new List<ImportQuizQuestionDTO>();
        var errors = new List<ImportError>();

        foreach (var item in rawList)
        {
            if (ValidateQuestion(item, errors))
            {
                validList.Add(item);
            }
        }

        if (validList.Any())
        {
            await _unitOfWork.QuizQuestions.AddRangeAsync(
                _mapper.Map<IEnumerable<QuizQuestion>>(validList)
            );

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Imported {Success}/{Total} quiz questions for quiz {QuizId}.", validList.Count, rawList.Count, quizId);
        }

        return new ImportResultDTO
        {
            Total = rawList.Count(),
            Success = validList.Count,
            Failed = errors.Count,
            Errors = errors.Select(e => $"Row {e.Row}: {e.Message}").ToList()
        };
    }

    private List<ImportQuizQuestionDTO> ReadQuizQuestion(IFormFile file, Guid quizId)
    {
        using var stream = file.OpenReadStream();
        using var package = new ExcelPackage(stream);

        var sheet = package.Workbook.Worksheets.FirstOrDefault();
        if (sheet == null)
            throw new Exception("Excel file does not contain any worksheet.");

        var questions = new List<ImportQuizQuestionDTO>();

        int startRow = 3;
        int endRow = sheet.Dimension?.End.Row ?? 0;

        for (int row = startRow; row <= endRow; row++)
        {
            var contentVN = sheet.Cells[row, 2].Text?.Trim();

            if (string.IsNullOrWhiteSpace(contentVN))
                continue;

            var question = new ImportQuizQuestionDTO
            {
                Row = row,

                ContentVN = contentVN,
                ContentEN = sheet.Cells[row, 3].Text?.Trim() ?? string.Empty,

                Score = float.TryParse(sheet.Cells[row, 4].Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var score)
                    ? score
                    : 1,

                CorrectAnswer = (sheet.Cells[row, 5].Text?.Trim() ?? string.Empty).ToUpperInvariant(),

                AnswerA = sheet.Cells[row, 6].Text?.Trim() ?? string.Empty,
                AnswerA_EN = sheet.Cells[row, 7].Text?.Trim() ?? string.Empty,

                AnswerB = sheet.Cells[row, 8].Text?.Trim() ?? string.Empty,
                AnswerB_EN = sheet.Cells[row, 9].Text?.Trim() ?? string.Empty,

                AnswerC = sheet.Cells[row, 10].Text?.Trim() ?? string.Empty,
                AnswerC_EN = sheet.Cells[row, 11].Text?.Trim() ?? string.Empty,

                AnswerD = sheet.Cells[row, 12].Text?.Trim() ?? string.Empty,
                AnswerD_EN = sheet.Cells[row, 13].Text?.Trim() ?? string.Empty,

                QuizID = quizId
            };

            questions.Add(question);
        }

        return questions;
    }

    private bool ValidateQuestion(ImportQuizQuestionDTO q, List<ImportError> errors)
    {
        if (string.IsNullOrWhiteSpace(q.AnswerA) ||
            string.IsNullOrWhiteSpace(q.AnswerB) ||
            string.IsNullOrWhiteSpace(q.AnswerC) ||
            string.IsNullOrWhiteSpace(q.AnswerD))
        {
            errors.Add(new ImportError { Row = q.Row, Message = "All answers must be filled" });
            return false;
        }

        if (!new[] { "A", "B", "C", "D" }.Contains(q.CorrectAnswer?.ToUpperInvariant()))
        {
            errors.Add(new ImportError { Row = q.Row, Message = "CorrectAnswer must be A/B/C/D" });
            return false;
        }

        if (q.Score <= 0)
        {
            errors.Add(new ImportError { Row = q.Row, Message = "Score must be > 0" });
            return false;
        }

        return true;
    }
}
