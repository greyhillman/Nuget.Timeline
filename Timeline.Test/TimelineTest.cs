using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CsCheck;
using TUnit.Assertions.Enums;

namespace Timeline.Test;

public class TimelineTest
{
    [Test]
    public async Task GetNext()
    {
        var gen = (
            from target in Gen.Int[-100, 100]
            from time in Gen.Int[-200, target - 1]
            select (target, time)
        );

        await gen.SampleAsync(async (initial) =>
        {
            (var target, var time) = initial;

            var gen = (
                from lower in Gen.Int[-400, time].Array[1, 5]
                from upper in Gen.Int[target, 200].Array[1, 5]
                select lower.Concat([target]).Concat(upper).ToArray()
            );

            await gen.SampleAsync(async (xs) =>
            {
                var timeline = new DictionaryTimeline<int, int>();
                foreach (var x in xs)
                {
                    timeline.Add(x, x);
                }

                var point = timeline.GetNext(time);
                await Assert.That(point.Time).IsEqualTo(target);
            });
        });
    }

    [Test]
    public async Task GetNextAfterEnd()
    {
        await Gen.Int.ArrayUnique[1, 128].SampleAsync(async (xs) =>
        {
            var timeline = new DictionaryTimeline<int, int>();
            foreach (var x in xs)
            {
                timeline.Add(x, x);
            }

            var maximum = xs.Max();

            await Assert.That(() => timeline.GetNext(maximum + 1)).Throws<KeyNotFoundException>();
        });
    }

    [Test]
    public async Task OrderedEnumerator()
    {
        await Gen.Int.ArrayUnique.SampleAsync(async (times) =>
        {
            var timeline = new DictionaryTimeline<int, int>();
            foreach (var time in times)
            {
                timeline.Add(time, time);
            }

            var points = timeline.ToArray();

            await Assert.That(points).IsOrderedBy(point => point.Time);

            Array.Sort(times);
            var timelineTimes = points.Select(point => point.Time).ToArray();
            await Assert.That(timelineTimes).IsEquivalentTo(times, CollectionOrdering.Matching);
        });
    }

    [Test]
    public async Task InternalConsistency()
    {
        await Gen.Int.ArrayUnique[2, 10].SampleAsync(async (times) =>
        {
            var timeline = new DictionaryTimeline<int, int>();
            foreach (var time in times)
            {
                timeline.Add(time, time);
            }

            var min = times.Min();
            var max = times.Max();

            await Gen.Int[min, max - 1].SampleAsync(async (target) =>
            {
                var point = timeline.GetNext(target);

                await Assert.That(timeline.HasEvent(point.Time)).IsTrue();
                await Assert.That(timeline[point.Time]).IsNotEmpty();
            });
        });
    }

    [Test]
    public async Task MatchesModelAdd()
    {
        var addOperation = Gen.Int.Operation<DictionaryTimeline<int, int>, ListTimeline<int, int>>(
            (actual, x) => actual.Add(x, x),
            (model, x) => model.Add(x, x)
        );

        Gen.Const(() => (new DictionaryTimeline<int, int>(), new ListTimeline<int, int>()))
            .SampleModelBased(addOperation);
    }
}