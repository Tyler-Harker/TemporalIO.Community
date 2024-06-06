using TemporalIO.Community.Actors.Interfaces;
using Temporalio.Workflows;

namespace TemporalIO.Community.Actors.AbstractImplementations;

public abstract class BaseActorWorkflow<TWorkflow, TModel> : IActorWorkflow<TWorkflow, TModel>
    where TModel : class, new()
    where TWorkflow : IActorWorkflow<TModel>
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public TModel Model { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    protected int NumberOfQueuedUpdates { get; set; }

    public void UpdateIndexes()
    {
        var indexes = this.GetIndexes();
        if (indexes.Count() > 0)
        {
            Workflow.UpsertTypedSearchAttributes(GetIndexes());
        }
    }

    public SearchAttributeUpdate[] GetIndexes() => [];

    public async Task RunAsync(TModel? model = null)
    {
        if (model is null)
        {
            model = new();
        }

        this.Model = model;

        this.UpdateIndexes();

        await Workflow.WaitConditionAsync(
            () => Workflow.ContinueAsNewSuggested && this.NumberOfQueuedUpdates == 0
        );

        Workflow.CreateContinueAsNewException<TWorkflow>(workflow => workflow.RunAsync(this.Model));
    }

    protected async Task UpdateAsync(Func<Task> action)
    {
        this.NumberOfQueuedUpdates++;
        await Workflow.WaitConditionAsync(() => this.NumberOfQueuedUpdates == 1);

        try
        {
            await action();
            this.NumberOfQueuedUpdates--;
        }
        catch (Exception ex)
        {
            this.NumberOfQueuedUpdates--;
            throw;
        }
    }

    protected async Task<TResponse> UpdateAsync<TResponse>(Func<Task<TResponse>> action)
    {
        this.NumberOfQueuedUpdates++;
        await Workflow.WaitConditionAsync(() => this.NumberOfQueuedUpdates == 1);

        TResponse response;
        try
        {
            response = await action();
            this.NumberOfQueuedUpdates--;
        }
        catch (Exception ex)
        {
            this.NumberOfQueuedUpdates--;
            throw;
        }

        return response;
    }
}
