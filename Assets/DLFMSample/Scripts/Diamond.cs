using Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Level
{
	public class Diamond : MonoBehaviour, ICollection
	{
		public GameObject child;
		[SerializeField] private float speed;
		[SerializeField] private Line limit;
		[SerializeField] private ParticlesGroup[] particles;
		public new Animation animation;
		private Transform particlesParent;
		private bool picked = false;  // 被线吃
		private bool destroyed = false;  // 被摧毁

		public Line Limit { get => limit; set => limit = value; }
		public bool IsPicked
		{
			get { return picked; }
		}
		public bool IdDestroyed
		{
			get { return destroyed; }
		}
		public float Speed
		{
			get => speed;
			set
			{
				speed = value;
				foreach (AnimationState state in animation)
				{
					state.speed = speed;
				}
			}
		}

		public void Pick()
		{
			if (picked || destroyed) { return; }  //被吃了
			picked = true;
			destroyed = false;
			CollectionController.Instance.Add(this, GameController.Instance.LevelTime);
			child.SetActive(false);
			foreach (Line line in GameController.Instance.lines)
			{
				line.Skin.PickDiamond(this, line);
			}
			//粒子效果
			if (particles.Length == 0) { return; }
			particlesParent = particlesParent != null ? particlesParent : new GameObject("ParticlesGroup").transform;
			ParticlesGroup group = particles[Random.Range(0, particles.Length)];
			foreach (ParticleAttributes particle in group.particles)
			{
				Destroy(Instantiate(particle.obj, transform.position, particle.obj.transform.rotation, particlesParent), particle.alive);
			}
		}

		public void Recover()
		{
			picked = destroyed = false;
			child.SetActive(true);
			Destroy(particlesParent.gameObject);
		}

		private void Awake()
		{
			picked = false;
		}

		private void Start()
		{
			animation = animation ?? GetComponent<Animation>();
			animation.Play();
			foreach (AnimationState state in animation)
			{
				state.speed = speed;
			}
		}

		private void OnTriggerEnter(Collider other)
		{
			if (picked) { return; }
			if (other.CompareTag("Player"))
			{
				Line line = other.GetComponent<Line>();
				if (limit != null && line != limit) { return; }
				GameController.Instance.OnDiamondPick.Invoke(new DiamondPickEventArgs(line, this, true), args =>
				{
					line.Events.OnDiamondPicked.Invoke(args, args2 =>
					{
						if (!args2.canceled) { Pick(); }
					});
				});
			}
		}
	}
}
