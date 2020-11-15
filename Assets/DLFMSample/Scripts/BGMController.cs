using Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Level
{
    public class BGMController : MonoBehaviour
    {
		[SerializeField] private AudioSource source;

		private void Awake()
		{
			if (source == null) { source = GetComponent<AudioSource>(); }
			EventManager.onStateChange.AddListener(OnStateChange, Priority.Lowest);
		}

		private StateChangeEventArgs OnStateChange(StateChangeEventArgs e)
		{
			if (!e.canceled)
			{
				if (e.newState == GameState.Playing) { source.Play(); }
				if (e.newState == GameState.WaitingRespawn) { source.Pause(); }
			}
			return e;
		}
	}
}
