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
		private LinkedList<HandlerAttributes<T>> handlers = new LinkedList<HandlerAttributes<T>>();

		public void AddListener(Func<T, T> action, Priority priority)
		{
			if (priority == Priority.Lowest)
			{
				handlers.AddLast(new HandlerAttributes<T>(action, priority));
				return;
			}
			foreach (HandlerAttributes<T> handler in handlers)
			{
				if ((int)handler.priority > (int)priority)
				{
					handlers.AddBefore(handlers.Find(handler), new HandlerAttributes<T>(action, priority));
					break;
				}
			}
		}

		public void RemoveListener(Func<T, T> action)
		{
			LinkedListNode<HandlerAttributes<T>> temp = null;
			foreach (HandlerAttributes<T> handler in handlers)
			{
				if (action == handler.action)
				{
					temp = handlers.Find(handler);
					break;
				}
			}
			if (temp != null) { handlers.Remove(temp); }
		}

		public T Invoke(T arg, Action<T> callback = null)
		{
			foreach (HandlerAttributes<T> handler in handlers)
			{
				arg = handler.action.Invoke(arg);
			}
			callback?.Invoke(arg);
			return arg;
		}
	}

	public static class EventManager
	{
		public static EventBase<StateChangeEventArgs> onStateChange = new EventBase<StateChangeEventArgs>();
		public static EventBase<DiamondPickedEventArgs> onDiamondPicked = new EventBase<DiamondPickedEventArgs>();
		public static EventBase<CrownPickedEventArgs> onCrownPicked = new EventBase<CrownPickedEventArgs>();
		public static EventBase<RespawnEventArgs> onRespawn = new EventBase<RespawnEventArgs>();
	}
}
