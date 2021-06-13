using System;
using System.Collections.Generic;
using UnityEngine;

namespace Level.Skins
{
	public class NormalSkinInfo : SkinInfoBase
	{
		public GameObject bodyPrefab;
		public AudioClip dieAudio;
		public ParticlesGroup[] jumpEffectParticles;
		public ParticlesGroup[] hitObstacleParticles;
	}
}
