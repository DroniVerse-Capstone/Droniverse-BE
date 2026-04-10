using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Shared.Exceptions;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

namespace Droniverse.Academy.Application.Services;

public class CertificateImageService : ICertificateImageService
{
    private static readonly string[] PreferredFonts = ["Arial", "Tahoma", "Times New Roman", "Segoe UI"];
    private readonly IHttpClientFactory _httpClientFactory;

    public CertificateImageService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<GeneratedCertificateImageResponseDTO> GenerateImageFromTemplateAsync(
        string templateImageUrl,
        string content,
        CertificateWriteFor certificateWriteFor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(templateImageUrl))
        {
            throw new ValidationException("Thiếu đường dẫn ảnh mẫu chứng chỉ.");
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ValidationException("Không có nội dung để ghi lên chứng chỉ.");
        }

        var httpClient = _httpClientFactory.CreateClient();
        using var response = await httpClient.GetAsync(templateImageUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new BaseException("Không thể tải ảnh mẫu chứng chỉ.", "BAD_REQUEST");
        }

        await using var templateStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var image = await Image.LoadAsync(templateStream, cancellationToken);

        var baseFontSize = Math.Clamp(image.Width / 14f, 28f, 72f);
        var font = ResolveFont(image.Width, certificateWriteFor);
        var originY = certificateWriteFor == CertificateWriteFor.User_Write
            ? image.Height / 2f
            : image.Height / 2f + (baseFontSize * 1.4f);

        var textOptions = new RichTextOptions(font)
        {
            Origin = new PointF(image.Width / 2f, originY),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            WrappingLength = image.Width * 0.8f
        };

        image.Mutate(ctx =>
            ctx.DrawText(textOptions, content.Trim(), Color.Black));

        await using var outputStream = new MemoryStream();
        await image.SaveAsPngAsync(outputStream, new PngEncoder
        {
            CompressionLevel = PngCompressionLevel.BestSpeed
        }, cancellationToken);

        return new GeneratedCertificateImageResponseDTO
        {
            Content = outputStream.ToArray(),
            FileName = $"certificate-{Guid.NewGuid():N}.png",
            ContentType = "image/png"
        };
    }

    private static Font ResolveFont(int imageWidth, CertificateWriteFor certificateWriteFor)
    {
        var userWriteFontSize = Math.Clamp(imageWidth / 14f, 28f, 72f);
        var fontSize = certificateWriteFor == CertificateWriteFor.User_Write
            ? userWriteFontSize
            : Math.Clamp(userWriteFontSize * 0.7f, 18f, 56f);
        var fontStyle = certificateWriteFor == CertificateWriteFor.User_Write
            ? FontStyle.Bold
            : FontStyle.Italic;

        foreach (var preferredFont in PreferredFonts)
        {
            if (SystemFonts.TryGet(preferredFont, out var family))
            {
                return family.CreateFont(fontSize, fontStyle);
            }
        }

        var fallbackFamily = SystemFonts.Collection.Families.First();
        return fallbackFamily.CreateFont(fontSize, fontStyle);
    }
}
