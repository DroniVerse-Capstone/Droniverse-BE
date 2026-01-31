using Droniverse.Community.Domain.Entities.Mongo;
using MongoDB.Driver;

namespace Droniverse.Community.Domain.IRepository.Mongo;

public interface IInvoiceRepository
{
    Task<IEnumerable<Invoice>> GetInvoices();
    Task<IEnumerable<Invoice?>> GetInvoicesByCondition(FilterDefinition<Invoice> filter);
    Task<Invoice?> GetInvoiceByCondition(FilterDefinition<Invoice> filter);
    Task<Invoice?> AddInvoice(Invoice invoice);
    //Task<Invoice?> UpdateInvoice(Invoice order);
    Task<bool?> DeleteInvoice(Guid invoiceID);
}

