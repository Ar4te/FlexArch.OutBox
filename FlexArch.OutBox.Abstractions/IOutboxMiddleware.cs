using FlexArch.OutBox.Abstractions.IModels;

namespace FlexArch.OutBox.Abstractions;

/// <summary>
/// OutBox 中间件委托，用于处理管道中的下一个步骤
/// </summary>
/// <param name="message">要处理的 OutBox 消息</param>
/// <returns>处理操作的任务</returns>
public delegate Task OutboxPublishDelegate(IOutboxMessage message);

/// <summary>
/// OutBox 中间件接口，支持在消息发布管道中添加横切关注点
/// </summary>
public interface IOutboxMiddleware
{
    /// <summary>
    /// 异步执行中间件逻辑，并调用管道中的下一个步骤
    /// </summary>
    /// <param name="message">要处理的 OutBox 消息</param>
    /// <param name="next">管道中的下一个步骤</param>
    /// <returns>中间件执行的任务</returns>
    /// <exception cref="ArgumentNullException">当 message 或 next 为 null 时抛出</exception>
    public Task InvokeAsync(IOutboxMessage message, OutboxPublishDelegate next);
}
