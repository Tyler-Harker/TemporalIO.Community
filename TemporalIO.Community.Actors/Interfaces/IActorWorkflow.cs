namespace TemporalIO.Community.Actors.Interfaces;

public interface IActorWorkflow { }

public interface IActorWorkflow<TModel> : IActorWorkflow
    where TModel : class, new()
{
    TModel Model { get; set; }
    Task RunAsync(TModel? model = null);
}

public interface IActorWorkflow<TWorkflow, TModel> : IActorWorkflow<TModel>
    where TModel : class, new() { }
