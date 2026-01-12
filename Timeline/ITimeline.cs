using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Timeline;

public interface ITimeline<Time, Event> : IEnumerable<ITimeline<Time, Event>.Point>
    where Time : notnull, IComparable<Time>
{
    bool HasEvent(Time time);
    Event[] this[Time time] { get; }

    /// <summary>
    /// Return the next point in the timeline that is after <paramref name="time"/>.
    /// </summary>
    /// Essentially, this is
    ///   (----------]
    ///  time       next
    Point GetNext(Time time);

    /// <summary>
    /// Return the previous point in the timeline that is before <paramref name="time"/>
    /// </summary>
    /// Essentially, this is
    ///    [----------)
    ///  prev        time
    Point GetPrevious(Time time);

    public readonly struct Point
    {
        public Time Time { get; init; }
        public Event[] Events { get; init; }

        public override string ToString()
        {
            var events = string.Join(", ", Events.Select(value => value?.ToString()));

            return $"{Time} = {events}";
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj is Point point)
            {
                return Time.Equals(point.Time) && Events.SequenceEqual(point.Events);
            }

            return base.Equals(obj);
        }

        public static bool operator ==(Point left, Point right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Point left, Point right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return Time.GetHashCode() ^ Events.GetHashCode();
        }
    }
}
