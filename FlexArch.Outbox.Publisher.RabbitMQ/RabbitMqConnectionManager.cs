using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace FlexArch.Outbox.Publisher.RabbitMQ;

/// <summary>
/// RabbitMQ连接管理器，负责管理连接的创建和生命周期
/// </summary>
public class RabbitMqConnectionManager : IDisposable
{
    private readonly IConnectionFactory _connectionFactory;
    private readonly ILogger<RabbitMqConnectionManager> _logger;
    private readonly SemaphoreSlim _connectionSemaphore = new(1, 1);
    private IConnection? _connection;
    private bool _disposed;

    public RabbitMqConnectionManager(IConnectionFactory connectionFactory, ILogger<RabbitMqConnectionManager> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    /// <summary>
    /// 获取RabbitMQ连接，如果连接不存在或已关闭则创建新连接
    /// </summary>
    /// <returns>RabbitMQ连接实例</returns>
    public async Task<IConnection> GetConnectionAsync()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(RabbitMqConnectionManager));
        }

        await _connectionSemaphore.WaitAsync();
        try
        {
            if (_connection == null || !_connection.IsOpen)
            {
                if (_connection != null && !_connection.IsOpen)
                {
                    _logger.LogWarning("RabbitMQ connection is closed, creating new connection");
                    _connection.Dispose();
                }

                _logger.LogInformation("Creating new RabbitMQ connection");
                _connection = await _connectionFactory.CreateConnectionAsync();
                _logger.LogInformation("RabbitMQ connection created successfully");
            }

            return _connection;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create RabbitMQ connection");
            throw;
        }
        finally
        {
            _connectionSemaphore.Release();
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _connectionSemaphore.Wait();
        try
        {
            _connection?.Dispose();
            _connection = null;
        }
        finally
        {
            _connectionSemaphore.Release();
            _connectionSemaphore.Dispose();
        }

        _disposed = true;
    }
}
