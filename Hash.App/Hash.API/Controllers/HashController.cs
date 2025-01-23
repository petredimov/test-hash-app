using Hash.Data.Models;
using Hash.Data.Services;
using Microsoft.AspNetCore.Mvc;

namespace Hash.API.Controllers;

[Route("hashes")]
[ApiController]
public class HashController (IHashService hashService, IHashAnalyticsService hashAnalyticsService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Post(PostHashesInputModel? model)
    {
        model ??= new PostHashesInputModel();

        if (model.BatchSize == 0)
        {
            model.BatchSize = 1000;
        }

        if (model.NumOfMessages == 0)
        {
            model.NumOfMessages = 40000;
        }
        
        hashService.GenerateHashesAsync(model.NumOfMessages, model.BatchSize);
        return Ok(new PostHashesOutputModel() { Published = model.NumOfMessages, BatchSize = model.BatchSize});
    }
    
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var model = hashAnalyticsService.GetHashAnalytics();
        return Ok(model);
    }
}