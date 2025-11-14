# Hướng Dẫn Chức Năng - Hệ Thống Quản Lý Nhân Sự (HRM)

## Tổng Quan

Hệ thống Quản Lý Nhân Sự (HRM) là một ứng dụng web được xây dựng với kiến trúc API-Client tách biệt, hỗ trợ quản lý nhân viên, chấm công, yêu cầu nghỉ phép, và đánh giá hiệu suất làm việc.

## Các Vai Trò Trong Hệ Thống

### 1. Admin (Quản trị viên)
- Quyền cao nhất trong hệ thống
- Quản lý toàn bộ người dùng, phòng ban, chính sách
- Xem báo cáo tổng hợp và đánh giá hiệu suất

### 2. Manager (Quản lý)
- Quản lý phòng ban của mình
- Duyệt/yêu cầu nghỉ phép của nhân viên
- Xem thống kê đội nhóm
- Chấm công cá nhân

### 3. Employee (Nhân viên)
- Xem thông tin cá nhân
- Chấm công hàng ngày
- Tạo yêu cầu nghỉ phép/tăng ca
- Xem lịch sử chấm công và thu nhập

---

## Các Chức Năng Chính

### 1. Quản Lý Người Dùng (Admin)

#### 1.1. Danh Sách Người Dùng
**Mục đích**: Xem và quản lý tất cả người dùng trong hệ thống

**Cách sử dụng**:
- Vào menu "User Management"
- Xem danh sách người dùng được nhóm theo vai trò (Admin, Manager, Employee)
- Mỗi vai trò có 2 nhóm: Active (đang hoạt động) và Banned (đã bị khóa)

**Các thao tác**:
- **Ban/Unban**: Khóa hoặc mở khóa tài khoản người dùng
  - Click nút "Ban" hoặc "Unban"
  - Xác nhận trong popup
  - Lưu ý: Không thể ban Manager đang quản lý phòng ban
- **Reset Password**: Đặt lại mật khẩu
  - Click "Reset Password"
  - Mật khẩu mới sẽ hiển thị (chỉ hiển thị 1 lần)
- **Tạo Người Dùng Mới**: Thêm nhân viên mới vào hệ thống
  - Click "Create New User"
  - Điền thông tin: Tên, Vai trò, Cấp độ, Địa chỉ, Phòng ban
  - Hệ thống tự động tạo username và password

#### 1.2. Quản Lý Phòng Ban
**Mục đích**: Tạo và quản lý các phòng ban trong công ty

**Các thao tác**:
- **Tạo Phòng Ban Mới**:
  - Click "Create Department"
  - Nhập tên phòng ban
  - Chọn nhân viên để làm quản lý (nhân viên này sẽ được thăng lên Manager Level 1)
- **Đổi Tên Phòng Ban**: Click vào tên phòng ban để sửa trực tiếp
- **Thay Đổi Quản Lý**:
  - Vào trang "Change Manager"
  - Tìm và chọn nhân viên mới
  - Quản lý cũ sẽ bị hạ xuống Employee Level 3
- **Xóa Phòng Ban**: Chỉ xóa được khi phòng ban không còn nhân viên nào

#### 1.3. Quản Lý Chính Sách
**Mục đích**: Thiết lập các quy định về chấm công và nghỉ phép

**Các thiết lập**:
- **Ngày nghỉ phép tối đa**: Số ngày nghỉ phép tối đa trong năm
- **Giờ tăng ca tối đa**: Số giờ tăng ca tối đa mỗi tháng
- **Giờ làm việc**: Giờ bắt đầu và kết thúc ca làm việc
- **Ngưỡng trễ**: Số phút được phép trễ mà không bị trừ lương
- **Tỷ lệ trừ lương**: % lương bị trừ khi trễ hoặc về sớm

#### 1.4. Quản Lý Mức Lương
**Mục đích**: Thiết lập mức lương cơ bản cho từng vai trò và cấp độ

**Cấu trúc lương**:
- **Admin**: Level 1 - 50,000,000 VNĐ
- **Manager**: 
  - Level 1 - 30,000,000 VNĐ
  - Level 2 - 40,000,000 VNĐ
  - Level 3 - 50,000,000 VNĐ
- **Employee**:
  - Level 1 - 15,000,000 VNĐ
  - Level 2 - 20,000,000 VNĐ
  - Level 3 - 25,000,000 VNĐ

**Cách sử dụng**: Click vào mức lương để chỉnh sửa (giới hạn từ 1,000,000 đến 1,000,000,000 VNĐ)

---

### 2. Chấm Công (Manager & Employee)

#### 2.1. Check-In (Chấm công vào)
**Mục đích**: Ghi nhận thời gian đến làm việc

**Cách sử dụng**:
- Vào Dashboard
- Click nút "Check In Now" trong card "Check-In Status"
- Hệ thống sẽ kiểm tra:
  - Nếu đến đúng giờ hoặc trễ trong ngưỡng cho phép → Hiển thị "On-time" (màu xanh)
  - Nếu trễ quá ngưỡng → Hiển thị "Late" (màu vàng) và bị trừ lương

**Lưu ý**:
- Chỉ được check-in 1 lần mỗi ngày
- Thời gian check-in được ghi nhận chính xác

#### 2.2. Check-Out (Chấm công ra)
**Mục đích**: Ghi nhận thời gian rời khỏi nơi làm việc

**Cách sử dụng**:
- Phải check-in trước
- Click nút "Check Out Now" trong card "Check-Out Status"
- Hệ thống sẽ kiểm tra:
  - Nếu về đúng giờ hoặc sau giờ quy định → Hiển thị "On-time" (màu xanh)
  - Nếu về sớm quá ngưỡng → Hiển thị "Early" (màu vàng) và bị trừ lương

#### 2.3. Lịch Sử Chấm Công
**Mục đích**: Xem lại lịch sử chấm công của bản thân

**Các tính năng**:
- **Xem theo kỳ**: Chọn "Last Week" (7 ngày) hoặc "Last Month" (30 ngày)
- **Tìm kiếm theo ngày**: Nhập ngày cụ thể để xem
- **Thông tin hiển thị**:
  - Ngày chấm công
  - Giờ check-in và check-out
  - Trạng thái: Present (Đúng giờ), Late (Trễ), LeaveEarly (Về sớm), Absent (Vắng)
  - Số giờ làm việc (tính từ check-in đến check-out)

---

### 3. Quản Lý Yêu Cầu (Requests)

#### 3.1. Tạo Yêu Cầu (Employee)
**Mục đích**: Gửi yêu cầu nghỉ phép, từ chức, hoặc tăng ca

**Các loại yêu cầu**:
- **Leave (Nghỉ phép)**: Yêu cầu nghỉ làm trong khoảng thời gian
- **Resignation (Từ chức)**: Thông báo nghỉ việc
- **Overtime (Tăng ca)**: Yêu cầu làm thêm giờ

**Cách tạo**:
1. Vào trang "My Requests"
2. Click "Create New Request"
3. Điền thông tin:
   - Loại yêu cầu
   - Ngày bắt đầu và kết thúc
   - Nội dung yêu cầu
4. Xác nhận và gửi

**Lưu ý**: Yêu cầu sẽ tự động được gán vào phòng ban của nhân viên

#### 3.2. Duyệt Yêu Cầu (Manager)
**Mục đích**: Xem và phê duyệt/yêu cầu từ nhân viên trong phòng ban

**Các tính năng**:
- **Lọc theo trạng thái**: Pending (Chờ duyệt), Processed (Đã xử lý)
- **Lọc theo loại**: All, Leave, Resignation, Overtime
- **Duyệt/Từ chối**:
  - Click "Approve" hoặc "Reject"
  - Xác nhận trong popup
  - Chỉ có thể xử lý yêu cầu ở trạng thái Pending

**Thông tin hiển thị**:
- ID yêu cầu
- Tên nhân viên
- Loại yêu cầu
- Ngày bắt đầu và kết thúc
- Nội dung
- Trạng thái hiện tại

---

### 4. Thống Kê Thu Nhập

#### 4.1. Thu Nhập Hôm Nay
**Mục đích**: Xem lương được tính cho ngày làm việc hôm nay

**Thông tin hiển thị**:
- **Lương cơ bản (tháng)**: Mức lương theo cấp độ
- **Lương ngày**: Lương tháng / 22 ngày làm việc
- **Trạng thái chấm công**: Present, Late, LeaveEarly, hoặc Absent
- **Số tiền bị trừ** (nếu có):
  - Trễ: Trừ % theo chính sách
  - Về sớm: Trừ % theo chính sách
  - Vắng: Trừ 100% lương ngày
- **Lương cuối cùng hôm nay**: Lương ngày - số tiền bị trừ

#### 4.2. Thu Nhập Tháng Này
**Mục đích**: Xem tổng thu nhập và các khoản khấu trừ trong tháng

**Thông tin hiển thị**:
- **Lương cơ bản**: Mức lương theo cấp độ
- **Tổng ngày làm việc**: Số ngày đã chấm công trong tháng
- **Phân tích chấm công**:
  - Số ngày đúng giờ (Present)
  - Số ngày trễ (Late)
  - Số ngày về sớm (LeaveEarly)
  - Số ngày vắng (Absent)
- **Tổng số tiền bị trừ**: Tổng các khoản khấu trừ từ tất cả các ngày vi phạm
- **Lương cuối cùng**: Lương cơ bản - tổng số tiền bị trừ

---

### 5. Đánh Giá Hiệu Suất (Admin)

#### 5.1. Đánh Giá Tổng Quan (General Evaluation)
**Mục đích**: Phân tích hiệu suất làm việc của toàn bộ nhân viên trong 7 ngày gần nhất

**Cách sử dụng**:
1. Click nút AI (góc dưới bên phải Dashboard)
2. Chọn "General Evaluation"
3. Đợi hệ thống phân tích (có thể mất 30-60 giây)
4. Xem báo cáo chi tiết

**Nội dung báo cáo**:
- Tóm tắt điều hành
- Phân tích hiệu suất tổng thể
- Điểm mạnh và điểm yếu
- Top performers (những người làm việc tốt nhất)
- Nhân viên cần chú ý
- Tóm tắt theo phòng ban
- Khuyến nghị

**Lưu ý**: Báo cáo được tạo bằng AI, có thể mất thời gian xử lý

#### 5.2. Đánh Giá Tăng Lương (Level Promotion)
**Mục đích**: Đánh giá và đề xuất tăng cấp độ lương cho nhân viên/quản lý

**Cách sử dụng**:
1. Click nút AI → Chọn "Level Promotion"
2. Chọn "Evaluate Managers" hoặc "Evaluate Employees"
3. Đợi hệ thống phân tích (30-60 giây)
4. Xem kết quả:
   - **Nếu không có đề xuất**: Hiển thị thông báo "No Promotion Recommendation"
   - **Nếu có đề xuất**: Hiển thị card với:
     - Tên người được đề xuất
     - Cấp độ hiện tại → Cấp độ đề xuất
     - Lý do đề xuất (do AI phân tích)

**Tiêu chuẩn đánh giá** (tất cả phải đạt):
- Tỷ lệ chấm công > 85%
- Số ngày trễ ≤ 2 ngày (trong 30 ngày)
- Số giờ làm việc trung bình ≥ 7 giờ/ngày
- Tỷ lệ yêu cầu được duyệt > 80%

**Các thao tác**:
- **Accept (Chấp nhận)**: 
  - Click nút "Accept"
  - Xác nhận trong popup
  - Hệ thống tự động tăng 1 cấp độ cho người được đề xuất
  - Chỉ được tăng 1 cấp độ mỗi lần (tối đa Level 3)
- **Reject (Từ chối)**: 
  - Click nút "Reject"
  - Đóng modal, không có thay đổi gì

**Lưu ý**:
- Chỉ đánh giá những người có cấp độ < 3 (chưa đạt mức tối đa)
- Đánh giá dựa trên dữ liệu 30 ngày gần nhất
- Approval rate được tính trên TẤT CẢ yêu cầu (không chỉ 30 ngày) để phản ánh đúng hành vi

---

### 6. Quản Lý Phòng Ban (Manager)

#### 6.1. Danh Sách Nhân Viên
**Mục đích**: Xem danh sách nhân viên trong phòng ban

**Thông tin hiển thị**:
- ID nhân viên
- Tên đầy đủ
- Cấp độ (Level 1, 2, hoặc 3)
- Trạng thái chấm công hôm nay:
  - Present (Xanh lá) - Đã chấm công đúng giờ
  - Late (Vàng) - Đã chấm công nhưng trễ
  - On Leave (Xanh dương) - Đang nghỉ phép
  - Absent (Đỏ) - Vắng mặt
  - Not checked in (Xám) - Chưa chấm công

#### 6.2. Thống Kê Đội Nhóm
**Mục đích**: Xem các chỉ số tổng hợp của phòng ban

**Các thống kê**:
- **Tổng số thành viên**: Tổng số nhân viên trong phòng ban
- **Phân bổ theo cấp độ**: Số lượng nhân viên ở mỗi cấp độ
- **Yêu cầu hôm nay**: Tổng số và số lượng đang chờ duyệt
- **Chấm công hôm nay**: Số người đã chấm công và tỷ lệ
- **Thống kê yêu cầu**: Phân tích theo kỳ (Hôm nay/Tuần này)
- **Thống kê chấm công**: Phân tích theo kỳ
- **Người vắng mặt nhiều nhất**: Hiển thị nhân viên có số ngày vắng nhiều nhất

---

### 7. Quản Lý Hồ Sơ Cá Nhân

#### 7.1. Xem và Chỉnh Sửa Thông Tin
**Mục đích**: Cập nhật thông tin cá nhân

**Các thông tin có thể chỉnh sửa**:
- **Tên đầy đủ**: Tên hiển thị trong hệ thống
- **Địa chỉ**: Địa chỉ liên hệ

**Các thông tin chỉ xem**:
- **Username**: Tên đăng nhập (không thể thay đổi)
- **Avatar**: Ảnh đại diện (hiện tại chỉ hiển thị chữ cái đầu)

**Cách sử dụng**:
1. Vào menu "Profile"
2. Click "Edit" để chỉnh sửa
3. Điền thông tin mới
4. Click "Save" để lưu

---

## Các Use Case Thường Gặp

### Use Case 1: Nhân viên mới vào làm
1. Admin tạo tài khoản mới trong "User Management"
2. Hệ thống tự động tạo username và password
3. Admin gán nhân viên vào phòng ban
4. Nhân viên đăng nhập và đổi mật khẩu (nếu cần)
5. Nhân viên bắt đầu chấm công hàng ngày

### Use Case 2: Nhân viên xin nghỉ phép
1. Nhân viên vào "My Requests" → "Create New Request"
2. Chọn loại "Leave", điền ngày và lý do
3. Yêu cầu được gửi đến Manager của phòng ban
4. Manager vào "Requests" → Xem yêu cầu → Approve/Reject
5. Nhân viên nhận thông báo kết quả

### Use Case 3: Đánh giá tăng lương
1. Admin vào Dashboard → Click nút AI
2. Chọn "Level Promotion" → "Evaluate Employees"
3. Hệ thống phân tích dữ liệu 30 ngày
4. Nếu có đề xuất:
   - Admin xem lý do đề xuất
   - Quyết định Accept hoặc Reject
   - Nếu Accept, nhân viên được tăng 1 cấp độ lương

### Use Case 4: Tính lương tháng
1. Hệ thống tự động tính lương dựa trên:
   - Mức lương cơ bản (theo Role và Level)
   - Lịch sử chấm công trong tháng
   - Các khoản khấu trừ (trễ, về sớm, vắng)
2. Nhân viên/Manager vào "Income Statistics" → "This Month"
3. Xem chi tiết:
   - Số ngày làm việc
   - Số ngày vi phạm
   - Tổng số tiền bị trừ
   - Lương cuối cùng

### Use Case 5: Manager quản lý đội nhóm
1. Manager vào Dashboard xem tổng quan:
   - Số thành viên
   - Yêu cầu đang chờ duyệt
   - Tình hình chấm công
2. Vào "Employees" để xem danh sách và trạng thái chấm công
3. Vào "Requests" để duyệt yêu cầu
4. Vào "Attendance" để xem lịch sử chấm công của bản thân

---

## Lưu Ý Quan Trọng

### Về Chấm Công
- Phải check-in trước khi check-out
- Chỉ được chấm công 1 lần mỗi ngày
- Thời gian chấm công được ghi nhận chính xác theo server

### Về Yêu Cầu
- Yêu cầu tự động được gán vào phòng ban của nhân viên
- Chỉ Manager của phòng ban mới có thể duyệt
- Yêu cầu ở trạng thái Pending mới có thể được xử lý

### Về Tăng Lương
- Chỉ được tăng 1 cấp độ mỗi lần
- Tối đa Level 3
- Đánh giá dựa trên dữ liệu 30 ngày
- Approval rate tính trên TẤT CẢ yêu cầu (không chỉ 30 ngày)

### Về Bảo Mật
- Mỗi vai trò chỉ có quyền truy cập các chức năng phù hợp
- Admin có quyền cao nhất
- Manager chỉ quản lý phòng ban của mình
- Employee chỉ xem và quản lý thông tin cá nhân

---

## Hỗ Trợ

Nếu gặp vấn đề hoặc cần hỗ trợ, vui lòng liên hệ Admin của hệ thống.

