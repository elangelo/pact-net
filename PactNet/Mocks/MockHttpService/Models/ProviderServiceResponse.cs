﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using PactNet.Configuration.Json.Converters;
using PactNet.Matchers;
using PactNet.Mocks.MockHttpService.Matchers;
using PactNet.Mocks.MockHttpService.Matchers.Regex;
using PactNet.Mocks.MockHttpService.Matchers.Type;

namespace PactNet.Mocks.MockHttpService.Models
{
    public class ProviderServiceResponse : IHttpMessage
    {
        private bool _bodyWasSet;
        private dynamic _body;

        [JsonProperty(PropertyName = "status")]
        public int Status { get; set; }

        [JsonProperty(PropertyName = "headers")]
        [JsonConverter(typeof(PreserveCasingDictionaryConverter))]
        public IDictionary<string, string> Headers { get; set; }

        [JsonProperty(PropertyName = "matchingRules")]
        public IDictionary<string, IMatcher> MatchingRules { get; private set; }

        [JsonProperty(PropertyName = "body", NullValueHandling = NullValueHandling.Include)]
        public dynamic Body
        {
            get { return _body; }
            set
            {
                _bodyWasSet = true;
                _body = ParseBodyMatchingRules(value);
            }
        }

        // A not so well known feature in JSON.Net to do conditional serialization at runtime
        public bool ShouldSerializeBody()
        {
            return _bodyWasSet;
        }

        private dynamic ParseBodyMatchingRules(dynamic body)
        {
            if (MatchingRules == null)
                MatchingRules = new Dictionary<string, IMatcher>
                {
                    {DefaultHttpBodyMatcher.Path, new DefaultHttpBodyMatcher(true)}
                };

            if (body == null)
            {
                return null;
            }

            var bodyToken = JToken.FromObject(body);

            if (bodyToken is JValue)
            {
                return body;
            }

            var matcherTypes = ((JToken)bodyToken).SelectTokens("$..*.$pactMatcherType").ToList();

            if (!matcherTypes.Any())
            {
                return body;
            }

            var matchersToRemove = new Stack<dynamic>();

            var matcherFactory = new Dictionary<string, Func<JContainer, IMatcher>>()
            {
                { RegexMatchDefinition.Name, props => new RegexMatcher(props["regex"].Value<string>()) },
                { TypeMatchDefinition.Name, props => new TypeMatcher() }
            };

            foreach (var matcherType in matcherTypes.Where(x => x is JValue).Cast<JValue>())
            {
                var matcherDefinition = matcherType.Parent.Parent;
                var example = matcherDefinition["example"].Value<dynamic>();

                matchersToRemove.Push(new { Path = matcherDefinition.Path, Example = example });
                MatchingRules.Add(matcherDefinition.Path, matcherFactory[matcherDefinition["$pactMatcherType"].Value<string>()](matcherDefinition));
            }

            foreach (var item in matchersToRemove)
            {
                bodyToken.SelectToken(item.Path).Replace(item.Example);
            }

            //http://blog.petegoo.com/2009/10/26/using-json-net-to-eval-json-into-a-dynamic-variable-in/
            //http://www.tomdupont.net/2014/02/deserialize-to-expandoobject-with.html
            //http://gotoanswer.com/?q=Deserialize+json+object+into+dynamic+object+using+Json.net

            return bodyToken is JArray
                ? JsonConvert.DeserializeObject<IEnumerable<ExpandoObject>>(bodyToken.ToString(), new ExpandoObjectConverter())
                : JsonConvert.DeserializeObject<ExpandoObject>(bodyToken.ToString(), new ExpandoObjectConverter());
        }
    }
}