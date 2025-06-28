using System;

namespace AdventureWorks.WebApi.Infrastructure.Messaging;

public interface IQueryHandler<in TQuery, TQueryResult>
{
    Task<TQueryResult?> Handle(TQuery query, CancellationToken cancellationToken);
}

public interface IQueryDispatcher
{
    Task<TQueryResult?> Dispatch<TQuery, TQueryResult>(TQuery query, CancellationToken cancellationToken);
}

public interface IQuery;

public interface IQueryResult;
