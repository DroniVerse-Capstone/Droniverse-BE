using Droniverse.Academy.Application.IService.Mongo;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository.Mongo;
using MongoDB.Bson;
using System.Text.Json;
using Droniverse.Shared.Exceptions;

namespace Droniverse.Academy.Application.Services.Mongo;

public class LabContentService : ILabContentService
{
    private readonly ILabContentRepository _labContentRepository;

    public LabContentService(ILabContentRepository labContentRepository)
    {
        _labContentRepository = labContentRepository;
    }

    public async Task<LabContentResponseDTO> CreateEmptyAsync(Guid labId, CancellationToken cancellationToken = default)
    {
        var labContent = new LabContent
        {
            _id = labId.ToString(),
            Environment = new BsonDocument()
        };

        await _labContentRepository.CreateAsync(labContent, cancellationToken);
        return ToResponse(labContent);
    }

    public async Task<LabContentResponseDTO?> GetByLabIdAsync(Guid labId, CancellationToken cancellationToken = default)
    {
        var labContent = await _labContentRepository.GetByIdAsync(labId.ToString(), cancellationToken);
        if (labContent == null)
            return null;

        return ToResponse(labContent);
    }

    public async Task<LabContentResponseDTO> UpdateByLabIdAsync(Guid labId, UpdateLabContentRequestDTO request, CancellationToken cancellationToken = default)
    {
        var existing = await _labContentRepository.GetByIdAsync(labId.ToString(), cancellationToken);
        if (existing == null)
            throw new BaseException("Không tìm thấy lab content.", "NOT_FOUND");

        existing.Environment = ToBsonDocument(request.Environment);
        var updated = await _labContentRepository.UpdateAsync(labId.ToString(), existing, cancellationToken);
        if (updated == null)
            throw new BaseException("Cập nhật lab content thất bại.", "UPDATE_FAILED");

        return ToResponse(updated);
    }

    public Task<bool> DeleteByLabIdAsync(Guid labId, CancellationToken cancellationToken = default)
    {
        return _labContentRepository.DeleteAsync(labId.ToString(), cancellationToken);
    }

    private static LabContentResponseDTO ToResponse(LabContent labContent)
    {
        return new LabContentResponseDTO
        {
            LabID = Guid.Parse(labContent._id),
            Environment = ToJsonElement(labContent.Environment)
        };
    }

    private static BsonDocument ToBsonDocument(JsonElement jsonElement)
    {
        if (jsonElement.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            return new BsonDocument();

        return BsonDocument.Parse(jsonElement.GetRawText());
    }

    private static JsonElement ToJsonElement(BsonDocument? bsonDocument)
    {
        var rawJson = bsonDocument?.ToJson() ?? "{}";
        return JsonDocument.Parse(rawJson).RootElement.Clone();
    }
}
