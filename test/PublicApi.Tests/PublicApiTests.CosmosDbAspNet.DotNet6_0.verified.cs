﻿namespace ExRam.Gremlinq.Core.AspNet
{
    public static class GremlinqSetupExtensions
    {
        public static ExRam.Gremlinq.Providers.Core.AspNet.IGremlinqProviderServicesBuilder<ExRam.Gremlinq.Providers.CosmosDb.ICosmosDbConfigurator<TVertexBase>> UseCosmosDb<TVertexBase, TEdgeBase>(this ExRam.Gremlinq.Core.AspNet.IGremlinqServicesBuilder setup) { }
    }
}