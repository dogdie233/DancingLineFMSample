using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemainValuesRandom : MonoBehaviour
{
	public Transform collision;
	public float maxOffset;
	public Rigidbody[] rigidbodies;
	public Vector2 drag;

	private void Start()
	{
		collision.position += new Vector3(Random.Range(-maxOffset, maxOffset), Random.Range(-maxOffset, maxOffset), Random.Range(-maxOffset, maxOffset));
		foreach (Rigidbody rig in rigidbodies)
		{
			rig.drag = Random.Range(drag.x, drag.y);
		}
	}
}
