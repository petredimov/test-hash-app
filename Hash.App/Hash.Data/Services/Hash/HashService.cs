using System.Security.Cryptography;
using System.Text;
using Hash.Data.Context;
using Hash.Data.Context.Entities;
using Hash.Data.Events;
using Hash.Data.ViewModels;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hash.Data.Services;

public class HashService (ILogger<HashService> logger, IPublishEndpoint publishEndpoint, DatabaseContext context) : IHashService
{
    private string GenerateRandomBase64Sha1Hash()
    {
        using (var sha1 = new SHA1Managed())
        {
            var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()));
            return Convert.ToBase64String(sha1.ComputeHash(hash));
        }
    }
    
    public void GenerateHashesAsync(int count, int batchSize = 1000)
    {
        try
        {
            var batchData = new List<string>();
            var skip = batchSize;
            
            for (var i = 0; i < count; i++)
            {
                batchData.Add(GenerateRandomBase64Sha1Hash());

                if (i < skip && i != count - 1) continue;
                publishEndpoint.Publish(new HashEvent()
                {
                    Data = batchData,
                });
                    
                batchData = [];
                skip += batchSize;
            }
        }
        catch (Exception ex)
        {
            logger.LogError("Exception occured at GenerateHashesAsync (HashService)");
        }
    }

    public async Task AddHashListAsync(List<HashModel> data)
    {
        context.Hashes.AddRange(data);
        await context.SaveChangesAsync();
    }
}