using System;

namespace Timeline;

public interface IMutableTimeline<Time, Event> : ITimeline<Time, Event>
    where Time : IComparable<Time>
{
    void Add(Time time, Event value);
}
