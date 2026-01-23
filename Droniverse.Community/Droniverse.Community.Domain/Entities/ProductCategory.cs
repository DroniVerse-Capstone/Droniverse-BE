namespace Droniverse.Community.Domain.Entities;
public class ProductCategory
{
    public Guid CategoryID { get; set; } //char(36)
    public ICollection<Product> Products;
    public string Code { get; set; } //varchar(50)
    public string CategoryNameVN { get; set; }
    public string CategoryNameEN { get; set; }
    public string DescriptionVN { get; set; }
    public string DescriptionEN { get; set; }
    public DateTime CreateAt { get; set; }
    public DateTime UpdateAt { get; set; }
}

