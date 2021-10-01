using System;
using System.Collections.Generic;
using UnityEngine;

namespace Level.Triggers
{
	public class AirObstacle : MonoBehaviour
	{
		private void OnTriggerEnter(Collider other)
		{
			if (other.CompareTag("Player")) { other.GetComponent<Line>()?.Die(DeathCause.Air); }
		}
	}
}
