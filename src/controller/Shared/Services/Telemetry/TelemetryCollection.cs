using System.Collections;

namespace Shared.Services.Telemetry;

public class TelemetryCollection<T> : ICollection<T>
{
    public event Func<T, Task>? ItemsAdded;

    public IEnumerator<T> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(T item)
    {
        ItemsAdded?.Invoke(item).Wait();
    }

    public void Clear()
    {
        throw new NotImplementedException();
    }

    public bool Contains(T item)
    {
        throw new NotImplementedException();
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }

    public bool Remove(T item)
    {
        throw new NotImplementedException();
    }

    public int Count => 0;
    public bool IsReadOnly => false;
}