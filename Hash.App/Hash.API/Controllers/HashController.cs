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