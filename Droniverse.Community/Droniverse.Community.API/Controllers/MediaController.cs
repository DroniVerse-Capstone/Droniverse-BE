using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.IService;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Response;
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
        /// Lấy thông tin chi tiết của một tệp media theo image URL
        /// </summary>
        /// <param name="imageUrl">URL đầy đủ của tệp media</param>
        /// <remarks>
        /// API tìm media theo đúng giá trị URL đã lưu trong database.
        /// Nếu không tìm thấy media tương ứng, API sẽ trả về 404.
        /// </remarks>
        /// <returns>
        /// 200 OK - Trả về thông tin chi tiết MediaResponseDto
        /// 404 NotFound - Nếu không tồn tại media
        /// </returns>
        [HttpGet("by-url")]
        [ProducesResponseType(typeof(SuccessResponse<MediaResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [AllowAnonymous]
        public async Task<ApiResponse> GetMediaByUrl([FromQuery] string imageUrl)
        {
            var media = await _mediaService.GetMediaByUrl(imageUrl);
            return SuccessResponse<MediaResponseDto>.Create(media, "Lấy thông tin media theo image url thành công!");
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
            var mediaResponse = await _mediaService.UploadTempMedia(dto);
            return SuccessResponse<MediaResponseDto>.Create(mediaResponse, "Tải lên tệp media tạm thành công! (Lưu database background)");
        }

        [HttpGet("mini")]
        [ProducesResponseType(typeof(IEnumerable<MediaMiniResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<MediaMiniResponse>>> GetMiniResponse([FromQuery] List<Guid>? mediaIds)
        {
            var media = await _mediaService.GetMiniResponse(mediaIds);
            return Ok(media);
        }

        /// <summary>
        /// Di chuyển tệp media từ thư mục tạm sang thư mục cuối cùng
        /// </summary>
        /// <param name="request">Chứa MediaId và thư mục đích</param>
        /// <remarks>
        /// API này di chuyển một tệp media từ thư mục Cloudinary hiện tại sang thư mục khác.
        /// Thường dùng để di chuyển media từ "droniverse/temporary" sang "droniverse/Club/{clubId}".
        /// 
        /// Quá trình:
        /// 1. Tải media từ database theo ID
        /// 2. Tải file từ Cloudinary URL hiện tại
        /// 3. Upload lên Cloudinary với thư mục mới
        /// 4. Cập nhật URL trong database
        /// 
        /// </remarks>
        /// <returns>
        /// 200 OK - Media đã được di chuyển thành công
        /// 400 BadRequest - Nếu dữ liệu đầu vào không hợp lệ
        /// 404 NotFound - Nếu không tìm thấy media
        /// 500 InternalServerError - Nếu lỗi khi tải/upload file từ Cloudinary
        /// </returns>
        [HttpPost("move-to-folder")]
        [Authorize(Roles = Roles.AllRoles)]
        [ProducesResponseType(typeof(SuccessResponse<MediaResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ApiResponse> MoveMediaToFolder([FromBody] MoveMediaFolderRequest request)
        {
            try
            {
                // Validate request
                if (request == null)
                    throw new ArgumentNullException(nameof(request), "Request cannot be null.");

                if (request.MediaId == Guid.Empty)
                    throw new ArgumentException("MediaId cannot be empty.", nameof(request.MediaId));

                if (string.IsNullOrWhiteSpace(request.Folder))
                    throw new ArgumentException("Folder cannot be empty or whitespace.", nameof(request.Folder));

                // Get media from database
                var media = await _mediaService.GetMediaById(request.MediaId);
                if (media == null)
                    return SuccessResponse<MediaResponseDto>.Create(null, "Không tìm thấy media với ID: " + request.MediaId);

                // Get full media entity with relationships
                var mediaEntity = await _mediaService.GetFullMedia(request.MediaId);
                if (mediaEntity == null)
                    throw new KeyNotFoundException($"Media entity not found for ID: {request.MediaId}");

                // Move media to new folder
                await _mediaService.UploadMedia(mediaEntity, request.Folder);

                // Get updated media info
                var updatedMedia = await _mediaService.GetMediaById(request.MediaId);
                return SuccessResponse<MediaResponseDto>.Create(updatedMedia, "Di chuyển tệp media thành công!");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument in MoveMediaToFolder: {Message}", ex.Message);
                return SuccessResponse<MediaResponseDto>.Create(null, ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Media not found in MoveMediaToFolder");
                return SuccessResponse<MediaResponseDto>.Create(null, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while moving media to folder: {MediaId} to {Folder}", 
                    request?.MediaId, request?.Folder);
                throw;
            }
        }
    }
}

