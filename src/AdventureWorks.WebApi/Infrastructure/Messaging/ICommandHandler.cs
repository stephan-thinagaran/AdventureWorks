using System;

namespace AdventureWorks.WebApi.Infrastructure.Messaging;

public interface ICommandHandler<in TCommand, TCommandResult>
{
    Task<TCommandResult?> Handle(TCommand command, CancellationToken cancellation);
}

public interface ICommandDispatcher
{
    Task<TCommandResult?> Dispatch<TCommand, TCommandResult>(TCommand command, CancellationToken cancellationToken);
}

public interface ICommand;

public interface ICommandResult;
