using System.Security.Cryptography;
using System.Text;
using FlexArch.OutBox.Abstractions;

namespace FlexArch.OutBox.Core.Security;

/// <summary>
/// HMAC SHA256签名提供器，提供消息签名和验证功能
/// </summary>
public class HmacSha256SignatureProvider : ISignatureProvider
{
    private readonly string _secret;

    public HmacSha256SignatureProvider(string secret)
    {
        if (string.IsNullOrWhiteSpace(secret))
        {
            throw new ArgumentNullException(nameof(secret), "Secret key cannot be null or empty");
        }

        if (secret.Length < 32)
        {
            throw new ArgumentException("Secret key should be at least 32 characters for security", nameof(secret));
        }

        _secret = secret;
    }

    /// <summary>
    /// 对内容进行签名
    /// </summary>
    /// <param name="content">要签名的内容</param>
    /// <returns>Base64编码的签名</returns>
    public string Sign(string content)
    {
        if (string.IsNullOrEmpty(content))
        {
            return string.Empty;
        }

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_secret));
        byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(content));
        return Convert.ToBase64String(hash);
    }

    /// <summary>
    /// 验证内容签名，使用时间常数比较防止时序攻击
    /// </summary>
    /// <param name="content">原始内容</param>
    /// <param name="signature">要验证的签名</param>
    /// <returns>验证是否成功</returns>
    public bool Verify(string content, string signature)
    {
        // null 签名总是无效的
        if (signature == null)
        {
            return false;
        }

        try
        {
            string expected = Sign(content ?? string.Empty);

            // 使用时间常数比较防止时序攻击
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(expected),
                Encoding.UTF8.GetBytes(signature)
            );
        }
        catch
        {
            // 发生任何异常都返回false，避免泄露信息
            return false;
        }
    }
}
