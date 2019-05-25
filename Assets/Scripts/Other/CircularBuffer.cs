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