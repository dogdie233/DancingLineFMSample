using Event;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;

namespace Level
{
	public class LineRespawnAttributes
	{
		public Line line;
		public Vector3 position;
		public Vector3 way;
		public Vector3 nextWay;
		public bool controllable;

		public LineRespawnAttributes(Line line, Vector3 position, Vector3 way, Vector3 nextWay, bool controllable)
		{
			this.line = line;
			this.position = position;
			this.way = way;
			this.nextWay = nextWay;
			this.controllable = controllable;
		}
	}

	public class Crown : MonoBehaviour, ICollection
	{
		public GameObject child;
		public MeshRenderer crownIcon;
		[SerializeField] private float speed;
		public float time;
		public Line limit;
		public LineRespawnAttributes[] lineRespawnAttributes;
		public bool auto;
		private bool picked = false;
		private bool used = false;
		private Tweener tweener;
		private new Animation animation;

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
			picked = true;
			GameController.collections.Add(this);
			child.SetActive(false);
			tweener?.Kill();
			if (auto)
			{
				lineRespawnAttributes = new LineRespawnAttributes[GameController.lines.Count];
				for (int i = 0; i < GameController.lines.Count; i++)
				{
					lineRespawnAttributes[i].line = GameController.lines[i];
					lineRespawnAttributes[i].position = GameController.lines[i].transform.position;
					lineRespawnAttributes[i].way = GameController.lines[i].transform.localEulerAngles;
					lineRespawnAttributes[i].nextWay = GameController.lines[i].nextWay;
					lineRespawnAttributes[i].controllable = GameController.lines[i].controllable;
				}
			}
			foreach (Line line in GameController.lines)
			{
				line.skin.PickCrown(this, line);
			}
			tweener = crownIcon.material.DOFloat(1f, "_Fade", 1f);
		}

		public void Recover()
		{
			picked = used = false;
			child.SetActive(true);
			tweener?.Kill();
			tweener = null;
			crownIcon.material.SetFloat("_Fade", 0f);
		}

		public void Respawn()
		{
			if (!picked) { throw new Exception("Unable to respawn before pick."); }
			tweener?.Kill();
			if (!used) { tweener = crownIcon.material.DOFloat(0f, "_Fade", 1f); }
			used = true;
		}

		private void Awake()
		{
			picked = false;
			used = false;
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
				EventManager.onCrownPicked.Invoke(new CrownPickedEventArgs(line, this), (CrownPickedEventArgs e1) =>
				{
					line.events.onCrownPicked.Invoke(e1, (CrownPickedEventArgs e2) =>
					{
						if (!e1.canceled && !e2.canceled) { Pick(); }
					});
				});
			}
		}
	}
}