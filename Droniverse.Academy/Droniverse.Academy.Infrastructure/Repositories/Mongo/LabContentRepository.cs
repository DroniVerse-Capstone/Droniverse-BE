using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository.Mongo;
using MongoDB.Driver;

namespace Droniverse.Academy.Infrastructure.Repositories.Mongo;

internal class LabContentRepository : ILabContentRepository
{
    private readonly IMongoCollection<LabContent> _collection;

    public LabContentRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<LabContent>("lab_contents");
    }

    public async Task<LabContent> CreateAsync(LabContent labContent, CancellationToken cancellationToken = default)
    {
        await _collection.InsertOneAsync(labContent, cancellationToken: cancellationToken);
        return labContent;
    }

    public async Task<IEnumerable<LabContent>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _collection.Find(_ => true).ToListAsync(cancellationToken);
    }

    public async Task<LabContent?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _collection.Find(x => x._id == id).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<LabContent?> UpdateAsync(string id, LabContent labContent, CancellationToken cancellationToken = default)
    {
        labContent._id = id;
        var result = await _collection.ReplaceOneAsync(x => x._id == id, labContent, cancellationToken: cancellationToken);

        if (result.MatchedCount == 0)
        {
            return null;
        }

        return labContent;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var result = await _collection.DeleteOneAsync(x => x._id == id, cancellationToken);
        return result.DeletedCount > 0;
    }
}
