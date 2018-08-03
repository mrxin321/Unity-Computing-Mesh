using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
public struct NativeList<T> where T:struct
{
    public NativeArray<T> array;
    private int m_length;
    public int length
    {
        get
        {
            return m_length;
        }
    }
    public NativeList(int capacity)
    {
        m_length = 0;
        array = new NativeArray<T>(capacity, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
    }

    public void Add(T value)
    {
        int currentIndex = m_length++;
        if(m_length > array.Length)
        {
            NativeArray<T> newArray = new NativeArray<T>(array.Length * 2, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            for(int i = 0; i < array.Length; ++i)
            {
                newArray[i] = array[i];
            }
            array.Dispose();
            array = newArray;
        }
        array[currentIndex] = value;
    }

    public void Clear()
    {
        m_length = 0;
    }

    public void Remove(int index)
    {
        int last = m_length - 1;
        if(last != index)
        {
            array[index] = array[last];
        }
        m_length--;
    }
}