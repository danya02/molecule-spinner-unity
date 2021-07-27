using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtomDict : IDictionary<Vector3, GameObject>
{
    Dictionary<Vector3, GameObject> backing = new Dictionary<Vector3, GameObject>();

    public GameObject this[Vector3 key] { get => backing[Atom.RoundAtomPosition(key)]; set => backing[Atom.RoundAtomPosition(key)] = value; }

    public ICollection<Vector3> Keys
    {
        get
        {
            ICollection<Vector3> keys = new List<Vector3>();
            foreach (Vector3 k in backing.Keys)
            {
                keys.Add(Atom.RoundAtomPosition(k));
            }
            return keys;
        }
    }

    public ICollection<GameObject> Values => backing.Values;

    public int Count => backing.Count;

    public bool IsReadOnly => false;

    public void Add(Vector3 key, GameObject value)
    {
        try
        {
            backing.Add(Atom.RoundAtomPosition(key), value);
        }
        catch { }
    }

    public void Add(KeyValuePair<Vector3, GameObject> item)
    {
        backing.Add(Atom.RoundAtomPosition(item.Key), item.Value);
    }

    public void Clear()
    {
        backing.Clear();
    }

    public bool Contains(KeyValuePair<Vector3, GameObject> item)
    {
        GameObject go;
        return backing.TryGetValue(Atom.RoundAtomPosition(item.Key), out go) && go == item.Value;
    }

    public bool ContainsKey(Vector3 key)
    {
        return backing.ContainsKey(Atom.RoundAtomPosition(key));
    }

    public void CopyTo(KeyValuePair<Vector3, GameObject>[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }

    public IEnumerator<KeyValuePair<Vector3, GameObject>> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    public bool Remove(Vector3 key)
    {
        return backing.Remove(Atom.RoundAtomPosition(key));
    }

    public bool Remove(KeyValuePair<Vector3, GameObject> item)
    {
        return Remove(item.Key);
    }

    public bool TryGetValue(Vector3 key, out GameObject value)
    {
        return backing.TryGetValue(Atom.RoundAtomPosition(key), out value);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }

}
