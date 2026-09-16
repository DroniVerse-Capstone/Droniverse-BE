namespace Droniverse.Shared.DTOs
{
    public record ClubCodesResponse
    {
        public required string ClubID { get; set; }
        public required SimpleCourseResponse CourseInfo { get; set; }
        public required PaginationResult<IEnumerable<CodeEntryResponse>> CodesItem { get; set; }
    }

    public record CodeEntryResponse
    {
        public required string Code { get; set; }
        public SimpleUserReponse? OwnerInfo { get; set; }
        public SimpleUserReponse? ComsumerInfo { get; set; }
        public DateTime ExpireDate { get; set; }
    }

}
