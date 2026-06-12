using System;
using System.Collections.Generic;
using FapWeb.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace FapWeb.Models.Data;

public partial class PostgresContext : DbContext
{
    public PostgresContext()
    {
    }

    public PostgresContext(DbContextOptions<PostgresContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AttendanceCheck> AttendanceChecks { get; set; }

    public virtual DbSet<AttendanceCheckStatus> AttendanceCheckStatuses { get; set; }

    public virtual DbSet<Class> Classes { get; set; }

    public virtual DbSet<ClassStudent> ClassStudents { get; set; }

    public virtual DbSet<FamilyRelationship> FamilyRelationships { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Schedule> Schedules { get; set; }

    public virtual DbSet<StudentGuardian> StudentGuardians { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<TransactionStatus> TransactionStatuses { get; set; }

    public virtual DbSet<TuitionFee> TuitionFees { get; set; }

    public virtual DbSet<TuitionFeeStatus> TuitionFeeStatuses { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql("Name=ConnectionStrings:DefaultConnection");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Removed Supabase specific schemas, enums, and extensions to work with local standard Postgres.

        modelBuilder.Entity<AttendanceCheck>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("attendance_check_pkey");

            entity.ToTable("attendance_check");

            entity.HasIndex(e => new { e.ScheduleId, e.StudentId }, "attendance_check_schedule_id_student_id_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CheckedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("checked_at");
            entity.Property(e => e.CheckedBy).HasColumnName("checked_by");
            entity.Property(e => e.Note).HasColumnName("note");
            entity.Property(e => e.ScheduleId).HasColumnName("schedule_id");
            entity.Property(e => e.StatusId).HasColumnName("status_id");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(d => d.CheckedByNavigation).WithMany(p => p.AttendanceCheckCheckedByNavigations)
                .HasForeignKey(d => d.CheckedBy)
                .HasConstraintName("attendance_check_checked_by_fkey");

            entity.HasOne(d => d.Schedule).WithMany(p => p.AttendanceChecks)
                .HasForeignKey(d => d.ScheduleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("attendance_check_schedule_id_fkey");

            entity.HasOne(d => d.Status).WithMany(p => p.AttendanceChecks)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("attendance_check_status_id_fkey");

            entity.HasOne(d => d.Student).WithMany(p => p.AttendanceCheckStudents)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("attendance_check_student_id_fkey");
        });

        modelBuilder.Entity<AttendanceCheckStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("attendance_check_status_pkey");

            entity.ToTable("attendance_check_status");

            entity.HasIndex(e => e.StatusName, "attendance_check_status_status_name_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.StatusName)
                .HasMaxLength(50)
                .HasColumnName("status_name");
        });

        modelBuilder.Entity<Class>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("classes_pkey");

            entity.ToTable("classes");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.ClassName)
                .HasMaxLength(100)
                .HasColumnName("class_name");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.MaxStudents)
                .HasDefaultValue(30)
                .HasColumnName("max_students");
            entity.Property(e => e.RoomName)
                .HasMaxLength(100)
                .HasColumnName("room_name");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.TeacherId).HasColumnName("teacher_id");
            entity.Property(e => e.TotalSessions).HasColumnName("total_sessions");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(d => d.Teacher).WithMany(p => p.Classes)
                .HasForeignKey(d => d.TeacherId)
                .HasConstraintName("classes_teacher_id_fkey");
        });

        modelBuilder.Entity<ClassStudent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("class_students_pkey");

            entity.ToTable("class_students");

            entity.HasIndex(e => new { e.ClassId, e.StudentId }, "class_students_class_id_student_id_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.ClassId).HasColumnName("class_id");
            entity.Property(e => e.EnrolledAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("enrolled_at");
            entity.Property(e => e.IsEnable).HasColumnName("is_enable");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'ACTIVE'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.StudentId).HasColumnName("student_id");

            entity.HasOne(d => d.Class).WithMany(p => p.ClassStudents)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("class_students_class_id_fkey");

            entity.HasOne(d => d.Student).WithMany(p => p.ClassStudents)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("class_students_student_id_fkey");
        });

        modelBuilder.Entity<FamilyRelationship>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("family_relationships_pkey");

            entity.ToTable("family_relationships");

            entity.HasIndex(e => e.RelationshipName, "family_relationships_relationship_name_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.RelationshipName)
                .HasMaxLength(50)
                .HasColumnName("relationship_name");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("notifications_pkey");

            entity.ToTable("notifications");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.IsRead)
                .HasDefaultValue(false)
                .HasColumnName("is_read");
            entity.Property(e => e.ReceiverId).HasColumnName("receiver_id");
            entity.Property(e => e.SenderId).HasColumnName("sender_id");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");

            entity.HasOne(d => d.Receiver).WithMany(p => p.NotificationReceivers)
                .HasForeignKey(d => d.ReceiverId)
                .HasConstraintName("notifications_receiver_id_fkey");

            entity.HasOne(d => d.Sender).WithMany(p => p.NotificationSenders)
                .HasForeignKey(d => d.SenderId)
                .HasConstraintName("notifications_sender_id_fkey");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("roles_pkey");

            entity.ToTable("roles");

            entity.HasIndex(e => e.RoleName, "roles_role_name_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.RoleName)
                .HasMaxLength(50)
                .HasColumnName("role_name");
        });

        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("schedules_pkey");

            entity.ToTable("schedules");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.ClassId).HasColumnName("class_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.EndTime).HasColumnName("end_time");
            entity.Property(e => e.ScheduleDate).HasColumnName("schedule_date");
            entity.Property(e => e.StartTime).HasColumnName("start_time");
            entity.Property(e => e.Topic)
                .HasMaxLength(255)
                .HasColumnName("topic");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(d => d.Class).WithMany(p => p.Schedules)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("schedules_class_id_fkey");
        });

        modelBuilder.Entity<StudentGuardian>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("student_guardians_pkey");

            entity.ToTable("student_guardians");

            entity.HasIndex(e => new { e.StudentId, e.GuardianId }, "student_guardians_student_id_guardian_id_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.GuardianId).HasColumnName("guardian_id");
            entity.Property(e => e.IsPrimary)
                .HasDefaultValue(false)
                .HasColumnName("is_primary");
            entity.Property(e => e.RelationshipId).HasColumnName("relationship_id");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(d => d.Guardian).WithMany(p => p.StudentGuardianGuardians)
                .HasForeignKey(d => d.GuardianId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("student_guardians_guardian_id_fkey");

            entity.HasOne(d => d.Relationship).WithMany(p => p.StudentGuardians)
                .HasForeignKey(d => d.RelationshipId)
                .HasConstraintName("student_guardians_relationship_id_fkey");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentGuardianStudents)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("student_guardians_student_id_fkey");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("transactions_pkey");

            entity.ToTable("transactions");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasPrecision(12, 2)
                .HasColumnName("amount");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.PaidBy).HasColumnName("paid_by");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(50)
                .HasColumnName("payment_method");
            entity.Property(e => e.StatusId).HasColumnName("status_id");
            entity.Property(e => e.TransactionCode)
                .HasMaxLength(255)
                .HasColumnName("transaction_code");
            entity.Property(e => e.TuitionFeeId).HasColumnName("tuition_fee_id");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(d => d.PaidByNavigation).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.PaidBy)
                .HasConstraintName("transactions_paid_by_fkey");

            entity.HasOne(d => d.Status).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.StatusId)
                .HasConstraintName("transactions_status_id_fkey");

            entity.HasOne(d => d.TuitionFee).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.TuitionFeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("transactions_tuition_fee_id_fkey");
        });

        modelBuilder.Entity<TransactionStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("transaction_status_pkey");

            entity.ToTable("transaction_status");

            entity.HasIndex(e => e.StatusName, "transaction_status_status_name_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.StatusName)
                .HasMaxLength(50)
                .HasColumnName("status_name");
        });

        modelBuilder.Entity<TuitionFee>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tuition_fee_pkey");

            entity.ToTable("tuition_fee");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.ClassId).HasColumnName("class_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DueDate).HasColumnName("due_date");
            entity.Property(e => e.PaidAmount)
                .HasPrecision(12, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("paid_amount");
            entity.Property(e => e.StatusId).HasColumnName("status_id");
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.TotalAmount)
                .HasPrecision(12, 2)
                .HasColumnName("total_amount");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(d => d.Class).WithMany(p => p.TuitionFees)
                .HasForeignKey(d => d.ClassId)
                .HasConstraintName("tuition_fee_class_id_fkey");

            entity.HasOne(d => d.Status).WithMany(p => p.TuitionFees)
                .HasForeignKey(d => d.StatusId)
                .HasConstraintName("tuition_fee_status_id_fkey");

            entity.HasOne(d => d.Student).WithMany(p => p.TuitionFees)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tuition_fee_student_id_fkey");
        });

        modelBuilder.Entity<TuitionFeeStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tuition_fee_status_pkey");

            entity.ToTable("tuition_fee_status");

            entity.HasIndex(e => e.StatusName, "tuition_fee_status_status_name_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.StatusName)
                .HasMaxLength(50)
                .HasColumnName("status_name");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "users_email_key").IsUnique();

            entity.HasIndex(e => e.Phone, "users_phone_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.AvatarUrl).HasColumnName("avatar_url");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(255)
                .HasColumnName("full_name");
            entity.Property(e => e.Gender)
                .HasMaxLength(10)
                .HasColumnName("gender");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("users_role_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
