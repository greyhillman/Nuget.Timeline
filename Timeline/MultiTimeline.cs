using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Timeline;

public class MultiTimeline<Time, Event> : ITimeline<Time, Event>
    where Time : notnull, IComparable<Time>
{
    private readonly List<ITimeline<Time, Event>> _timelines;

    public MultiTimeline(IEnumerable<ITimeline<Time, Event>> timelines)
    {
        _timelines = timelines.ToList();
    }

    public MultiTimeline(params ITimeline<Time, Event>[] timelines)
    {
        _timelines = timelines.ToList();
    }

    public bool HasEvent(Time time)
    {
        return _timelines.Any(timeline => timeline.HasEvent(time));
    }

    public Event[] this[Time time]
    {
        get
        {
            var result = new List<Event>();

            foreach (var timeline in _timelines)
            {
                if (timeline.HasEvent(time))
                {
                    result.AddRange(timeline[time]);
                }
            }

            if (result.Count == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(time));
            }

            return result.ToArray();
        }
    }

    public ITimeline<Time, Event>.Point GetNext(Time time)
    {
        var recents = new List<ITimeline<Time, Event>.Point>();

        foreach (var timeline in _timelines)
        {
            try
            {
                recents.Add(timeline.GetNext(time));
            }
            catch (ArgumentOutOfRangeException)
            {
                continue;
            }
        }

        var groups =
            from recent in recents
            orderby recent ascending
            group recent by recent.Time;

        var points = groups.First();

        return new ITimeline<Time, Event>.Point
        {
            Time = points.First().Time,
            Events = points.SelectMany(point => point.Events).ToArray(),
        };
    }

    public ITimeline<Time, Event>.Point GetPrevious(Time time)
    {
        var recents = new List<ITimeline<Time, Event>.Point>();

        foreach (var timeline in _timelines)
        {
            try
            {
                recents.Add(timeline.GetPrevious(time));
            }
            catch (ArgumentOutOfRangeException)
            {
                continue;
            }
        }

        var groups =
            from recent in recents
            orderby recent descending
            group recent by recent.Time;

        var points = groups.First();

        return new ITimeline<Time, Event>.Point
        {
            Time = points.First().Time,
            Events = points.SelectMany(point => point.Events).ToArray(),
        };
    }

    public IEnumerator<ITimeline<Time, Event>.Point> GetEnumerator()
    {
        var enumerators = (
            from timeline in _timelines
            select timeline.GetEnumerator()
        ).ToArray();

        return new MultiEnumerator(enumerators);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private class MultiEnumerator : IEnumerator<ITimeline<Time, Event>.Point>
    {
        private readonly IEnumerator<ITimeline<Time, Event>.Point>[] _enumerators;
        private readonly List<IEnumerator<ITimeline<Time, Event>.Point>> _current;
        private readonly Dictionary<IEnumerator<ITimeline<Time, Event>.Point>, bool> _hasMore;

        public MultiEnumerator(IEnumerable<IEnumerator<ITimeline<Time, Event>.Point>> enumerators)
        {
            _enumerators = enumerators.ToArray();
            _hasMore = new();

            _current = new();
            foreach (var enumerator in _enumerators)
            {
                _current.Add(enumerator);
            }
        }

        public bool MoveNext()
        {
            foreach (var enumerator in _current)
            {
                var hasMore = enumerator.MoveNext();
                _hasMore[enumerator] = hasMore;
            }

            _current.Clear();

            foreach (var enumerator in _enumerators)
            {
                var hasMore = _hasMore[enumerator];
                if (!hasMore)
                {
                    continue;
                }

                if (!_current.Any())
                {
                    _current.Add(enumerator);
                    continue;
                }

                var time = enumerator.Current.Time;
                var currentTime = _current[0].Current.Time;

                if (time.CompareTo(currentTime) < 0)
                {
                    _current.Clear();
                    _current.Add(enumerator);
                }
                else if (time.CompareTo(currentTime) == 0)
                {
                    _current.Add(enumerator);
                }
            }

            return _current.Any();
        }

        public ITimeline<Time, Event>.Point Current
        {
            get
            {
                var result = new List<ITimeline<Time, Event>.Point>();
                foreach (var enumerator in _current)
                {
                    var point = enumerator.Current;

                    result.Add(point);
                }

                if (result.Count == 0)
                {
                    throw new InvalidOperationException();
                }

                return new ITimeline<Time, Event>.Point
                {
                    Time = result.First().Time,
                    Events = result.SelectMany(p => p.Events).ToArray(),
                };
            }
        }

        object IEnumerator.Current => Current;

        public void Dispose()
        {
            foreach (var enumerator in _enumerators)
            {
                enumerator.Dispose();
            }
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }
}
