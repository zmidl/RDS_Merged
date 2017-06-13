using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDS.Models
{
	public class Language
	{
		public string Name { get; set; }
		public string FileName { get; set; }
		public bool IsUsed { get; set; }

		public string Serialize()
		{
			string isUsed = this.IsUsed ? Properties.Resources.One : Properties.Resources.Zero;

			return string.Join(Properties.Resources.Separator3, new string[] { this.Name, this.FileName, isUsed });
		}

		public Language(string stringFormat)
		{
			var items = stringFormat.Split(Properties.Resources.Separator3.ToCharArray()[0]);
			this.Name = items[0];
			this.FileName = items[1];
			this.IsUsed = items[2] == Properties.Resources.One ? true : false;
		}
	}
}
