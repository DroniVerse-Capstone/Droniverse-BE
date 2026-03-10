namespace Droniverse.Community.Application.DTO.Response
{
    public class CompetitionCertificateResponseDto
    {
        public Guid CompetitionID { get; set; }
        public Guid CertificateID { get; set; }
        public CertificateDetailDto? CertificateDetail { get; set; }
    }

    public class CompetitionCertificatesBulkResponseDto
    {
        public Guid CompetitionID { get; set; }
        public int TotalAdded { get; set; }
        public List<CompetitionCertificateResponseDto> Certificates { get; set; } = new();
    }

    public class CertificateDetailDto
    {
        public Guid CertificateID { get; set; }
        public Guid CourseVersionID { get; set; }
        public string CertificateName { get; set; }
        public string ImageUrl { get; set; }
        public string LogoCertificate { get; set; }
        public string Description { get; set; }
        public string Signature { get; set; }
        public string AuthorName { get; set; }
        public DateTime CreateAt { get; set; }
        public Guid CreateBy { get; set; }
        public Guid UpdateBy { get; set; }
        public DateTime UpdateAt { get; set; }
    }
}
