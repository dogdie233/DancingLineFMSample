using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RemainAnimatorRandomPlay : MonoBehaviour
{
	[SerializeField] private Animator animator;

    private void Start()
	{
		animator = animator ?? GetComponent<Animator>();
		string[] states = animator.runtimeAnimatorController.animationClips.Select(clip => clip.name).ToArray();
		animator.Play(states[Random.Range(0, states.Length)]);
		transform.localEulerAngles = new Vector3(0, Random.Range(0f, 360f), 0);
	}
}
