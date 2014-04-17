using System;
using Microsoft.AspNet.Identity;

namespace Insight.Website.Models
{
	public class IdentityRole : IRole
	{
		public IdentityRole()
		{
			Id = Guid.NewGuid().ToString();
		}

		public string Id { get; set; }
		public string Name { get; set; }
	}
}