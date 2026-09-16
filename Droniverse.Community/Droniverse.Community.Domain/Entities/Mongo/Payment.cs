using Droniverse.Community.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Droniverse.Community.Domain.Entities.Mongo;
public class Payment
{
    public Guid TransactionID { get; set; }
    [BsonRepresentation(BsonType.String)]
    public PaymentMethod PaymentMethod { get; set; }
    [BsonRepresentation(BsonType.String)]
    public PaymentStatus PaymentStatus { get; set; }
    [BsonRepresentation(BsonType.DateTime)]
    public DateTime TransactionDate { get; set; }
    public string? PaymentUrl { get; set; }
    public string Reference { get; set; } // Mã tham chiếu PayOs
    public string PaymentLinkID { get; set; } // ID của link thanh toán PayOs
    public string Code { get; set; } // Mã kết quả (00=thành công, 01=thất bại, 02=đang xử lý)
    public DateTime? WebhookReceivedAt { get; set; } // Thời điểm nhận webhook từ Payos
}

