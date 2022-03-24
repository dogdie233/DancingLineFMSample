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

		/// <summary>
		/// 生成组里面的所有粒子并启用
		/// </summary>
		/// <param name="parent">生成的粒子的父变换组件</param>
		/// <param name="active">生成时设置的激活状态</param>
		/// <param name="destroyOnTime">是否在到达alive时间时销毁</param>
		public void PlayAll(Transform parent, bool active = true, bool destroyOnTime = true)
        {
			foreach (var particle in particles)
            {
				var go = GameObject.Instantiate(particle.obj, parent);
				go.SetActive(active);
				if (destroyOnTime) { GameObject.Destroy(go, particle.alive); }
            }
        }

		/// <summary>
		/// 生成组里面的所有粒子并启用
		/// </summary>
		/// <param name="position">生成的粒子的位置</param>
		/// <param name="parent">生成的粒子的父变换组件</param>
		/// <param name="active">生成时设置的激活状态</param>
		/// <param name="destroyOnTime">是否在到达alive时间时销毁</param>
		public void PlayAll(Vector3 position, Transform parent, bool active = true, bool destroyOnTime = true)
		{
			foreach (var particle in particles)
			{
				var go = GameObject.Instantiate(particle.obj, position, particle.obj.transform.rotation, parent);
				go.SetActive(active);
				if (destroyOnTime) { GameObject.Destroy(go, particle.alive); }
			}
		}
	}
}
