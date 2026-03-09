namespace Aloe.Apps.WindowsServiceMonitorLib.OtelViewer.Storage;

public sealed class CircularBuffer<T>
{
    private readonly T[] buffer;
    private readonly object lockObj = new();
    private int head;
    private int count;

    public CircularBuffer(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(capacity, 1);
        this.buffer = new T[capacity];
    }

    public int Count
    {
        get
        {
            lock (this.lockObj)
            {
                return this.count;
            }
        }
    }

    public int Capacity => this.buffer.Length;

    public void Add(T item)
    {
        lock (this.lockObj)
        {
            this.buffer[this.head] = item;
            this.head = (this.head + 1) % this.buffer.Length;
            if (this.count < this.buffer.Length)
            {
                this.count++;
            }
        }
    }

    public void AddRange(IEnumerable<T> items)
    {
        lock (this.lockObj)
        {
            foreach (var item in items)
            {
                this.buffer[this.head] = item;
                this.head = (this.head + 1) % this.buffer.Length;
                if (this.count < this.buffer.Length)
                {
                    this.count++;
                }
            }
        }
    }

    public List<T> ToList()
    {
        lock (this.lockObj)
        {
            var result = new List<T>(this.count);
            for (int i = 0; i < this.count; i++)
            {
                int index = (this.head - this.count + i + this.buffer.Length) % this.buffer.Length;
                result.Add(this.buffer[index]);
            }

            return result;
        }
    }

    public List<T> Query(Func<T, bool> predicate, int skip, int take)
    {
        lock (this.lockObj)
        {
            var result = new List<T>();
            int skipped = 0;

            // Iterate from newest to oldest
            for (int i = this.count - 1; i >= 0 && result.Count < take; i--)
            {
                int index = (this.head - this.count + i + this.buffer.Length) % this.buffer.Length;
                var item = this.buffer[index];
                if (predicate(item))
                {
                    if (skipped < skip)
                    {
                        skipped++;
                    }
                    else
                    {
                        result.Add(item);
                    }
                }
            }

            return result;
        }
    }

    public int CountWhere(Func<T, bool> predicate)
    {
        lock (this.lockObj)
        {
            int result = 0;
            for (int i = 0; i < this.count; i++)
            {
                int index = (this.head - this.count + i + this.buffer.Length) % this.buffer.Length;
                if (predicate(this.buffer[index]))
                {
                    result++;
                }
            }

            return result;
        }
    }

    public IReadOnlySet<TKey> DistinctValues<TKey>(Func<T, TKey> selector)
    {
        lock (this.lockObj)
        {
            var result = new HashSet<TKey>();
            for (int i = 0; i < this.count; i++)
            {
                int index = (this.head - this.count + i + this.buffer.Length) % this.buffer.Length;
                result.Add(selector(this.buffer[index]));
            }

            return result;
        }
    }
}
