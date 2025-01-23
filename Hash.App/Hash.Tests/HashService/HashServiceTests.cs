using Hash.Data.Context;
using Hash.Data.Events;
using Hash.Data.Services;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;

namespace Hash.Tests;

public class HashServiceTests
{
    IHashService? _service;
    private Mock<IPublishEndpoint> mockPublishEndpoint = null;
    
    [SetUp]
    public void Setup()
    {
        var mockedContext = new Mock<DatabaseContext>();
        mockPublishEndpoint = new Mock<IPublishEndpoint>();
        mockPublishEndpoint.Setup(x => x.Publish(It.IsAny<HashEvent>(), It.IsAny<CancellationToken>())).Returns(() => Task.CompletedTask);
        
        var mockLogger = new Mock<ILogger<HashService>>();
        _service = new HashService(mockLogger.Object, mockPublishEndpoint.Object, mockedContext.Object);
    }

    [Test]
    public void GenerateHashesAsyncValid()
    {
        _service.GenerateHashesAsync(5000);
        mockPublishEndpoint.Verify(x => x.Publish(It.IsAny<HashEvent>(), It.IsAny<CancellationToken>()), Times.Exactly(5));
    }   
    
    [Test]
    public void GenerateHashesAsyncInvalid()
    {
        _service.GenerateHashesAsync(0);
        mockPublishEndpoint.Verify(x => x.Publish(It.IsAny<HashEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}