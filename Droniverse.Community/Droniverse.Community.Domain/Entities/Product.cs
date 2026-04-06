using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Domain.Entities;
public class Product
{
    public Guid ProductID { get; set; }
    public Guid ReferenceID { get; set; } //Id của course hoặc drone
    //public ProductCategory ProductCategory { get; set; }
    //public Guid? CategoryID { get; set; }
    public ICollection<UserProduct> UserProducts { get; set; }

    public string ProductNameVN { get; set; }
    public string ProductNameEN { get; set; }
    public string DescriptionVN { get; set; }
    public string DescriptionEN { get; set; }
    public decimal Price { get; set; }
    public CurrencyType Currency { get; set; }
    public ProductStatus Status { get; set; }
    public DateTime CreateAt { get; set; }
    public DateTime UpdateAt { get; set; }

    // Foreign Keys
}

