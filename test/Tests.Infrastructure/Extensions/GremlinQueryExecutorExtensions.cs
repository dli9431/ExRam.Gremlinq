﻿using Newtonsoft.Json.Linq;

namespace ExRam.Gremlinq.Core.Execution
{
    public static class GremlinQueryExecutorExtensions
    {
        private sealed class CatchingGremlinQueryExecutor : IGremlinQueryExecutor
        {
            private readonly IGremlinQueryExecutor _baseExecutor;

            public CatchingGremlinQueryExecutor(IGremlinQueryExecutor baseExecutor)
            {
                _baseExecutor = baseExecutor;
            }

            public IAsyncEnumerable<T> Execute<T>(GremlinQueryExecutionContext context) => _baseExecutor
                .Execute<T>(context)
                .Catch<T, Exception>(ex => typeof(T).IsAssignableFrom(typeof(JObject))
                    ? AsyncEnumerableEx
                        .Return((T)(object)new JObject()
                        {
                            {
                                "serverException",
                                new JObject
                                {
                                    { "type", ex.GetType().Name },
                                    { "message", ex.Message }
                                }
                            }
                        })
                    : AsyncEnumerable.Empty<T>());
        }

        public static IGremlinQueryExecutor CatchExecutionExceptions(this IGremlinQueryExecutor executor) => new CatchingGremlinQueryExecutor(executor);
    }
}
