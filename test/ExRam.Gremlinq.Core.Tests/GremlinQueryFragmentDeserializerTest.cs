﻿using System.Dynamic;
using ExRam.Gremlinq.Core.Deserialization;
using FluentAssertions;
using Newtonsoft.Json.Linq;

namespace ExRam.Gremlinq.Core.Tests
{
    public class GremlinQueryFragmentDeserializerTest : GremlinqTestBase
    {
        public GremlinQueryFragmentDeserializerTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {

        }

        [Fact]
        public async Task Empty()
        {
            await Verify(GremlinQueryFragmentDeserializer.Identity
                .TryDeserialize<string>().From("serialized", GremlinQueryEnvironment.Empty));
        }

        [Fact]
        public async Task Base_type()
        {
            await Verify(GremlinQueryFragmentDeserializer.Identity
                .Override<object>((serialized, type, env, overridden, recurse) => "overridden")
                .TryDeserialize<string>().From("serialized", GremlinQueryEnvironment.Empty));
        }

        [Fact]
        public async Task Irrelevant()
        {
            await Verify(GremlinQueryFragmentDeserializer.Identity
                .Override<JObject>((serialized, type, env, overridden, recurse) => "should not be here")
                .TryDeserialize<string>().From("serialized", GremlinQueryEnvironment.Empty));
        }

        [Fact]
        public async Task Override1()
        {
            await Verify(GremlinQueryFragmentDeserializer.Identity
                .Override<string>((serialized, type, env, overridden, recurse) => overridden("overridden", type, env, recurse))
                .TryDeserialize<string>().From("serialized", GremlinQueryEnvironment.Empty));
        }

        [Fact]
        public async Task Override2()
        {
            await Verify(GremlinQueryFragmentDeserializer.Identity
                .Override<string>((serialized, type, env, overridden, recurse) => overridden("overridden 1", type, env, recurse))
                .Override<string>((serialized, type, env, overridden, recurse) => overridden("overridden 2", type, env, recurse))
                .TryDeserialize<string>().From("serialized", GremlinQueryEnvironment.Empty));
        }

        [Fact]
        public async Task Recurse()
        {
            await Verify(GremlinQueryFragmentDeserializer.Identity
                .Override<string>((serialized, type, env, overridden, recurse) => recurse.TryDeserialize(type).From(36, env))
                .TryDeserialize<int>().From("serialized", GremlinQueryEnvironment.Empty));
        }

        [Fact]
        public void Recurse_wrong_type()
        {
            GremlinQueryFragmentDeserializer.Identity
                .Override<string>((serialized, type, env, overridden, recurse) => recurse.TryDeserialize(type).From(36, env))
                .Invoking(_ => _
                    .TryDeserialize<string>().From("serialized", GremlinQueryEnvironment.Empty))
                .Should()
                .Throw<ArgumentException>();
        }

        [Fact]
        public async Task Recurse_to_previous_override()
        {
            await Verify(GremlinQueryFragmentDeserializer.Identity
                .Override<int>((serialized, type, env, overridden, recurse) => overridden(37, type, env, recurse))
                .Override<string>((serialized, type, env, overridden, recurse) => recurse.TryDeserialize(type).From(36, env))
                .TryDeserialize<int>().From("serialized", GremlinQueryEnvironment.Empty));
        }

        [Fact]
        public async Task Recurse_to_later_override()
        {
            await Verify(GremlinQueryFragmentDeserializer.Identity
                .Override<string>((serialized, type, env, overridden, recurse) => recurse.TryDeserialize(type).From(36, env))
                .Override<int>((serialized, type, env, overridden, recurse) => overridden(37, type, env, recurse))
                .TryDeserialize<int>().From("serialized", GremlinQueryEnvironment.Empty));
        }

        [Fact]
        public async Task More_specific_type_is_deserialized()
        {
            await Verify(GremlinQueryFragmentDeserializer.Identity
                .AddNewtonsoftJson()
                .TryDeserialize<object>().From(JObject.Parse("{ \"@type\": \"g:Date\", \"@value\": 1657527969000 }"), GremlinQueryEnvironment.Empty));
        }

        [Fact]
        public async Task JObject_is_not_changed()
        {
            var original = JObject.Parse("{ \"prop1\": \"value\", \"prop2\": 1657527969000 }");

            var deserialized = GremlinQueryFragmentDeserializer.Identity
                .AddNewtonsoftJson()
                .TryDeserialize<JObject>().From(original, GremlinQueryEnvironment.Empty);

            deserialized
                .Should()
                .BeSameAs(original);
        }

        [Fact]
        public async Task Request_for_Dictionary_yields_expandoObject()
        {
            var original = JObject.Parse("{ \"prop1\": \"value\", \"prop2\": 1657527969000 }");

            var deserialized = GremlinQueryFragmentDeserializer.Identity
                .AddNewtonsoftJson()
                .TryDeserialize<IDictionary<string, object>>().From(original, GremlinQueryEnvironment.Empty);

            deserialized
                .Should()
                .BeOfType<ExpandoObject>();

            await Verify(deserialized);
        }

        [Fact]
        public async Task Request_for_Dictionary_yields_expandoObject_from_typed_GraphSON()
        {
            var original = JObject.Parse("{ \"@type\": \"g:unknown\", \"@value\": { \"prop1\": \"value\", \"prop2\": 1657527969000 } }");

            var deserialized = GremlinQueryFragmentDeserializer.Identity
                .AddNewtonsoftJson()
                .TryDeserialize<IDictionary<string, object>>().From(original, GremlinQueryEnvironment.Empty);

            deserialized
                .Should()
                .BeOfType<ExpandoObject>();

            await Verify(deserialized);
        }

        [Fact]
        public async Task Overridden_request_for_Dictionary_yields_dictionary()
        {
            var original = JObject.Parse("{ \"prop1\": \"value\", \"prop2\": 1657527969000 }");

            var deserialized = GremlinQueryFragmentDeserializer.Identity
                .AddNewtonsoftJson()
                .Override<JObject, IDictionary<string, object?>>(GremlinQueryFragmentDeserializerDelegate<JObject>.From(static (jObject, type, env, overridden, recurse) =>
                {
                    if (recurse.TryDeserialize<JObject>().From(jObject, env) is JObject processedFragment)
                    {
                        var dict = new Dictionary<string, object?>();

                        foreach (var property in processedFragment)
                        {
                            dict.TryAdd(property.Key, recurse.TryDeserialize<object>().From(property.Value, env));
                        }

                        return dict;
                    }

                    return overridden(jObject, type, env, recurse);
                }))
                .TryDeserialize<IDictionary<string, object>>().From(original, GremlinQueryEnvironment.Empty);

            deserialized
                .Should()
                .BeOfType<Dictionary<string, object?>>();

            await Verify(deserialized);
        }
    }
}
