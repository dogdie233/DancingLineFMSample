using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Level.Triggers
{
    public class Teleport : MonoBehaviour
    {
        public Vector3 offset;
		public Line limit;
		public CameraFollower cameraFollower;

		private void OnTriggerEnter(Collider other)
		{
			if (other.CompareTag("Player") && (limit == null || other.GetComponent<Line>() == limit))
			{
				Line line = other.GetComponent<Line>();
				line.GoOffset(offset);
				if (cameraFollower != null)
				{
					cameraFollower.FollowPoint += offset;
				}
			}
		}
	}
}
