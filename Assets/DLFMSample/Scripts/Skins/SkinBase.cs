using System;
using System.Collections.Generic;
using UnityEngine;

namespace Level.Skins
{
	public abstract class SkinBase
	{
		public Line line;
		public GameObject gameObject;

		public abstract void Update();
		public abstract void Turn(bool foucs);
		public abstract void Die(DeathCause deathCause);
		public abstract void StartFly();
		public abstract void EndFly();
		public abstract void Respawn();
		public abstract void PickDiamond(Diamond diamond, Line line);  // who pick diamond
		public abstract void PickCrown(Crown crown, Line line);
		public abstract void Win();
		public abstract void Enable();
		public abstract void Disable();
	}
}
