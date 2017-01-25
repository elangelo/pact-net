using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PactNet.Matchers.Regex
{
    public class RegexMatcher : IMatcher
    {
        [JsonIgnore]
        public string Type
        {
            get { return RegexMatchDefinition.Name; }
        }

        [JsonProperty("regex")]
        public string Regex { get; protected set; }
        
        public RegexMatcher(string regex)
        {
            Regex = regex;
        }

        public MatcherResult Match(string path, JToken expected, JToken actual)
        {
            var act = actual as JValue;
            var matches = act != null && System.Text.RegularExpressions.Regex.IsMatch(act.Value.ToString(), Regex);

            return matches ?
                new MatcherResult(new SuccessfulMatcherCheck(path, this.Regex, act.Value)) :
                new MatcherResult(new FailedMatcherCheck(path, MatcherCheckFailureType.ValueDoesNotMatch, this.Regex, act.Value));
        }
    }
}