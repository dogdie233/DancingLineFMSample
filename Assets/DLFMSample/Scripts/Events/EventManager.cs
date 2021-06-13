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
		Monitor,
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

		public bool RemoveListener(Func<T, T> action)
		{
			LinkedListNode<HandlerAttributes<T>> node = handlers.First;
			lockSlim.EnterUpgradeableReadLock();
			while (true)
			{
				if (node.Value.action == action)
				{
					lockSlim.EnterWriteLock();
					handlers.Remove(node);
					lockSlim.ExitWriteLock();
					return true;
				}
				if (node.Next == null)
				{
					lockSlim.ExitUpgradeableReadLock();
					return false;
				}
				node = node.Next;
			}
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
		public static EventBase<StateChangeEventArgs> onStateChange = new EventBase<StateChangeEventArgs>();
		public static EventBase<DiamondPickedEventArgs> onDiamondPicked = new EventBase<DiamondPickedEventArgs>();
		public static EventBase<CrownPickedEventArgs> onCrownPicked = new EventBase<CrownPickedEventArgs>();
		public static EventBase<RespawnEventArgs> onRespawn = new EventBase<RespawnEventArgs>();
		public static EventBase<LineTurnEventArgs> onLineTurn = new EventBase<LineTurnEventArgs>();
		public static EventBase<LineDieEventArgs> onLineDie = new EventBase<LineDieEventArgs>();
		public static EventBase<SkinChangeEventArgs> onSkinChange = new EventBase<SkinChangeEventArgs>();
	}
}
