using Droniverse.Community.Domain.Entities.Mongo;
using Droniverse.Community.Domain.IRepository.Mongo;
using MongoDB.Driver;

namespace Droniverse.Community.Infrastructure.Repositories.Mongo;

internal class InvoiceRepository : IInvoiceRepository
{
    private readonly IMongoCollection<Invoice> _invoices;
    private readonly string collectionName = "invoices";
    public InvoiceRepository(IMongoDatabase mongoDatabase)
    {
        _invoices = mongoDatabase.GetCollection<Invoice>(collectionName);
    }

    public async Task<IEnumerable<Invoice>> GetInvoices()
    {
        IAsyncCursor<Invoice> invoiceList = await _invoices.FindAsync(Builders<Invoice>.Filter.Empty);
        return invoiceList.ToList();
    }
    public async Task<Invoice> AddInvoice(Invoice invoice)
    {
        await _invoices.InsertOneAsync(invoice);
        return invoice;
    }

    public async Task<IEnumerable<Invoice?>> GetInvoicesByCondition(FilterDefinition<Invoice> filter)
    {
        IAsyncCursor<Invoice> invoiceList = await _invoices.FindAsync(filter);
        return invoiceList.ToList();
    }

    public async Task<Invoice?> GetInvoiceByCondition(FilterDefinition<Invoice> filter)
    {
        IAsyncCursor<Invoice> invoice = await _invoices.FindAsync(filter);
        return invoice.FirstOrDefault();
    }

    public async Task<Invoice?> UpdateInvoice(Invoice invoice)
    {
        FilterDefinition<Invoice> filter = Builders<Invoice>.Filter.Eq(temp => temp._id, invoice._id);
        Invoice? existingInvoice = (await _invoices.FindAsync(filter)).FirstOrDefault();
        if (existingInvoice == null)
        {
            return null;
        }

        invoice._id = existingInvoice._id;

        ReplaceOneResult replaceOneResult = await _invoices.ReplaceOneAsync(filter, invoice);
        return replaceOneResult.ModifiedCount > 0 ? invoice : null;
    }

    public async Task<bool?> DeleteInvoice(Guid invoiceID)
    {
        FilterDefinition<Invoice> filter = Builders<Invoice>.Filter.Eq(temp => temp._id, invoiceID);
        Invoice? existingInvoice = (await _invoices.FindAsync(filter)).FirstOrDefault();
        if (existingInvoice == null)
            return false;
        DeleteResult deleteResult = await _invoices.DeleteOneAsync(filter);
        return deleteResult.DeletedCount > 0;
    }
}

