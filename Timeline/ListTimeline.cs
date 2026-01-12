

using System;
using System.Collections;
using System.Collections.Generic;

namespace Timeline;

public class ListTimeline<Time, Event> : IMutableTimeline<Time, Event>
    where Time : notnull, IComparable<Time>
{
    private readonly SortedList<Time, List<Event>> _points;

    public ListTimeline()
    {
        _points = new();
    }

    public Event[] this[Time time] => _points[time].ToArray();

    public void Add(Time time, Event value)
    {
        if (_points.ContainsKey(time))
        {
            _points[time].Add(value);
        }
        else
        {
            _points[time] = new List<Event> { value };
        }
    }

    public IEnumerator<ITimeline<Time, Event>.Point> GetEnumerator()
    {
        foreach (var entry in _points)
        {
            yield return new ITimeline<Time, Event>.Point
            {
                Time = entry.Key,
                Events = entry.Value.ToArray(),
            };
        }
    }

    public ITimeline<Time, Event>.Point GetNext(Time time)
    {
        foreach (var point in _points)
        {
            if (point.Key.CompareTo(time) > 0)
            {
                return new ITimeline<Time, Event>.Point
                {
                    Time = point.Key,
                    Events = point.Value.ToArray(),
                };
            }
        }

        throw new KeyNotFoundException(time.ToString());
    }

    public ITimeline<Time, Event>.Point GetPrevious(Time time)
    {
        ITimeline<Time, Event>.Point? result = null;
        foreach (var point in _points)
        {
            if (point.Key.CompareTo(time) < 0)
            {
                break;
            }

            result = new ITimeline<Time, Event>.Point
            {
                Time = point.Key,
                Events = point.Value.ToArray(),
            };
        }

        if (result != null)
        {
            return result.Value;
        }

        throw new KeyNotFoundException(time.ToString());
    }

    public bool HasEvent(Time time)
    {
        return _points.ContainsKey(time);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}