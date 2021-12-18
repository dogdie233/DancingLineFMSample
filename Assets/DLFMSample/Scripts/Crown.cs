using Event;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;
using System.Linq;

namespace Level
{
	[Serializable]
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

		public LineRespawnAttributes() { }
	}

	[Serializable]
	public class CameraFollowerRespawnAttributes
	{
		public CameraFollower follower;
		public bool enabled;
		public Vector2 rotation;
		public float distance;
		public Vector3 pivot;

		public CameraFollowerRespawnAttributes(CameraFollower follower, bool enabled, Vector2 rotation, float distance, Vector3 pivot)
		{
			this.follower = follower;
			this.enabled = enabled;
			this.rotation = rotation;
			this.distance = distance;
			this.pivot = pivot;
		}

		public CameraFollowerRespawnAttributes() { }
	}

	public class Crown : MonoBehaviour, ICollection
	{
		public GameObject child;
		public MeshRenderer crownIcon;
		[SerializeField] private float speed;
		[SerializeField] private float time;
		[SerializeField] private Line limit;
		[SerializeField] private LineRespawnAttributes[] lineRespawnAttributes;
		[SerializeField] private CameraFollowerRespawnAttributes[] cameraFollowerRespawnAttributes;
		[SerializeField] private bool auto;
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
		public float Time { get => time; set => time = value; }
		public Line Limit { get => limit; set => limit = value; }
		public bool IsPicked { get => picked; set => picked = value; }
		public bool IsUsed { get => used; set => used = value; }
		public bool IsAuto { get => auto; set => auto = value; }
		public LineRespawnAttributes[] LineRespawnAttributes { get => lineRespawnAttributes; set => lineRespawnAttributes = value; }
		public CameraFollowerRespawnAttributes[] CameraFollowerRespawnAttributes { get => cameraFollowerRespawnAttributes; set => cameraFollowerRespawnAttributes = value; }


		public void Pick()
		{
			picked = true;
			GameController.collections.Add(this);
			child.SetActive(false);
			tweener?.Kill();
			if (auto)
			{
				time = BGMController.Time;
				lineRespawnAttributes = new LineRespawnAttributes[GameController.lines.Count];
				for (int i = 0; i < GameController.lines.Count; i++)
				{
					lineRespawnAttributes[i] = new LineRespawnAttributes();
					lineRespawnAttributes[i].line = GameController.lines[i];
					lineRespawnAttributes[i].position = GameController.lines[i].transform.position;
					lineRespawnAttributes[i].way = GameController.lines[i].transform.localEulerAngles;
					lineRespawnAttributes[i].nextWay = GameController.lines[i].NextWay;
					lineRespawnAttributes[i].controllable = GameController.lines[i].Controllable;
				}
				cameraFollowerRespawnAttributes = Camera.allCameras
					.Select(c => (c, c.GetComponent<CameraFollower>()))
					.Where(az => az.Item2 != null)
					.Select(az => new CameraFollowerRespawnAttributes(az.Item2, az.Item2.enabled, az.Item2.arm.localEulerAngles, Mathf.Abs(az.c.transform.localPosition.z), az.Item2.pivotOffset))
					.ToArray();
			}
			foreach (Line line in GameController.lines)
			{
				line.Skin.PickCrown(this, line);
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
				GameController.OnCrownPick.Invoke(new CrownPickEventArgs(line, this), args =>
				{
					line.Events.OnCrownPicked.Invoke(args, args2 =>
					{
						if (!args2.canceled) { Pick(); }
					});
				});
			}
		}
	}
}