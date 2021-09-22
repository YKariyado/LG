using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sparse3DArray<T>
{
    public Dictionary<Tuple<int, int, int>, T> data = new Dictionary<Tuple<int, int, int>, T>();

    public int Nnz { get { return data.Count; } }

    public void dataClear()
    {
        data.Clear();
    }

    public T this[int x, int y, int z]
    {
        get
        {
            var key = new Tuple<int, int, int>(x, y, z);
            T value;
            data.TryGetValue(key, out value);
            return value;
        }

        set
        {
            var key = new Tuple<int, int, int>(x, y, z);
            if (null == value)
                data.Remove(key);
            else if (value.Equals(default(T)))
                data.Remove(key);
            else
                data[key] = value;
        }
    }
}
