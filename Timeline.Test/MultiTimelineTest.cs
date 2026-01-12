using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CsCheck;

namespace Timeline.Test;

public class MultiTimelineTest
{
    [Test]
    public async Task InterleaveTimes()
    {
        await Gen.Int.ArrayUnique[1, 5].SampleAsync(async (times) =>
        {
            await Gen.OneOfConst(times).Array2D[Gen.Int[1, 10], Gen.Int[1, 10]].SampleAsync(async (multipleTimes) =>
            {
                var timelines = new List<DictionaryTimeline<int, int>>();

                for (var i = 0; i < multipleTimes.GetLength(0); i++)
                {
                    var timeline = new DictionaryTimeline<int, int>();
                    for (var j = 0; j < multipleTimes.GetLength(1); j++)
                    {
                        var time = multipleTimes[i, j];
                        timeline.Add(time, time);
                    }

                    timelines.Add(timeline);
                }

                var multiTimeline = new MultiTimeline<int, int>(timelines);

                await Assert.That(multiTimeline.Select(point => point.Time)).IsInOrder();
            });
        });
    }
}