using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace QuranCentersSystem.Models
{
	// الحل: يجب أن يرث الكلاس من IdentityUser وليس من نفسه
	public class ApplicationUser : IdentityUser
	{
		[Required]
		[Display(Name = "نوع الحساب (الدور)")]
		public string Role { get; set; }

		[Display(Name = "حالة الحساب")]
		public bool IsActive { get; set; } = true;
	}
}