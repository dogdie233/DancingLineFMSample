using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Level.Skins
{
	public class SkinInfoBase : MonoBehaviour
	{
		public string displayName;
		public Texture2D previewTexture;
		public bool unlocked = true;
		public bool skinEnable = false;
	}
}
