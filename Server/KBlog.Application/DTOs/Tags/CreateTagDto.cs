﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KBlog.Application.DTOs.Tags
{
	public class CreateTagDto
	{
		[Required]
		[StringLength(100)]
		public string Name { get; set; }  = string.Empty;
	}
}
