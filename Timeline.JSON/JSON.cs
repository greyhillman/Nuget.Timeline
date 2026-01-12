using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Timeline.JSON
{
    public class ObjectTimelineConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            if (!typeToConvert.IsGenericType)
            {
                return false;
            }
            return typeToConvert.GetGenericTypeDefinition() == typeof(DictionaryTimeline<,>);
        }

        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var timeType = typeToConvert.GetGenericArguments()[0];
            var eventType = typeToConvert.GetGenericArguments()[1];

            JsonConverter converter = (JsonConverter)Activator.CreateInstance(
                typeof(ObjectTimelineConverter<,>).MakeGenericType(new[]
                {
                    timeType, eventType
                }),
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                args: new object[] { options },
                culture: null)!;

            return converter;
        }
    }

    public class ObjectTimelineConverter<Time, Event> : JsonConverter<DictionaryTimeline<Time, Event>>
        where Time : notnull, IComparable<Time>
    {
        private readonly JsonConverter<Time> _timeConverter;
        private readonly JsonConverter<Event> _eventConverter;

        public ObjectTimelineConverter(JsonSerializerOptions options)
        {
            _timeConverter = (JsonConverter<Time>)options.GetConverter(typeof(Time));
            _eventConverter = (JsonConverter<Event>)options.GetConverter(typeof(Event));
        }

        public override DictionaryTimeline<Time, Event>? Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            var timeline = new DictionaryTimeline<Time, Event>();

            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                var time = _timeConverter.ReadAsPropertyName(ref reader, typeof(Time), options);
                reader.Read();
                var @event = _eventConverter.Read(ref reader, typeof(Event), options);

                if (time == null || @event == null)
                {
                    throw new JsonException();
                }

                timeline.Add(time, @event);
            }

            return timeline;
        }

        public override void Write(
            Utf8JsonWriter writer,
            DictionaryTimeline<Time, Event> value,
            JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            foreach (var entry in value)
            {
                _timeConverter.WriteAsPropertyName(writer, entry.Time, options);
                writer.WriteStartArray();
                foreach (var @event in entry.Events)
                {
                    _eventConverter.Write(writer, @event, options);
                }
                writer.WriteEndArray();
            }

            writer.WriteEndObject();
        }
    }
}
