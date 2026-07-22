-- Role refactor 2026: teacher/parent permission changes + fee approval + schedule edit limit
-- Chạy 1 lần trên Postgres. An toàn với dữ liệu cũ (default giữ hành vi hiện tại).

-- tuition_fee: nội dung khoản phí + loại phí + trạng thái duyệt + người tạo
ALTER TABLE tuition_fee ADD COLUMN IF NOT EXISTS description     varchar(255);
ALTER TABLE tuition_fee ADD COLUMN IF NOT EXISTS fee_type        varchar(20)  NOT NULL DEFAULT 'TUITION';
ALTER TABLE tuition_fee ADD COLUMN IF NOT EXISTS approval_status varchar(20)  NOT NULL DEFAULT 'APPROVED';
ALTER TABLE tuition_fee ADD COLUMN IF NOT EXISTS created_by      uuid REFERENCES users(id);

-- schedules: đếm số lần sửa lịch trong tháng (giới hạn 3 lần/tháng/buổi cho teacher)
ALTER TABLE schedules ADD COLUMN IF NOT EXISTS edit_count       int NOT NULL DEFAULT 0;
ALTER TABLE schedules ADD COLUMN IF NOT EXISTS edit_count_month date;
