using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Level.Triggers
{
	public class CameraRotater : MonoBehaviour
	{
		public CameraFollower cameraFollower;
		public Vector2 rotation;
		public float distance;
		public float duration;
		public AnimationCurve curve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 0.0f, 1.0f, 1.0f), new Keyframe(1.0f, 1.0f, 1.0f, 1.0f) });

		void OnTriggerEnter(Collider other)
		{
			if (other.CompareTag("Player"))
			{
				cameraFollower.Rotate(rotation, distance, duration, curve);
			}
		}
	}
}
