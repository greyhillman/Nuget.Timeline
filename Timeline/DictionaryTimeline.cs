using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Timeline;

public class DictionaryTimeline<Time, Event> : IMutableTimeline<Time, Event>
    where Time : notnull, IComparable<Time>
{
    private readonly SortedDictionary<Time, List<Event>> _points;

    public DictionaryTimeline()
    {
        _points = new();
    }

    public Event[] this[Time time]
    {
        get => _points[time].ToArray();
    }

    public void Add(Time time, Event value)
    {
        if (_points.ContainsKey(time))
        {
            _points[time].Add(value);
        }
        else
        {
            _points.Add(time, new List<Event>() { value });
        }
    }

    public ITimeline<Time, Event>.Point GetNext(Time time)
    {
        var laterTimes = _points.Keys.Where(t => t.CompareTo(time) > 0)
            .OrderBy(t => t);

        if (!laterTimes.Any())
        {
            throw new KeyNotFoundException(time.ToString());
        }

        var mostRecentTime = laterTimes.First();

        return new ITimeline<Time, Event>.Point
        {
            Time = mostRecentTime,
            Events = _points[mostRecentTime].ToArray(),
        };
    }

    public ITimeline<Time, Event>.Point GetPrevious(Time time)
    {
        var earlierTimes = _points.Keys.Where(t => t.CompareTo(time) < 0)
            .OrderByDescending(t => t);

        if (!earlierTimes.Any())
        {
            throw new KeyNotFoundException(time.ToString());
        }

        var previousTime = earlierTimes.First();

        return new ITimeline<Time, Event>.Point
        {
            Time = previousTime,
            Events = _points[previousTime].ToArray(),
        };
    }

    public bool HasEvent(Time time)
    {
        return _points.ContainsKey(time);
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

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }
}
