using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Text;

namespace ZombiePanic
{
	public class MediaPlayer
	{

		public Sound? CurrentMusic = null;

		public string[] ListofMusic =
		{
			"musicambiant01.ambiant", 
			"musicambiant02.ambiant",
			"musicambiant03.ambiant",
			"musicambiant04.ambiant",
			"musicambiant05.ambiant",
			"musicambiant06.ambiant",
			"musicambiant07.ambiant",
			"musicambiant08.ambiant",
			"musicambiant09.ambiant",
			"musicambiant10.ambiant",
			"musicambiant11.ambiant",
			"musicambiant12.ambiant",
			"musicambiant13.ambiant",
			"musicambiant14.ambiant",
			"musicambiant15.ambiant",
			"musicambiant16.ambiant",
			"musicambiant18.ambiant",
			"musicambiant19.ambiant",
			"musicambiant20.ambiant",
			"musicambiant21.ambiant",
			"musicambiant22.ambiant",
			"musicambiant23.ambiant"
		};
		
		public MediaPlayer()
		{
			StartSound();
		}

		public void StartSound()
		{
			
		}

		public void ChangeSound()
		{
			
		}
	}
}
