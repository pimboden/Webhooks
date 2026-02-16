using System.Collections.ObjectModel;
using Webhooks.Api.Models;

namespace Webhooks.Api.Repositories;

public class InMemorySampleDataRepository
{
    private readonly List<SampleData> _sampleDataItems = [];

    public Task AddAsync(SampleData sampleData, CancellationToken cancellationToken)
    {
        _sampleDataItems.Add(sampleData);
        return Task.CompletedTask;
    }

    public Task<ReadOnlyCollection<SampleData>> GetAllAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(_sampleDataItems.AsReadOnly());
    }
}