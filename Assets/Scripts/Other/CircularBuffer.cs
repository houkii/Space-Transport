using System;
using System.Collections;
using System.Collections.Generic;

public class CircularBuffer
{
    private int capacity;
    private Queue<float> Values;
    public float AverageValue => AveragedValue();

    public CircularBuffer(int capacity)
    {
        this.capacity = capacity;
        Values = new Queue<float>(capacity);
    }

    private float AveragedValue()
    {
        float sum = 0f;
        foreach (float item in Values)
        {
            sum += item;
        }
        return (sum / Values.Count);
    }

    public void Add(float value)
    {
        if (Values.Count + 1 > capacity)
        {
            Values.Dequeue();
        }
        Values.Enqueue(value);
    }

    public void Clear()
    {
        Values.Clear();
    }
}

public class CircularBuffer<T> : IEnumerable<T>
{
    private readonly T[] _buffer;
    private int _start;
    private int _end;
    private int _size;

    public CircularBuffer(int capacity)
        : this(capacity, new T[] { })
    {
    }

    public CircularBuffer(int capacity, T[] items)
    {
        _buffer = new T[capacity];

        Array.Copy(items, _buffer, items.Length);
        _size = items.Length;

        _start = 0;
        _end = _size == capacity ? 0 : _size;
    }
    public int Capacity { get { return _buffer.Length; } }

    public bool IsFull
    {
        get
        {
            return Size == Capacity;
        }
    }

    public bool IsEmpty
    {
        get
        {
            return Size == 0;
        }
    }

    public int Size { get { return _size; } }

    public T Front()
    {
        ThrowIfEmpty();
        return _buffer[_start];
    }

    public T Back()
    {
        ThrowIfEmpty();
        return _buffer[(_end != 0 ? _end : Capacity) - 1];
    }

    public void PushBack(T item)
    {
        if (IsFull)
        {
            _buffer[_end] = item;
            Increment(ref _end);
            _start = _end;
        }
        else
        {
            _buffer[_end] = item;
            Increment(ref _end);
            ++_size;
        }
    }

    public void PushFront(T item)
    {
        if (IsFull)
        {
            Decrement(ref _start);
            _end = _start;
            _buffer[_start] = item;
        }
        else
        {
            Decrement(ref _start);
            _buffer[_start] = item;
            ++_size;
        }
    }

    public void PopBack()
    {
        ThrowIfEmpty("Cannot take elements from an empty buffer.");
        Decrement(ref _end);
        _buffer[_end] = default(T);
        --_size;
    }

    public void PopFront()
    {
        ThrowIfEmpty("Cannot take elements from an empty buffer.");
        _buffer[_start] = default(T);
        Increment(ref _start);
        --_size;
    }

    public T[] ToArray()
    {
        T[] newArray = new T[Size];
        int newArrayOffset = 0;
        var segments = new ArraySegment<T>[2] { ArrayOne(), ArrayTwo() };
        foreach (ArraySegment<T> segment in segments)
        {
            Array.Copy(segment.Array, segment.Offset, newArray, newArrayOffset, segment.Count);
            newArrayOffset += segment.Count;
        }
        return newArray;
    }

    #region IEnumerable<T> implementation
    public IEnumerator<T> GetEnumerator()
    {
        var segments = new ArraySegment<T>[2] { ArrayOne(), ArrayTwo() };
        foreach (ArraySegment<T> segment in segments)
        {
            for (int i = 0; i < segment.Count; i++)
            {
                yield return segment.Array[segment.Offset + i];
            }
        }
    }
    #endregion
    #region IEnumerable implementation
    IEnumerator IEnumerable.GetEnumerator()
    {
        return (IEnumerator)GetEnumerator();
    }
    #endregion

    private void ThrowIfEmpty(string message = "Cannot access an empty buffer.")
    {
        if (IsEmpty)
        {
            throw new InvalidOperationException(message);
        }
    }

    private void Increment(ref int index)
    {
        if (++index == Capacity)
        {
            index = 0;
        }
    }

    private void Decrement(ref int index)
    {
        if (index == 0)
        {
            index = Capacity;
        }
        index--;
    }

    private int InternalIndex(int index)
    {
        return _start + (index < (Capacity - _start) ? index : index - Capacity);
    }

    #region Array items easy access.

    private ArraySegment<T> ArrayOne()
    {
        if (_start < _end)
        {
            return new ArraySegment<T>(_buffer, _start, _end - _start);
        }
        else
        {
            return new ArraySegment<T>(_buffer, _start, _buffer.Length - _start);
        }
    }

    private ArraySegment<T> ArrayTwo()
    {
        if (_start < _end)
        {
            return new ArraySegment<T>(_buffer, _end, 0);
        }
        else
        {
            return new ArraySegment<T>(_buffer, 0, _end);
        }
    }
    #endregion
}
