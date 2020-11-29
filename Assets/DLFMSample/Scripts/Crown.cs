using Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;

namespace Level
{
	[System.Serializable]
	public struct LineRespawnAttributes
	{
		public Line line;
		public Vector3 positon;
		public Vector3 way;
		public Vector3 nextWay;
	}

	public class Crown : MonoBehaviour, ICollection
	{
		public GameObject child;
		public MeshRenderer crownIcon;
		public float speed;
		public float time;
		public Line limit;
		public LineRespawnAttributes[] lineRespawnAttributes;
		private bool picked = false;
		private bool used = false;
		private Tweener tweener;

		public void Pick()
		{
			picked = true;
			GameController.collections.Add(this);
			child.SetActive(false);
			tweener?.Kill();
			tweener = crownIcon.material.DOFloat(1f, "_Fade", 1f);
		}

		public void Recover()
		{
			picked = used = false;
			child.SetActive(true);
			tweener?.Kill();
			if (!used) { tweener = crownIcon.material.DOFloat(0f, "_Fade", 1f); }
		}

		public void Respawn()
		{
			tweener?.Kill();
			if (!used) { tweener = crownIcon.material.DOFloat(0f, "_Fade", 1f); }
			used = true;
		}

		private void Awake()
		{
			picked = false;
		}

		private void Start()
		{
			child.transform.Rotate(Vector3.up, Random.Range(0f, 360f), Space.Self);
		}

		private void Update()
		{
			child.transform.Rotate(Vector3.up, speed, Space.Self);
		}

		private void OnTriggerEnter(Collider other)
		{
			if (picked) { return; }
			if (other.CompareTag("Player"))
			{
				Line line = other.GetComponent<Line>();
				if (limit != null && line != limit) { return; }
				EventManager.onCrownPicked.Invoke(new CrownPickedEventArgs(line, this), (CrownPickedEventArgs e1) => {
					line.events.onCrownPicked.Invoke(new CrownPickedEventArgs(line, this), (CrownPickedEventArgs e2) => {
						if (!e1.canceled && !e2.canceled) { Pick(); }
					});
				});
			}
		}
	}
}