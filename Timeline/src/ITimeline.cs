using System.Collections.Generic;

namespace Timeline
{
    public interface ITimeline<Time, Event> : IEnumerable<Point<Time, Event>>
    {
        Event this[Time time] { get; set; }

        bool HasEvent(Time time);
        Point<Time, Event> MostRecent(Time time);
        Point<Time, Event> First();
    }

    public struct Point<K, E>
    {
        public K Time { get; init; }
        public E Event { get; init; }
    }
}
