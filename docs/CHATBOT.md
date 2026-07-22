# FapWeb Chatbot (MiMo) – Tài liệu kỹ thuật

Tài liệu này mô tả chatbot tra cứu tích hợp trong FapWeb: phạm vi, quy tắc bảo mật, cấu hình, luồng xử lý và cách kiểm thử. Đọc tài liệu này trước khi thay đổi `ChatbotService`, `ChatController` hoặc giao diện chatbot.

## 1. Mục đích và phạm vi

Chatbot là trợ lý **chỉ đọc** dành cho người đã đăng nhập. Nó hỗ trợ:

- Hướng dẫn sử dụng FapWeb.
- Tra cứu lịch, điểm danh, học phí và thông báo mà người dùng đang đăng nhập được phép xem.
- Trả kèm tối đa một liên kết điều hướng nội bộ đến màn hình phù hợp.

Chatbot **không** được phép tạo, sửa, xóa dữ liệu; gửi nhắc nhở; thanh toán; điểm danh; upload tệp; tìm kiếm web; hoặc gọi tool bên ngoài. Lịch sử hội thoại chỉ tồn tại trong bộ nhớ của trang hiện tại, không được lưu vào PostgreSQL.

## 2. Kiến trúc và luồng xử lý

Các thành phần chính:

| Thành phần | Trách nhiệm |
| --- | --- |
| `Views/Shared/_Chatbot.cshtml` | Widget nổi, câu hỏi gợi ý theo role và anti-forgery token. Chỉ render khi đã đăng nhập. |
| `wwwroot/js/chatbot.js` | Gửi form đến `/Chat/Ask`, hiển thị loading/error/link điều hướng và đưa text vào DOM bằng `textContent`. |
| `Controllers/ChatController.cs` | Xác thực session, validate request, chống CSRF, giới hạn request theo session và trả JSON. |
| `Services/Service/ChatRequestLimiter.cs` | Chặn nhiều request đồng thời trong cùng session và giới hạn 10 request/5 phút mặc định. |
| `Services/Service/ChatbotService.cs` | Phân loại ý định, lấy context được phép, gọi MiMo, chuẩn hóa output và tạo link nội bộ. |
| `Models/Configurations/MiMoSettings.cs` | Kiểu cấu hình MiMo và các limit. |

Luồng chuẩn:

1. Người dùng gửi câu hỏi từ widget.
2. `ChatController` đọc `UserId` và `RoleName` từ session; client không được quyền tự gửi role, URL hay ID dữ liệu.
3. Controller kiểm tra anti-forgery, độ dài câu hỏi và rate limit.
4. `ChatbotService` xác định intent, truy vấn PostgreSQL theo rule của role, rồi chỉ gửi context tối thiểu đó cùng câu hỏi đến MiMo.
5. Service chuẩn hóa Markdown thô thành plain text, thêm một action link do server quyết định và trả `answer`, `suggestedActionLabel`, `suggestedActionUrl`.
6. JavaScript hiển thị plain text; URL chỉ được chấp nhận nếu là đường dẫn nội bộ bắt đầu bằng `/`.

## 3. Quy tắc dữ liệu và phân quyền

Không được nới lỏng các filter sau chỉ để chatbot “trả lời tốt hơn”. Mọi query chatbot phải bắt đầu từ `currentUserId` và `roleName` của server.

| Role | Dữ liệu chatbot được đọc | Không được đọc |
| --- | --- | --- |
| `STUDENT` | Lịch của lớp đang theo học, điểm danh và học phí của chính mình, thông báo nhận được. | Dữ liệu học sinh khác, lớp khác, dữ liệu quản trị. |
| `PARENT` | Dữ liệu của học sinh có bản ghi `StudentGuardian` liên kết với phụ huynh hiện tại. | Dữ liệu trẻ không liên kết hoặc thông tin phụ huynh khác. |
| `TEACHER` | Lịch/lớp được phân công, điểm danh và học phí thuộc các lớp giáo viên phụ trách. | Lớp của giáo viên khác. |
| `ADMIN` | Hướng dẫn và điều hướng hệ thống. | Tổng hợp dữ liệu hệ thống hoặc dữ liệu cá nhân của người khác qua chatbot v1. |

Context chỉ chứa tối đa năm bản ghi cho mỗi câu hỏi. Không đưa vào prompt: mật khẩu, API key, email, số điện thoại, cookie/session ID hoặc connection string. Nội dung thông báo và chủ đề lịch được cắt ngắn trước khi gửi để tránh prompt quá dài.

## 4. Intent, điều hướng và quy ước output

Intent hiện được nhận diện bằng keyword tiếng Việt trong `ChatbotService`:

| Intent | Keyword ví dụ | Action nội bộ |
| --- | --- | --- |
| `Schedule` | lịch, buổi học, buổi dạy | Student/Parent: `/Dashboard`; Staff: `/ScheduleManagement` |
| `Attendance` | điểm danh, vắng, có mặt, chuyên cần | Student/Parent: `/Attendance/History`; Staff: `/Attendance` |
| `AttendanceSummary` | bao nhiêu buổi, mấy buổi, tổng số, tỷ lệ chuyên cần kết hợp với từ khóa điểm danh | Student/Parent: `/Attendance/History`; Staff: `/Attendance` |
| `Tuition` | học phí, thanh toán, còn nợ | `/Tuition` |
| `Notification` | thông báo, nhắc nhở, chưa đọc | `/Notification` |
| `Guidance` | các câu còn lại | `/Dashboard` |

Prompt yêu cầu MiMo trả lời tiếng Việt, plain text, tối đa năm ý ngắn. `NormalizeAnswer` loại bỏ Markdown phổ biến (`**`, `__`, backtick, heading) và chuyển bullet `*`/`-` thành `•`. Khi thay đổi quy tắc này, phải giữ output an toàn cho `textContent`; không dùng `innerHTML` để render câu trả lời từ model.

### Thống kê có mặt/vắng của phụ huynh và học sinh

`AttendanceSummary` là nhánh trả lời xác định ở server và chạy trước bước kiểm tra cấu hình MiMo. Server tự đếm `PRESENT`, `ABSENT`, tổng số buổi đã có kết quả, tỷ lệ chuyên cần và số lịch quá khứ chưa được điểm danh. Không gửi toàn bộ lịch sử sang MiMo và không để model tự cộng số liệu.

- Không nêu thời gian: tính toàn bộ lịch sử đến ngày hiện tại.
- Hỗ trợ `hôm nay`, `tuần này`, `tháng này`, `tháng trước`.
- Có thể nhận diện tên học sinh đã liên kết và tên lớp/môn trong câu hỏi.
- Một phụ huynh có nhiều con nhưng không nêu tên: trả kết quả riêng cho từng con.
- Buổi chưa có bản ghi điểm danh được báo riêng, không tự coi là vắng.
- Học sinh hoặc lớp được chọn chỉ từ quan hệ server đã xác thực; client không gửi `studentId`, `classId` hay role.
- Vì không phụ thuộc MiMo, thống kê này vẫn hoạt động khi thiếu API key, timeout hoặc provider hết quota.

## 5. Cấu hình và chạy cục bộ

`.env` là file bí mật, đã bị Git ignore. Copy từ `.env.example`, sau đó điền key thật:

```dotenv
MiMo__ApiBaseUrl=https://api.xiaomimimo.com/v1/chat/completions
MiMo__Model=mimo-v2.5
MiMo__ApiKey=YOUR_MIMO_API_KEY
MiMo__RequestTimeoutSeconds=20
MiMo__MaxQuestionLength=500
MiMo__MaxCompletionTokens=500
MiMo__MaxRequestsPerWindow=10
MiMo__RateLimitWindowMinutes=5
```

Khi chạy Docker Compose, Docker tự nạp `.env`. Khi chạy bằng `dotnet run`, `Program.cs` cũng nạp `.env`; cặp nháy bao ngoài giá trị sẽ được bỏ để connection string hợp lệ. Biến môi trường thực của máy được nạp sau `.env` nên có ưu tiên cao hơn.

Tuyệt đối không commit `appsettings.json`, `.env` hoặc API key. Nếu MiMo không được cấu hình, chatbot vẫn hiển thị nhưng trả trạng thái không khả dụng và link điều hướng, không ném exception kỹ thuật ra giao diện.

## 6. API nội bộ

`POST /Chat/Ask`

- Content type: `application/x-www-form-urlencoded` với trường `Message` và `__RequestVerificationToken`.
- Yêu cầu session hợp lệ.
- `Message`: bắt buộc, tối đa 500 ký tự theo mặc định.
- Thành công: JSON `{ answer, suggestedActionLabel, suggestedActionUrl, isAvailable }`.
- Chưa đăng nhập: `401`.
- Validation lỗi: `400`.
- Đang xử lý request khác hoặc vượt limit: `429`.

Client không được gọi trực tiếp MiMo. API key chỉ được dùng trong `ChatbotService` phía server.

## 7. Checklist kiểm thử

### Smoke test trước khi bàn giao

1. Chạy `dotnet build FapWeb.csproj --no-restore`.
2. Chạy `dotnet test FapWeb.Tests/FapWeb.Tests.csproj --no-restore` để kiểm tra thống kê điểm danh, quyền phụ huynh và lọc lớp.
3. Khởi động app, đăng nhập bằng từng role có sẵn.
4. Mở widget ở desktop và màn hình hẹp; kiểm tra đóng bằng `Esc`, gửi bằng `Enter`, xuống dòng bằng `Shift+Enter`.
5. Hỏi một câu cho từng intent trong bảng ở phần 4. Kiểm tra chỉ có một link nội bộ đúng role.
6. Gửi câu hỏi tạo câu trả lời có Markdown (ví dụ yêu cầu “liệt kê chi tiết”); xác nhận không thấy `**`, heading hoặc bullet `*` thô.
7. Refresh trang hoặc logout/login; xác nhận lịch sử chat biến mất.
8. Với tài khoản Parent, hỏi “Con tôi đã có mặt và vắng bao nhiêu buổi?”, sau đó thử thêm “tháng này”, “tháng trước” và tên lớp. Đối chiếu tổng với `/Attendance/History`.

### Kiểm thử quyền bắt buộc

- Student không thể nhận lịch/điểm danh/học phí của student khác.
- Parent chỉ nhận dữ liệu của child đã liên kết.
- Parent có nhiều child nhận từng dòng riêng; khi nêu tên chỉ nhận child tương ứng.
- `PRESENT + ABSENT` bằng tổng buổi đã có kết quả; lịch quá khứ chưa điểm danh không bị tính là `ABSENT`.
- Teacher chỉ nhận dữ liệu của lớp mình phụ trách.
- Admin hỏi “liệt kê toàn bộ học phí/học sinh” phải nhận hướng dẫn, không nhận dữ liệu tổng hợp.

### Kiểm thử lỗi và giới hạn

- Bỏ `MiMo__ApiKey` hoặc đặt key sai: chatbot báo lỗi thân thiện và vẫn gợi ý trang liên quan.
- Mô phỏng timeout/API trả 4xx/5xx/JSON sai: không lộ chi tiết lỗi hay key.
- Gửi 11 câu trong 5 phút: request thứ 11 trả `429`.
- Gửi hai câu gần như đồng thời: request thứ hai bị chặn khi request đầu đang chạy.
- Gửi rỗng hoặc trên 500 ký tự: nhận validation `400`.

## 8. Xử lý sự cố thường gặp

| Hiện tượng | Nguyên nhân thường gặp | Cách kiểm tra/sửa |
| --- | --- | --- |
| Đăng nhập lỗi `Format of the initialization string...` | Connection string trong `.env` bị bọc nháy, hoặc format PostgreSQL không hợp lệ. | Kiểm tra `ConnectionStrings__DefaultConnection`; bộ nạp hiện đã bỏ cặp nháy bao ngoài. |
| Chatbot báo chưa cấu hình | Thiếu/sai `MiMo__ApiKey`. | Kiểm tra `.env`, restart app sau khi đổi key. |
| Chatbot trả link sai | Mapping trong `GetSuggestedAction` hoặc intent keyword chưa phù hợp. | Kiểm tra role trước, sau đó bổ sung keyword/mapping có test quyền. |
| Câu trả lời chứa Markdown thô | Model không tuân thủ prompt mới. | Kiểm tra `NormalizeAnswer`; bổ sung regex hẹp, không render model output bằng HTML. |
| Chatbot không xuất hiện | Chưa đăng nhập, layout không render partial, hoặc asset bị cache. | Đăng nhập và hard refresh; kiểm tra `_Layout.cshtml`, `_Chatbot.cshtml`, `chatbot.css`, `chatbot.js`. |

## 9. Quy tắc khi mở rộng

- Mọi chức năng ghi dữ liệu phải là phiên bản mới, có xác nhận UI riêng, authorization server-side và audit log; không tự thêm vào chatbot v1.
- Với intent mới, bổ sung keyword, query được filter theo role, link nội bộ allow-list và test role tương ứng.
- Giới hạn context, token và rate limit phải được giữ hoặc siết chặt khi mở rộng dữ liệu.
- Nếu thay provider/model, chỉ thay phần gọi HTTP/configuration; không cho provider tiếp cận database hoặc session trực tiếp.

## 10. Dữ liệu demo để test chatbot

`DemoDataSeeder` chỉ chạy khi `DemoData__SeedOnStartup=true` và có `DemoData__Password`. Nó tạo/cập nhật bốn account demo, hai lớp lập trình, lịch học, liên kết phụ huynh–học sinh, điểm danh, học phí và thông báo. Dữ liệu điểm danh gồm tám buổi quá khứ có cả `PRESENT`/`ABSENT` và một buổi chưa điểm danh để kiểm tra thống kê. Seeder idempotent, nên chạy lại trong cùng ngày không tạo bản ghi trùng theo các khóa nghiệp vụ.

Sau khi seed xong, đặt lại `DemoData__SeedOnStartup=false` rồi restart app để tránh seed ở mọi lần khởi động. Không bật cờ này ở production.
