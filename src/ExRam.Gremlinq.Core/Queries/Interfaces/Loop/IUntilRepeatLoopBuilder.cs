﻿#pragma warning disable IDE0003
// ReSharper disable ArrangeThisQualifier

namespace ExRam.Gremlinq.Core
{
    public interface IUntilRepeatLoopBuilder<TQuery> : IFinalLoopBuilder<TQuery>
        where TQuery : IGremlinQueryBase
    {
        IUntilRepeatEmitLoopBuilder<TQuery> Emit();
    }
}
