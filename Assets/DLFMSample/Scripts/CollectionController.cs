using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace Level
{
    public class CollectionController : MonoBehaviour
    {
        private static CollectionController _instance;
        private List<KeyValuePair<ICollection, float>> collections = new List<KeyValuePair<ICollection, float>>();
        private ReadOnlyCollection<KeyValuePair<ICollection, float>> _collectionsCache = null;
        private readonly List<KeyValuePair<ICollection, float>> emptyList = new List<KeyValuePair<ICollection, float>>();

        public static CollectionController Instance
		{
            get => _instance ?? GameController.Instance.gameObject.AddComponent<CollectionController>();
        }
        public ReadOnlyCollection<KeyValuePair<ICollection, float>> Collections
		{
            get
			{
                if (_collectionsCache == null) { _collectionsCache = collections.AsReadOnly(); }
                return _collectionsCache;
			}
		}

        private void Awake()
		{
            if (_instance != null && _instance != this)
			{
                Destroy(this);
                Debug.LogWarning($"There is another {GetType()} active.");
                return;
            }
            _instance = this;
		}
        private void OnDestroy()
		{
            _instance = null;
		}

        public List<KeyValuePair<ICollection, float>> RemoveAfter(int index)
		{
            if (collections.Count - 1 < index) { return emptyList; }
            var startIndex = index + 1;
            var result = collections.GetRange(startIndex, collections.Count - startIndex);
            collections.RemoveRange(startIndex, collections.Count - startIndex);
            _collectionsCache = null;
            Debug.Log($"Remove {result.Count} collection{(result.Count > 1 ? "s" : "")} from {startIndex} to end, remains {collections.Count}");
            return result;
		}
        /// <summary>
        /// 不存在则啥都不干
        /// </summary>
        public List<KeyValuePair<ICollection, float>> RemoveAfter(ICollection collection)
		{
            if (collection == null)
            {
                Debug.LogWarning($"argument {nameof(collection)} is null, please use {nameof(RemoveAllNull)}");
                return emptyList;
            }
            var result = emptyList;
            var index = 0;
            foreach (var c in collections)
            {
                if (c.Key == collection)
                {
                    result = collections.GetRange(index, collections.Count - index);
                    collections.RemoveRange(index, collections.Count - index);
                    _collectionsCache = null;
                    break;
                }
                index++;
            }
            Debug.Log($"Remove {result.Count} collection{(result.Count > 1 ? "s" : "")} from {index} to end, remains {collections.Count}");
            return result;
		}

        public List<KeyValuePair<ICollection, float>> RemoveAfterByTime(float time)
        {
            var result = emptyList;
            var index = 0;
            foreach (var c in collections)
            {
                if (c.Value > time)
                {
                    result = collections.GetRange(index, collections.Count - index);
                    collections.RemoveRange(index, collections.Count - index);
                    _collectionsCache = null;
                    break;
                }
                index++;
            }
            Debug.Log($"Remove {result.Count} collection{(result.Count > 1 ? "s" : "")} from {index} to end, remains {collections.Count}");
            return result;
        }

        public void Add(ICollection collection, float time)
        {
            if (collection == null) { return; }
            Debug.Log($"Add collection {collection.GetType().ToString()} with time {time}");
            var index = 0;
            foreach (var c in collections)
            {
                if (c.Value > time)
                {
                    collections.Insert(index, new KeyValuePair<ICollection, float>(collection, time));
                    _collectionsCache = null;
                    return;
                }
                index++;
            }
            collections.Add(new KeyValuePair<ICollection, float>(collection, time));
            _collectionsCache = null;
        }
        public void Add(ICollection[] collections, float time)
		{
            if (collections == null || collections.Length == 0) { return; }
            var index = 0;
            foreach (var c in this.collections)
            {
                if (c.Value > time)
                {
                    this.collections.InsertRange(index, collections.Select(v => new KeyValuePair<ICollection, float>(v, time)));
                    _collectionsCache = null;
                    return;
                }
                index++;
            }
            this.collections.AddRange(collections.Select(v => new KeyValuePair<ICollection, float>(v, time)));
            _collectionsCache = null;
        }

        public KeyValuePair<ICollection, float>? Remove(ICollection collection)
		{
            var index = 0;
            foreach (var c in collections)
            {
                if (c.Key == collection)
                {
                    collections.RemoveAt(index);
                    return c;
                }
            }
            return null;
        }
        public void RemoveAll()
		{
            if (collections.Count == 0) { return; }
            collections.Clear();
            _collectionsCache = null;
        }
        public void RemoveAllNull()
        {
            var newCollections = new List<KeyValuePair<ICollection, float>>(collections.Where(v => v.Key != null));
            collections.Clear();
            collections = newCollections;
            _collectionsCache = null;
        }
    }
}
