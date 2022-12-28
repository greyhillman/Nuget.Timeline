using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Timeline
{
    public class DictionaryTimeline<Time, Event> : ITimeline<Time, Event>
        where Time : notnull, IComparable<Time>
    {
        private readonly Dictionary<Time, Event> _points;

        public DictionaryTimeline()
        {
            _points = new();
        }

        public Event this[Time time]
        {
            get => _points[time];
            set => _points[time] = value;
        }

        public Point<Time, Event> First()
        {
            var firstTime = _points.Keys.OrderBy(x => x).First();

            return new Point<Time, Event>
            {
                Time = firstTime,
                Event = _points[firstTime],
            };
        }

        public Point<Time, Event> MostRecent(Time time)
        {
            var mostRecentTime = _points.Keys.Where(t => t.CompareTo(time) <= 0)
                .OrderByDescending(t => t)
                .First();

            return new Point<Time, Event>
            {
                Time = mostRecentTime,
                Event = _points[mostRecentTime],
            };
        }

        public bool HasEvent(Time time)
        {
            return _points.ContainsKey(time);
        }

        public IEnumerator<Point<Time, Event>> GetEnumerator()
        {
            foreach (var entry in _points.OrderBy(point => point.Key))
            {
                yield return new Point<Time, Event>
                {
                    Time = entry.Key,
                    Event = entry.Value,
                };
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}