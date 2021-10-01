using Level;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

namespace Event
{
	public enum Priority
	{
		High,
		Normal,
		Low,
		Monitor
	}

	public struct HandlerAttributes<T> : IEquatable<HandlerAttributes<T>>
	{
		public Func<T, T> action;
		public Priority priority;

		public HandlerAttributes(Func<T, T> action, Priority priority)
		{
			this.action = action;
			this.priority = priority;
		}

		public static bool operator ==(HandlerAttributes<T> lhs, HandlerAttributes<T> rhs) => lhs.Equals(rhs);
		public static bool operator !=(HandlerAttributes<T> lhs, HandlerAttributes<T> rhs) => !lhs.Equals(rhs);

		public bool Equals(HandlerAttributes<T> other) => action == other.action && priority == other.priority;

		public override bool Equals(object obj)
		{
			if (!(obj is HandlerAttributes<T>)) { return false; }
			return Equals((HandlerAttributes<T>)obj);
		}

		public override int GetHashCode()
		{
			int hash = 17;
			hash = 31 * hash + action.GetHashCode();
			hash = 31 * hash + priority.GetHashCode();
			return hash;
		}
	}

	public class EventBase<T>
	{
		private LinkedList<HandlerAttributes<T>> handlers = new LinkedList<HandlerAttributes<T>>();
		private ReaderWriterLockSlim lockSlim = new ReaderWriterLockSlim();

		public void AddListener(Func<T, T> action, Priority priority)
		{
			lockSlim.EnterWriteLock();
			if (handlers.Count == 0)
			{
				handlers.AddFirst(new LinkedListNode<HandlerAttributes<T>>(new HandlerAttributes<T>(action, priority)));
				lockSlim.ExitWriteLock();
				return;
			}
			if (priority == Priority.Monitor)
			{
				handlers.AddLast(new HandlerAttributes<T>(action, priority));
				lockSlim.ExitWriteLock();
				return;
			}
			LinkedListNode<HandlerAttributes<T>> node = handlers.First;
			while (true)
			{
				if ((int)node.Value.priority < (int)priority)
				{
					handlers.AddBefore(node, new LinkedListNode<HandlerAttributes<T>>(new HandlerAttributes<T>(action, priority)));
					break;
				}
				if (node.Next == null)
				{
					handlers.AddLast(new LinkedListNode<HandlerAttributes<T>>(new HandlerAttributes<T>(action, priority)));
					break;
				}
				node = node.Next;
			}
			lockSlim.ExitWriteLock();
		}

		public bool RemoveListener(Func<T, T> action, Priority priority)
		{
			HandlerAttributes<T> handler = new HandlerAttributes<T>(action, priority);
			lockSlim.EnterWriteLock();
			bool succeed = handlers.Remove(handler);
			lockSlim.ExitWriteLock();
			return succeed;
		}

		public void Invoke(T arg, Action<T> callback = null)
		{
			T newArg;
			bool callbackRan = false;
			lockSlim.EnterReadLock();
			foreach (HandlerAttributes<T> handler in handlers)
			{
				try
				{
					if (handler.priority != Priority.Monitor)
					{
						newArg = handler.action.Invoke(arg);
						arg = newArg;
					}
					else
					{
						if (!callbackRan) { callback?.Invoke(arg); }
						callbackRan = true;
						handler.action.Invoke(arg);
					}
				}
				catch (Exception e)
				{
					Debug.LogError($"An exception \"{e.Message}\" occurred when running action \"{handler.action.Method.Name}\", the content modified by the event will be drop.\n{e.StackTrace}");
				}
			}
			lockSlim.ExitReadLock();
			if (!callbackRan) { callback?.Invoke(arg); }
		}
	}

	public static class EventManager
	{
		public static EventBase<StateChangeEventArgs> OnStateChange { get; } = new EventBase<StateChangeEventArgs>();
		public static EventBase<DiamondPickedEventArgs> OnDiamondPicked { get; } = new EventBase<DiamondPickedEventArgs>();
		public static EventBase<CrownPickedEventArgs> OnCrownPicked { get; } = new EventBase<CrownPickedEventArgs>();
		public static EventBase<RespawnEventArgs> OnRespawn { get; } = new EventBase<RespawnEventArgs>();
		public static EventBase<LineTurnEventArgs> OnLineTurn { get; } = new EventBase<LineTurnEventArgs>();
		public static EventBase<LineDieEventArgs> OnLineDie { get; } = new EventBase<LineDieEventArgs>();
		public static EventBase<SkinChangeEventArgs> OnSkinChange { get; } = new EventBase<SkinChangeEventArgs>();
	}
}
