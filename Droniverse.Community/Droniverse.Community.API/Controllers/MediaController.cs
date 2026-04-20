using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.IService;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Community.API.Controllers
{
    [Route("community/media")]
    [ApiController]
    [Authorize]
    public class MediaController : ControllerBase
    {
        private readonly ILogger<MediaController> _logger;
        private readonly IMediaService _mediaService;
        private readonly ICloudinaryService _cloudinaryService;

        public MediaController(
            ILogger<MediaController> logger,
            IMediaService mediaService,
            ICloudinaryService cloudinaryService)
        {
            _logger = logger;
            _mediaService = mediaService;
            _cloudinaryService = cloudinaryService;
        }

        /// <summary>
        /// Lấy toàn bộ danh sách media
        /// </summary>
        /// <remarks>
        /// API trả về danh sách tất cả các tệp media trong hệ thống.
        /// </remarks>
        /// <returns>
        /// 200 OK - Trả về danh sách MediaResponseDto
        /// </returns>
        [HttpGet]
        [ProducesResponseType(typeof(SuccessResponse<IEnumerable<MediaResponseDto>>), StatusCodes.Status200OK)]
        [AllowAnonymous]
        public async Task<ApiResponse> GetMedia()
        {
            var media = await _mediaService.GetAllMedia();
            return SuccessResponse<IEnumerable<MediaResponseDto>>.Create(media, "Lấy danh sách media thành công!");
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một tệp media theo ID
        /// </summary>
        /// <param name="id">ID của tệp media</param>
        /// <remarks>
        /// Nếu không tìm thấy media theo ID truyền vào, API sẽ trả về 404.
        /// </remarks>
        /// <returns>
        /// 200 OK - Trả về thông tin chi tiết MediaResponseDto  
        /// 404 NotFound - Nếu không tồn tại media
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(SuccessResponse<MediaResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [AllowAnonymous]
        public async Task<ApiResponse> GetMediaById(Guid id)
        {
            var media = await _mediaService.GetMediaById(id);
            return SuccessResponse<MediaResponseDto>.Create(media, "Lấy thông tin media thành công!");
        }

        /// <summary>
        /// Upload tệp media tạm (hình ảnh hoặc video)
        /// </summary>
        /// <param name="dto">Tệp media và loại media (IMAGE hoặc VIDEO)</param>
        /// <remarks>
        /// API cho phép tải lên tệp hình ảnh hoặc video tạm thời.
        /// Loại media được xác định bởi tham số MediaType.
        /// Tệp được lưu trữ trên Cloudinary.
        /// 
        /// **Tối ưu hóa**: 
        /// - Response được trả về ngay sau khi Cloudinary nhận file thành công (không chờ transcode)
        /// - Lưu vào database được xử lý async background (fire-and-forget)
        /// - Giảm thời gian upload xuống đáng kể (đặc biệt với video file lớn)
        /// </remarks>
        /// <returns>
        /// 202 Accepted - Tệp đã được Cloudinary nhận, sẽ lưu vào database background.
        /// 400 BadRequest - Nếu dữ liệu đầu vào không hợp lệ hoặc loại tệp không được hỗ trợ.
        /// 404 NotFound - Nếu không tìm thấy MediaType.
        /// </returns>
        [HttpPost("upload-temp")]
        [Authorize(Roles = Roles.AllRoles)]
        [ProducesResponseType(typeof(SuccessResponse<MediaResponseDto>), StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ApiResponse> UploadTempMedia([FromForm] FileUploadMediaDto dto)
        {
            var mediaResponse = await _mediaService.UploadTempMedia(dto, _cloudinaryService);
            return SuccessResponse<MediaResponseDto>.Create(mediaResponse, "Tải lên tệp media tạm thành công! (Lưu database background)");
        }
    } 
}

