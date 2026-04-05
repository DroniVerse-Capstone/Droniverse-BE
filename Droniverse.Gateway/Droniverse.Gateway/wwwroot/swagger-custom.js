// Danh sách token mẫu
const tokens = {
    "Admin Token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJVc2VySUQiOiI4YTY0ZDk1ZS1mMDQxLTQ5ZjctYmMxOC1hODJhZWNkODE2MTIiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiYWRtaW5AZ21haWwuY29tIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvZW1haWxhZGRyZXNzIjoiYWRtaW5AZ21haWwuY29tIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiQURNSU4iLCJqdGkiOiJhMjNlZDg2Ni1jZGU1LTQxNjctODRkOC04M2Q5MzU1OWU5OGQiLCJleHAiOjE3Nzg0MTU3MjgsImlzcyI6IkRyb25pdmVyc2UuSWRlbnRpdHkiLCJhdWQiOiJEcm9uaXZlcnNlLklkZW50aXR5In0.f-8scgOBuJ7xZQn5vtkAI4kZXaezNCTiUBNYbF1L4zQ",
    "System Manager": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJVc2VySUQiOiIyMTUwZjdmZC05ZDE5LTQ2YjQtYTAzMS0wMjEwYmMxNjE2MGYiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoic3lzbWFuYWdlckBnbWFpbC5jb20iLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJzeXNtYW5hZ2VyQGdtYWlsLmNvbSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlNZU1RFTV9NQU5BR0VSIiwianRpIjoiNmFjZDBhMGMtMTUyZC00NzdmLWEwZTktN2IxNjU5MzYzNGRkIiwiZXhwIjoxNzc4NDE1OTgwLCJpc3MiOiJEcm9uaXZlcnNlLklkZW50aXR5IiwiYXVkIjoiRHJvbml2ZXJzZS5JZGVudGl0eSJ9.IUy3k10f86Ypz76gFYwros9IaFdWlPiQNzGlRUDoYPA",
    "Club Manager": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJVc2VySUQiOiJhZTZkYTdmNS0xNDczLTQ1NmYtOWU1NS03MGRmNzAyZDQ3ZWUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiY2x1Ym1hbmFnZXJAZ21haWwuY29tIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvZW1haWxhZGRyZXNzIjoiY2x1Ym1hbmFnZXJAZ21haWwuY29tIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiQ0xVQl9NQU5BR0VSIiwianRpIjoiMjhlOGViNmItZjUzZi00YWIzLTgyZmEtM2EwMjlmYTM3M2QxIiwiZXhwIjoxNzc4NDE2MDMyLCJpc3MiOiJEcm9uaXZlcnNlLklkZW50aXR5IiwiYXVkIjoiRHJvbml2ZXJzZS5JZGVudGl0eSJ9.sHyLg7SAoq9QFqzkEtWtDipxmw-a5UR0NLLI1nXFeiM",
    "Club Member": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJVc2VySUQiOiIzMTk3NzM0ZC1kMjVkLTQyYjEtYjk2OC04NGI2ZWU0ZDMzYzIiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiY2x1Ym1lbWJlckBnbWFpbC5jb20iLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJjbHVibWVtYmVyQGdtYWlsLmNvbSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkNMVUJfTUVNQkVSIiwianRpIjoiYWY2NTRmZTktNmNlNS00MjY1LTgyNTAtMjZjMmMwNmZlMWNiIiwiZXhwIjoxNzc4NDE2MDc3LCJpc3MiOiJEcm9uaXZlcnNlLklkZW50aXR5IiwiYXVkIjoiRHJvbml2ZXJzZS5JZGVudGl0eSJ9.9lLyp26EMeWGLRwYQnaUjlcd5fzFIW8EGF89zaWLcIY"
};

// Function để tạo dropdown
function addTokenDropdown() {
    // Tìm input field trong modal authorize
    const input = document.querySelector('.modal-ux input[type="text"], .modal-ux input[type="password"]');

    if (!input) {
        console.log('Không tìm thấy input field');
        return;
    }

    // Kiểm tra xem dropdown đã tồn tại chưa
    if (input.parentElement.querySelector('.token-dropdown')) {
        console.log('Dropdown đã tồn tại');
        return;
    }

    // Tạo dropdown
    const select = document.createElement('select');
    select.className = 'token-dropdown';
    select.style.cssText = 'width: 100%; padding: 8px; margin-bottom: 10px; border: 1px solid #d9d9d9; border-radius: 4px; font-size: 14px;';

    // Thêm option mặc định
    const defaultOption = document.createElement('option');
    defaultOption.text = '-- Chọn token có sẵn --';
    defaultOption.value = '';
    select.appendChild(defaultOption);

    // Thêm các token
    for (const [label, token] of Object.entries(tokens)) {
        const option = document.createElement('option');
        option.text = label;
        option.value = token;
        select.appendChild(option);
    }

    // Event khi chọn token
    select.onchange = function () {
        if (this.value) {
            // Query lại mỗi lần thay vì dùng closure
            const currentInput = document.querySelector('.modal-ux input[type="text"], .modal-ux input[type="password"]');
            if (!currentInput) return;

            const nativeInputValueSetter = Object.getOwnPropertyDescriptor(
                window.HTMLInputElement.prototype, 'value'
            ).set;
            nativeInputValueSetter.call(currentInput, this.value);
            currentInput.dispatchEvent(new InputEvent('input', { bubbles: true, inputType: 'insertText' }));
            currentInput.dispatchEvent(new Event('change', { bubbles: true }));
        }
    };

    // Thêm dropdown vào trước input
    input.parentElement.insertBefore(select, input);
    console.log('Dropdown đã được thêm thành công!');
}

// Lắng nghe sự kiện mở modal authorize
document.addEventListener('click', function (e) {
    // Kiểm tra nếu click vào nút Authorize
    if (e.target.classList.contains('authorize') ||
        e.target.closest('.authorize')) {

        console.log('Nút Authorize được click');

        // Đợi modal render xong rồi mới thêm dropdown
        setTimeout(() => {
            addTokenDropdown();
        }, 300);
    }
});

window.addEventListener("load", function () {
    const interval = setInterval(() => {
        if (!window.ui) return;

        clearInterval(interval);

        const config = window.ui.getConfigs();

        // Collapse tất cả endpoint
        config.docExpansion = "none";

        // Bật ô search/filter
        config.filter = true;

        // Hiện thời gian request
        config.displayRequestDuration = true;

        // Giữ token khi reload trang
        config.persistAuthorization = true;

        // Re-render lại Swagger UI với config mới
        const oldSpec = config.spec;
        const oldUrl = config.url;

        window.ui = SwaggerUIBundle({
            ...config,
            spec: oldSpec,
            url: oldUrl
        });
    }, 200);
});

console.log('swagger-custom.js đã được load!');