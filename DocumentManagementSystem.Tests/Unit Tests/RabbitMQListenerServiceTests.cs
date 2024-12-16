using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using dms_bl.Services;
using dms_bl.Exceptions;
using RabbitMQ.Client.Exceptions;

public class RabbitMqListenerServiceTests
{
    private readonly Mock<IConnection> _mockConnection;
    private readonly Mock<IModel> _mockChannel;
    private readonly Mock<ILogger<RabbitMqListenerService>> _mockLogger;
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly RabbitMqListenerService _listenerService;

    public RabbitMqListenerServiceTests()
    {
        _mockConnection = new Mock<IConnection>();
        _mockChannel = new Mock<IModel>();
        _mockLogger = new Mock<ILogger<RabbitMqListenerService>>();
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();

        // Mock CreateModel method to return the mocked channel
        _mockConnection.Setup(conn => conn.CreateModel()).Returns(_mockChannel.Object);
        _mockConnection.Setup(conn => conn.IsOpen).Returns(true);  // Simulate the connection being open

        // Initialize the service with the mocks
        _listenerService = new RabbitMqListenerService(_mockLogger.Object, _mockHttpClientFactory.Object);
    }

    [Fact]
    public async Task StartAsync_ShouldThrowQueueException_WhenConnectionFails()
    {
        // Arrange: Mock the connection to always fail
        _mockConnection.Setup(conn => conn.CreateModel()).Throws(new Exception("Could not connect"));
        _mockConnection.Setup(conn => conn.IsOpen).Returns(false); // Simulate the connection failing

        // Act & Assert: Ensure that the QueueException is thrown
        var exception = await Assert.ThrowsAsync<QueueException>(() => _listenerService.StartAsync(CancellationToken.None));
        Assert.Contains("Connection was not possible, 5 tries failed.", exception.Message);
    }
}
