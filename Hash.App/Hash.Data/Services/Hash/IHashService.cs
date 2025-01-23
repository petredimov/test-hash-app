using Hash.Data.Context.Entities;
using Hash.Data.Events;
using Hash.Data.ViewModels;

namespace Hash.Data.Services;

public interface IHashService
{
    void GenerateHashesAsync(int count, int batchSize = 1000);
    Task AddHashListAsync(List<HashModel> data);
}