using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FapWeb.Models.Dtos.UserManagementDtos
{
    public class GuardianLinkFormDto
    {
        [Required(ErrorMessage = "Học sinh là bắt buộc.")]
        [Display(Name = "Học sinh")]
        public Guid StudentId { get; set; }

        [Required(ErrorMessage = "Phụ huynh là bắt buộc.")]
        [Display(Name = "Phụ huynh")]
        public Guid GuardianId { get; set; }

        [Display(Name = "Quan hệ")]
        public int? RelationshipId { get; set; }

        [Display(Name = "Người giám hộ chính")]
        public bool IsPrimary { get; set; }

        public List<SelectListItem> StudentOptions { get; set; } = new();

        public List<SelectListItem> GuardianOptions { get; set; } = new();

        public List<SelectListItem> RelationshipOptions { get; set; } = new();
    }
}
