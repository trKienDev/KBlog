using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KBlog.Models
{
	public class User
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		[StringLength(100)]
		public string Name { get; set; } = string.Empty;

		[Required]
		[EmailAddress]
		[StringLength(255)]
		public string Email { get; set; } = string.Empty;

		[Required]
		public string Password_hash { get; set; } = string.Empty;

		[StringLength(500)]
		public string? ProfileImageUrl { get; set; } // Đổi tên từ Avatar_url để đồng bộ

		[Required]
		[StringLength(20)]
		public string Role { get; set; } = "user"; // Mặc định là user

		[Required]
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		[Required]
		public bool isDeleted { get; set; } = false; // Hỗ trợ xóa mềm
	}
}
