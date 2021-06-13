using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Level.Skins
{
	public abstract class SkinCreationBase<T> : SkinBase where T : SkinInfoBase
	{
		public T skinInfo;

		public override void Enable() => skinInfo.skinEnable = true;
		public override void Disable() => skinInfo.skinEnable = false;
	}
}
