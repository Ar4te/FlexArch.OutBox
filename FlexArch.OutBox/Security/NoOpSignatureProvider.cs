using FlexArch.OutBox.Abstractions;

namespace FlexArch.OutBox.Core.Security;

/// <summary>
/// 空操作签名提供器，用于禁用签名时的占位实现
/// </summary>
public class NoOpSignatureProvider : ISignatureProvider
{
    /// <summary>
    /// 不执行任何签名操作，返回空字符串
    /// </summary>
    /// <param name="content">内容（忽略）</param>
    /// <returns>空字符串</returns>
    public string Sign(string content)
    {
        return string.Empty;
    }

    /// <summary>
    /// 不执行任何验证，总是返回true（禁用签名验证）
    /// </summary>
    /// <param name="content">内容（忽略）</param>
    /// <param name="signature">签名（忽略）</param>
    /// <returns>总是返回true</returns>
    public bool Verify(string content, string signature)
    {
        return true; // 禁用签名时总是通过验证
    }
}
