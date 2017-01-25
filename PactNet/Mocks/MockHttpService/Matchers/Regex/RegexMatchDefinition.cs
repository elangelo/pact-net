﻿using Newtonsoft.Json;
using PactNet.Matchers;

namespace PactNet.Mocks.MockHttpService.Matchers.Regex
{
    public class RegexMatchDefinition : MatchDefinition
    {
        public const string Name = "regex";

        [JsonProperty("regex")]
        public string Regex { get; protected set; }

        public RegexMatchDefinition(object example, string regex) :
            base(Name, example)
        {
            Regex = regex;
        }
    }
}