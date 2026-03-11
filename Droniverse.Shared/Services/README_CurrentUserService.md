# H??ng d?n s? d?ng ICurrentUserService

## ?? M?c ?ích
`ICurrentUserService` giúp b?n d? dàng l?y thông tin ng??i dùng hi?n t?i t? JWT token trong b?t k? Controller, Service nào.

## ? ?ã c?u hình
- ? `ICurrentUserService` và `CurrentUserService` ?ã ???c t?o trong `Droniverse.Shared/Services/`
- ? ?ã ??ng ký trong `DependencyInjection.cs`
- ? JWT middleware t? ??ng ??c token t? Cookie ho?c Authorization header
- ? Token ???c validate và parse thành Claims t? ??ng

## ?? Cách s? d?ng

### 1. Inject vào Constructor

```csharp
public class YourController : ControllerBase
{
    private readonly ICurrentUserService _currentUserService;

    public YourController(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    [Authorize]
    [HttpGet("my-info")]
    public IActionResult GetMyInfo()
    {
        // L?y thông tin user
        var userId = _currentUserService.UserId;
        var userName = _currentUserService.UserName;
        var email = _currentUserService.Email;
        var roles = _currentUserService.Roles;
        var isAuth = _currentUserService.IsAuthenticated;

        return Ok(new 
        { 
            UserId = userId,
            UserName = userName,
            Email = email,
            Roles = roles,
            IsAuthenticated = isAuth
        });
    }
}
```

### 2. S? d?ng trong Service Layer

```csharp
public class YourService : IYourService
{
    private readonly ICurrentUserService _currentUserService;

    public YourService(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public async Task<Post> CreatePost(CreatePostDto dto)
    {
        // T? ??ng l?y UserId t? token
        var post = new Post
        {
            Title = dto.Title,
            Content = dto.Content,
            CreatedBy = _currentUserService.UserId, // ? L?y t? token
            CreatedAt = DateTime.UtcNow
        };

        await _postRepository.AddAsync(post);
        return post;
    }
}
```

### 3. Ki?m tra Authentication

```csharp
[HttpPost]
public IActionResult CreateSomething(CreateDto dto)
{
    if (!_currentUserService.IsAuthenticated)
    {
        return Unauthorized("B?n c?n ??ng nh?p!");
    }

    // X? lý logic...
}
```

### 4. Ki?m tra Role

```csharp
[HttpDelete("{id}")]
public IActionResult DeletePost(Guid id)
{
    var roles = _currentUserService.Roles;
    
    if (!roles.Contains("Admin"))
    {
        return Forbid("B?n không có quy?n xóa!");
    }

    // X? lý logic...
}
```

## ?? Properties có s?n

| Property | Type | Mô t? |
|----------|------|-------|
| `UserId` | `string?` | ID c?a ng??i dùng (t? NameIdentifier claim) |
| `UserName` | `string?` | Tên ng??i dùng (t? Name claim) |
| `Email` | `string?` | Email ng??i dùng (t? Email claim) |
| `Roles` | `IEnumerable<string>` | Danh sách các role |
| `IsAuthenticated` | `bool` | Ki?m tra ?ã xác th?c ch?a |
| `User` | `ClaimsPrincipal` | Toàn b? claims c?a user |

## ?? Ví d? th?c t?

### Endpoint test trong CategoryController
?ã t?o s?n endpoint `/community/categories/current-user` ?? test:

```bash
# Test v?i Postman ho?c curl
GET http://localhost:5000/community/categories/current-user
Authorization: Bearer <your_token>
# Ho?c token s? t? ??ng ??c t? Cookie "AccessToken"
```

Response:
```json
{
  "data": {
    "userId": "123e4567-e89b-12d3-a456-426614174000",
    "userName": "john_doe",
    "email": "john@example.com",
    "roles": ["User", "Admin"],
    "isAuthenticated": true
  },
  "message": "L?y thông tin ng??i dùng thành công!",
  "success": true
}
```

## ?? L?u ý b?o m?t
- ? Token ?ã ???c validate t? ??ng b?i JWT middleware
- ? Ch? c?n thêm `[Authorize]` attribute là token s? ???c ki?m tra
- ? N?u token invalid/expired, API s? t? ??ng tr? v? 401 Unauthorized
- ? Claims ?ã ???c parse s?n, không c?n gi?i mã th? công

## ?? Hoàn thành!
Gi? b?n có th? s? d?ng `ICurrentUserService` ? b?t k? ?âu trong ?ng d?ng!
