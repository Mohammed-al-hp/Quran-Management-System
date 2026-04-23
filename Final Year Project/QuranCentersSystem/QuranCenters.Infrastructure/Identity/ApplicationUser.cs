<<<<<<< HEAD:Final Year Project/QuranCentersSystem/QuranCenters.Infrastructure/Identity/ApplicationUser.cs
=======
﻿using Microsoft.AspNetCore.Identity;
>>>>>>> 7097dff658495d1d8b18b5d9bb1a3b0e942784ad:Final Year Project/QuranCentersSystem/QuranCentersSystem/Models/ApplicationUser.cs
using System.ComponentModel.DataAnnotations;

namespace QuranCenters.Infrastructure.Identity
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