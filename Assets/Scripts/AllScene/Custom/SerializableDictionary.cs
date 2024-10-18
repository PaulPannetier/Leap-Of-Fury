using System;
using System.Collections.Generic;

[Serializable]
public class SerializableDictionary<TKey, TValue>
{
    public List<DictionaryElement> elements;

    public SerializableDictionary()
    {
        elements = new List<DictionaryElement>();
    }

    public TValue this[TKey key]
    {
        get
        {
            int hashCode = key.GetHashCode();
            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i].key.GetHashCode() == hashCode)
                {
                    return elements[i].value;
                }
            }
            throw new IndexOutOfRangeException($"The key {key} is not in the dictionnary");
        }
        set
        {
            int hashCode = key.GetHashCode();
            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i].key.GetHashCode() == hashCode)
                {
                    elements[i] = new DictionaryElement(key, value);
                    return;
                }
            }
            elements.Add(new DictionaryElement(key, value));
        }
    }

    public void Add(TKey key, TValue value) => this[key] = value;

    [Serializable]
    public struct DictionaryElement
    {
        public TKey key;
        public TValue value;

        public DictionaryElement(TKey key, TValue value)
        {
            this.key = key;
            this.value = value;
        }
    }
}