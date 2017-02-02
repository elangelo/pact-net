using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PactNet.Mocks.MockHttpService.Matchers;
using PactNet.Mocks.MockHttpService.Matchers.Regex;
using PactNet.Mocks.MockHttpService.Matchers.Type;

namespace PactNet.Matchers
{
    [JsonConverter(typeof(MatcherConverter))]
    public interface IMatcher
    {
        [JsonProperty("match")]
        string Type { get; }

        MatcherResult Match(string path, JToken expected, JToken actual);
    }

    public class MatcherConverter : JsonConverter
    {
        public override bool CanWrite => false;
        public override bool CanRead => true;
        public override bool CanConvert(System.Type objectType)
        {
            return objectType == typeof(IMatcher);
        }
        public override void WriteJson(JsonWriter writer,
            object value, JsonSerializer serializer)
        {
            throw new InvalidOperationException("Use default serialization.");
        }

        public override object ReadJson(JsonReader reader,
            System.Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            var matcher = default(IMatcher);
            switch (jsonObject["match"].Value<string>())
            {
                case "regex":
                    matcher = new RegexMatcher(jsonObject["regex"].Value<string>());
                    break;
                case "type":
                    matcher = new TypeMatcher();
                    break;
                case "default":
                    matcher = new DefaultHttpBodyMatcher(false);
                    break;
            }
            serializer.Populate(jsonObject.CreateReader(), matcher);
            return matcher;
        }
    }
}