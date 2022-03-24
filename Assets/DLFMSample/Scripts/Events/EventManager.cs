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
		public Action<T> action;
		public Priority priority;

		public HandlerAttributes(Action<T> action, Priority priority)
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

	public class EventPipeline<T> where T : EventArgsBase
	{
		private LinkedList<HandlerAttributes<T>> handlers = new LinkedList<HandlerAttributes<T>>();
		private ReaderWriterLockSlim lockSlim = new ReaderWriterLockSlim();
		private LinkedListNode<HandlerAttributes<T>>[] lastPriorityNodePositions;
		private bool invoking;

		public EventPipeline()
		{
			lastPriorityNodePositions = new LinkedListNode<HandlerAttributes<T>>[Enum.GetNames(typeof(Priority)).Length];
		}

		public void AddListener(Action<T> action, Priority priority)
		{
			if (invoking) { throw new LockRecursionException("died lock"); }
			lockSlim.EnterWriteLock();
			var handlerAttributes = new HandlerAttributes<T>(action, priority);
			if (lastPriorityNodePositions[(int)priority] == null)
			{
				// 往前找
				for (int i = (int)priority - 1; i >= 0; i--)
				{
					if (lastPriorityNodePositions[i] != null)
					{
						var node = handlers.AddAfter(lastPriorityNodePositions[i], handlerAttributes);
						lastPriorityNodePositions[(int)priority] = node;
						lockSlim.ExitWriteLock();
						return;
					}
				}
				// 找不到
				handlers.AddFirst(handlerAttributes);
				lastPriorityNodePositions[(int)priority] = handlers.First;
			}
			else
			{
				var node = handlers.AddAfter(lastPriorityNodePositions[(int)priority], handlerAttributes);
				lastPriorityNodePositions[(int)priority] = node;
			}
			lockSlim.ExitWriteLock();
		}

		public bool RemoveListener(Action<T> action, Priority priority)
		{
			if (invoking) { throw new LockRecursionException("died lock"); }
			HandlerAttributes<T> handler = new HandlerAttributes<T>(action, priority);
			lockSlim.EnterWriteLock();
			bool succeed = handlers.Remove(handler);
			lockSlim.ExitWriteLock();
			return succeed;
		}

		public void Invoke(T args, Action<T> callback = null)
		{
			bool callbackRan = false;
			lockSlim.EnterReadLock();
			invoking = true;
			foreach (HandlerAttributes<T> handler in handlers)
			{
				try
				{
					if (handler.priority != Priority.Monitor)
					{
						handler.action.Invoke(args);
					}
					else
					{
						if (!callbackRan) { callback?.Invoke(args); }
						callbackRan = true;
						handler.action.Invoke(args);
					}
				}
				catch (Exception e)
				{
					Debug.LogError($"An exception \"{e.Message}\" occurred when running action \"{handler.action.Method.Name}\"");
					throw;
				}
			}
			lockSlim.ExitReadLock();
			invoking = false;
			if (!callbackRan) { callback?.Invoke(args); }
		}
	}
}
