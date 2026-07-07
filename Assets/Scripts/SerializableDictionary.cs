using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class SerializableDictionary<TKey, TValue> {
    [SerializeField]
    private List<SerializableKeyValuePair<TKey, TValue>> items = new List<SerializableKeyValuePair<TKey, TValue>>();

    private Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();

    public Dictionary<TKey, TValue> Dictionary => dictionary;

    public KeyValuePair<TKey, TValue> this[int index] {
        get {
            if (index >= 0 && index < items.Count)
                return new KeyValuePair<TKey, TValue>(items[index].Key, items[index].Value);
            return default;
        }
    }

    public int Count => dictionary.Count;

    [Serializable]
    private class SerializableKeyValuePair<T1, T2> {
        public T1 Key;
        public T2 Value;
    }

    private void OnValidate() {
        dictionary.Clear();
        foreach (var item in items) {
            if (item.Key != null && !dictionary.ContainsKey(item.Key))
                dictionary[item.Key] = item.Value;
        }
    }
}
