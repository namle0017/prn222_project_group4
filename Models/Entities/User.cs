using System;
using System.Collections.Generic;

namespace FapWeb.Models.Entities;

public partial class User
{
    public Guid Id { get; set; }

    public int RoleId { get; set; }

    public string FullName { get; set; } = null!;

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string PasswordHash { get; set; } = null!;

    public string? AvatarUrl { get; set; }

    public string? Gender { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<AttendanceCheck> AttendanceCheckCheckedByNavigations { get; set; } = new List<AttendanceCheck>();

    public virtual ICollection<AttendanceCheck> AttendanceCheckStudents { get; set; } = new List<AttendanceCheck>();

    public virtual ICollection<ClassStudent> ClassStudents { get; set; } = new List<ClassStudent>();

    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();

    public virtual ICollection<Notification> NotificationReceivers { get; set; } = new List<Notification>();

    public virtual ICollection<Notification> NotificationSenders { get; set; } = new List<Notification>();

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<StudentGuardian> StudentGuardianGuardians { get; set; } = new List<StudentGuardian>();

    public virtual ICollection<StudentGuardian> StudentGuardianStudents { get; set; } = new List<StudentGuardian>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public virtual ICollection<TuitionFee> TuitionFees { get; set; } = new List<TuitionFee>();
}
