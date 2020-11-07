using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Level.Event
{
	public static class EventManager
	{
		public delegate void StateChange(ref StateChangeEvent arg);
		public delegate void GameOver(ref GameOverEvent arg);
		public static UnityEvent onStart = new UnityEvent();
		public static UnityEvent onBGButtonClick = new UnityEvent();
		public static StateChange OnStateChange;
		public static GameOver onGameOver;

		public static void RunStateChangeEvent(GameState oldState, GameState newState, UnityAction<StateChangeEvent> callback)
		{
			StateChangeEvent arg = new StateChangeEvent(oldState, newState);
			try {
				IAsyncResult iar = OnStateChange.BeginInvoke(ref arg, ar => { callback.Invoke(arg); }, null);
				OnStateChange.EndInvoke(ref arg, iar);
			}
			catch (NullReferenceException) { callback.Invoke(arg); }
		}

		public static void RunGameOverEvent(DeathCause deathCause, UnityAction<GameOverEvent> callback)
		{
			GameOverEvent arg = new GameOverEvent(deathCause);
			try {
				IAsyncResult iar = onGameOver.BeginInvoke(ref arg, ar => { callback.Invoke(arg); }, null);
				onGameOver.EndInvoke(ref arg, iar);
			}
			catch (NullReferenceException) { callback.Invoke(arg); }
		}
	}
}
