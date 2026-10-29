/**
 * Tiện ích trích xuất và định dạng thông báo lỗi chi tiết từ Backend
 * Xử lý mọi trường hợp: ValidationException, ASP.NET Core ValidationProblemDetails,
 * lỗi kết nối HTTP, hoặc lỗi hệ thống.
 */
export function extractErrorMessage(err: any, fallbackMessage: string = 'Đã có lỗi xảy ra.'): string {
  if (!err) return fallbackMessage;
  if (typeof err === 'string') return err;

  // 1. Lỗi trả về từ Backend (Middleware tự định nghĩa: { message: "..." })
  if (err.error?.message && typeof err.error.message === 'string') {
    return err.error.message;
  }

  // 2. Lỗi validation của ASP.NET Core ProblemDetails ({ errors: { fieldName: ["lỗi 1", "lỗi 2"] } })
  if (err.error?.errors && typeof err.error.errors === 'object') {
    const errorMessages: string[] = [];
    for (const [field, messages] of Object.entries(err.error.errors)) {
      if (Array.isArray(messages)) {
        messages.forEach((msg) => {
          if (typeof msg === 'string') {
            errorMessages.push(msg);
          }
        });
      } else if (typeof messages === 'string') {
        errorMessages.push(messages);
      }
    }
    if (errorMessages.length > 0) {
      return errorMessages.join('; ');
    }
  }

  // 3. ProblemDetails detail hoặc title
  if (err.error?.detail && typeof err.error.detail === 'string') {
    return err.error.detail;
  }
  if (err.error?.title && typeof err.error.title === 'string') {
    return err.error.title;
  }

  // 4. Các mã trạng thái HTTP phổ biến
  if (err.status === 0) {
    return 'Không thể kết nối đến máy chủ Backend (Port 5135). Vui lòng kiểm tra server có đang chạy không.';
  }
  if (err.status === 401) {
    return 'Phiên đăng nhập đã hết hạn hoặc chưa đăng nhập. Vui lòng đăng nhập lại.';
  }
  if (err.status === 403) {
    return 'Bạn không có quyền thực hiện thao tác này.';
  }
  if (err.status === 404) {
    return 'Không tìm thấy dữ liệu yêu cầu (404 Not Found).';
  }
  if (err.status === 500) {
    return 'Máy chủ gặp lỗi nội bộ (500 Internal Server Error). Vui lòng kiểm tra log backend.';
  }

  // 5. Chuỗi message chung của HttpErrorResponse
  if (err.message && typeof err.message === 'string') {
    return err.message;
  }

  return fallbackMessage;
}
