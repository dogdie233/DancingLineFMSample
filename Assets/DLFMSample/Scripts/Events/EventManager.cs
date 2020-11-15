using Level;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Event
{
	public enum Priority
	{
		Highest,
		High,
		Normal,
		Low,
		Lowest
	}

	public class HandlerAttributes<T>
	{
		public Func<T, T> action;
		public Priority priority;

		public HandlerAttributes(Func<T, T> action, Priority priority)
		{
			this.action = action;
			this.priority = priority;
		}
	}

	public class EventBase<T>
	{
		private static LinkedList<HandlerAttributes<T>> handlers = new LinkedList<HandlerAttributes<T>>();

		public void AddListener(Func<T, T> action, Priority priority)
		{
			if (priority == Priority.Lowest)
			{
				handlers.AddLast(new HandlerAttributes<T>(action, priority));
				return;
			}
			foreach (HandlerAttributes<T> handler in handlers)
			{
				if ((int)handler.priority < (int)priority)
				{
					handlers.AddBefore(handlers.Find(handler), new HandlerAttributes<T>(action, priority));
					break;
				}
			}
		}

		public void RemoveListener(Func<T, T> action)
		{
			foreach (HandlerAttributes<T> handler in handlers)
			{
				if (action == handler.action)
				{
					handlers.Remove(handlers.Find(handler));
					break;
				}
			}
		}

		public void Invoke(T arg)
		{
			foreach (HandlerAttributes<T> handler in handlers)
			{
				arg = handler.action.Invoke(arg);
			}
		}
	}

	public static class EventManager
	{
		public class StateChangeEvent: EventBase<StateChangeEventArgs>
		{
			public void Invoke(GameState oldState, GameState newState)
			{
				Invoke(new StateChangeEventArgs(oldState, newState));
			}
		}


		public static StateChangeEvent onStateChange = new StateChangeEvent();
	}
}
