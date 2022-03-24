using Event;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;

namespace Level
{
	public class Crown : MonoBehaviour, ICollection, ICheckpoint
	{
		public GameObject child;
		public MeshRenderer crownIcon;
		[SerializeField] private float speed;
		[SerializeField] private float time;
		[SerializeField] private Line limit;
		[SerializeField] private LineRespawnAttributes[] lineRespawnAttributes;
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
		public bool IsAvailable => IsPicked;

		public void Pick()
		{
			picked = true;
			CollectionController.Instance.Add(this, GameController.Instance.LevelTime);
			child.SetActive(false);
			tweener?.Kill();
			if (auto)
			{
				time = GameController.Instance.LevelTime;
				lineRespawnAttributes = new LineRespawnAttributes[GameController.Instance.lines.Count];
				for (int i = 0; i < GameController.Instance.lines.Count; i++)
				{
					lineRespawnAttributes[i] = new LineRespawnAttributes();
					lineRespawnAttributes[i].line = GameController.Instance.lines[i];
					lineRespawnAttributes[i].position = GameController.Instance.lines[i].transform.position;
					lineRespawnAttributes[i].way = GameController.Instance.lines[i].transform.localEulerAngles;
					lineRespawnAttributes[i].nextWay = GameController.Instance.lines[i].NextWay;
					lineRespawnAttributes[i].controllable = GameController.Instance.lines[i].Controllable;
				}
			}
			foreach (Line line in GameController.Instance.lines)
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
			if (!picked)
			{
				Debug.LogWarning("Unable to respawn by crown before pick.");
				return;
			}
			tweener?.Kill();
			if (!used) { tweener = crownIcon.material.DOFloat(0f, "_Fade", 1f); }
			foreach (LineRespawnAttributes lineRespawnAttributes in lineRespawnAttributes)
			{
				lineRespawnAttributes.line.RespawnByAttribute(lineRespawnAttributes);
			}
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
				GameController.Instance.OnCheckpointReach.Invoke(new CrownPickEventArgs(line, this), args =>
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