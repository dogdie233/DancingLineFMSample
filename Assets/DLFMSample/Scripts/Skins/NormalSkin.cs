using System;
using System.Collections.Generic;
using UnityEngine;

namespace Level.Skins
{
	public class NormalSkin : SkinCreationBase<NormalSkinInfo>
	{
		private Vector3 lastTurnPosition;
		private BoxCollider lineCollider;
		private Transform body;
		private Transform bodiesParent;
		private Transform particlesParent;

		public NormalSkin(Line line)
		{
			base.line = line;
			GameObject prefab = Resources.Load<GameObject>("LineSkins/Normal/Object");
			gameObject = GameObject.Instantiate(prefab, line.transform.position, line.transform.rotation, line.transform);  // 实例化皮肤go
			skinInfo = gameObject.GetComponent<NormalSkinInfo>();
			lineCollider = line.GetComponent<BoxCollider>();
			particlesParent = new GameObject("ParticlesParent").transform;
			bodiesParent = new GameObject("BodiesParent").transform;
		}

		public override void Update()
		{
			if (line.Moving)
			{
				if (body != null)
				{
					body.position = Vector3.Lerp(lastTurnPosition, line.transform.position, 0.5f);
					body.localScale = new Vector3(body.localScale.x, body.localScale.y, Vector3.Distance(lastTurnPosition, line.transform.position));
				}
			}
		}

		public override void Die(DeathCause deathCause)
		{
			switch (deathCause)
			{
				case DeathCause.Obstacle:
					if (skinInfo.hitObstacleParticles.Length != 0)
					{
						foreach (ParticleAttributes particle in skinInfo.hitObstacleParticles[UnityEngine.Random.Range(0, skinInfo.hitObstacleParticles.Length)].particles)
						{
							GameObject particleObj = GameObject.Instantiate(particle.obj, line.transform.position, Quaternion.Euler(Vector3.zero), particlesParent);
							GameObject.Destroy(particleObj, particle.alive);
						}
					}
					GameController.PlaySound(skinInfo.dieAudio);
					break;
			}
		}

		public override void EndFly()
		{
			CreateBody();
			// 生成跳跃粒子
			if (skinInfo.jumpEffectParticles.Length != 0)
			{
				foreach (ParticleAttributes particle in skinInfo.jumpEffectParticles[UnityEngine.Random.Range(0, skinInfo.jumpEffectParticles.Length)].particles)
				{
					GameObject particleObj = GameObject.Instantiate(particle.obj, line.transform.position, line.transform.rotation, particlesParent);
					GameObject.Destroy(particleObj, particle.alive);
				}
			}
		}

		public override void PickCrown(Crown crown, Line line) { }

		public override void PickDiamond(Diamond diamond, Line line) { }

		public override void Respawn()
		{
			GameObject.Destroy(particlesParent.gameObject);
			GameObject.Destroy(bodiesParent.gameObject);
			particlesParent = new GameObject("ParticlesParent").transform;
			bodiesParent = new GameObject("BodiesParent").transform;
		}

		public override void StartFly()
		{
			if (body != null)  //下落线身与地板对齐
			{
				body.localScale -= Vector3.forward * lineCollider.size.z;
				body.Translate(Vector3.back * lineCollider.size.z / 2, Space.Self);
				body = null;
			}
		}

		public override void Turn(bool foucs)
		{
			CreateBody();
		}

		public override void Win() { }

		private void CreateBody()
		{
			if (bodiesParent == null) { bodiesParent = new GameObject("Bodies").transform; }
			if (body != null && (line.transform.localScale.z / 2) < Vector3.Distance(lastTurnPosition, line.transform.position))
			{
				body.localScale += Vector3.forward * line.transform.localScale.z / 2;
				body.Translate(Vector3.forward * line.transform.localScale.z / 4, Space.Self);
			}
			body = GameObject.Instantiate(skinInfo.bodyPrefab, line.transform.position, line.transform.rotation, bodiesParent).transform;
			lastTurnPosition = line.transform.position;
		}
	}
}
