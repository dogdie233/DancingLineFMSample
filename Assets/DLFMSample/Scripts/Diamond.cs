using Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Level
{
	[System.Serializable]
	public class ParticleAttributes
	{
		public GameObject obj;
		public float alive;
	}

	[System.Serializable]
	public class ParticlesGroup
	{
		public ParticleAttributes[] particles;
	}

	public class Diamond : MonoBehaviour, ICollect
	{
		public GameObject child;
		public float speed;
		public Line limit;
		public ParticlesGroup[] particles;
		private Transform particlesParent;
		private bool picked = false;  // 被线吃
		private bool destroyed = false;  // 被摧毁

		public bool IsPicked
		{
			get { return picked; }
		}

		public bool IdDestroyed
		{
			get { return destroyed; }
		}

		public void Pick() { Pick(true); }

		public void Pick(bool lineEat)
		{
			if (picked || destroyed) { return; }  //被吃了
			if (lineEat) { GameController.diamondNumber++; }
			child.SetActive(false);
			//粒子效果
			if (particles.Length == 0) { return; }
			particlesParent = particlesParent == null ? particlesParent : new GameObject("ParticlesGroup").transform;
			ParticlesGroup group = particles[Random.Range(0, particles.Length)];
			foreach (ParticleAttributes particle in group.particles)
			{
				Destroy(Instantiate(particle.obj, transform.position, particle.obj.transform.rotation, particlesParent), particle.alive);
			}
		}

		public void Recover()
		{
			picked = destroyed = false;
			if (picked) { GameController.diamondNumber--; }
			child.SetActive(true);
			Destroy(particlesParent);
		}

		private void Start()
		{
			child.transform.Rotate(Vector3.up, Random.Range(0f, 360f), Space.Self) ;
		}

		private void Update()
		{
			child.transform.Rotate(Vector3.up, speed, Space.Self);
		}

		private DiamondPickedEventArgs OnDiamondPicked(DiamondPickedEventArgs e)
		{
			if (e.diamond != this) { return e;}
			if (!e.canceled) { Pick(true); }
			return e;
		}

		private void OnTriggerEnter(Collider other)
		{
			if (other.CompareTag("Player"))
			{
				Line line = other.GetComponent<Line>();
				if (limit != null && line != limit) { return; }
				line.events.onDiamondPicked.Invoke(new DiamondPickedEventArgs(line, this, true), (DiamondPickedEventArgs e) => {
					if (!e.canceled) { Pick(true); }
				});
			}
		}
	}
}
