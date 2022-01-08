using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Level.Triggers
{
	[RequireComponent(typeof(Collider))]
	public class SetLineSpeed : MonoBehaviour
	{
		[SerializeField] private float speed;
		[SerializeField] private Line limit {  get; set; }

		private void OnTriggerEnter(Collider other)
		{
			if (other.CompareTag("Player") && (limit == null || other.GetComponent<Line>() == limit))
			{
				Line line = other.GetComponent<Line>();
				line.Speed = speed;
			}
		}
	}
}
