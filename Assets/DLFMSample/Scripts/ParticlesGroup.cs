using System;
using System.Collections.Generic;
using UnityEngine;

namespace Level
{
	[Serializable]
	public struct ParticleAttributes : IEquatable<ParticleAttributes>
	{
		public GameObject obj;
		public float alive;

		public override bool Equals(object other)
		{
			if (!(other is ParticleAttributes)) { return false; }
			return Equals((ParticleAttributes)other);
		}
		public bool Equals(ParticleAttributes other) => obj == other.obj && alive == other.alive;
		public static bool operator ==(ParticleAttributes i1, ParticleAttributes i2) => i1.Equals(i2);
		public static bool operator !=(ParticleAttributes i1, ParticleAttributes i2) => !i1.Equals(i2);
		public override int GetHashCode()
		{
			int hash = 17;
			hash = hash * 31 + obj.GetHashCode();
			hash = hash * 31 + alive.GetHashCode();
			return hash;
		}
		public override string ToString() => $"(obj name: {obj.name}, alive: {alive})";
	}

	[Serializable]
	public struct ParticlesGroup : IEquatable<ParticlesGroup>
	{
		public ParticleAttributes[] particles;

		public override bool Equals(object other)
		{
			if (!(other is ParticlesGroup)) { return false; }
			return Equals((ParticlesGroup)other);
		}
		public bool Equals(ParticlesGroup other) => particles == other.particles;
		public static bool operator ==(ParticlesGroup i1, ParticlesGroup i2) => i1.Equals(i2);
		public static bool operator !=(ParticlesGroup i1, ParticlesGroup i2) => !i1.Equals(i2);
		public override int GetHashCode() => particles.GetHashCode();
		public override string ToString() => $"ParticlesGroup, length: {particles.Length}";
	}
}
