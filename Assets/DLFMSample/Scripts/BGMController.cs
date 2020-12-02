using Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;
using System;

namespace Level
{
    public class BGMController : MonoBehaviour
    {
		[SerializeField] private AudioSource source;
		private Tweener tweener;

		private void Awake()
		{
			if (source == null) { source = GetComponent<AudioSource>(); }
			EventManager.onStateChange.AddListener(OnStateChange, Priority.Lowest);
			EventManager.onRespawn.AddListener(onRespawn, Priority.Lowest);
		}

		private RespawnEventArgs onRespawn(RespawnEventArgs e)
		{
			if (e.canceled) { return e; }
			tweener?.Kill();
			source.Play();
			source.volume = 1f;
			source.time = e.crown.time;
			source.Pause();
			return e;
		}

		private StateChangeEventArgs OnStateChange(StateChangeEventArgs e)
		{
			if (!e.canceled)
			{
				if (e.newState == GameState.Playing) { source.Play(); }
				if (e.newState == GameState.WaitingRespawn)
				{
					tweener?.Kill();
					tweener = source.DOFade(0f, 5f);
				}
			}
			return e;
		}
	}
}
