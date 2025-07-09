namespace FlexArch.OutBox.Core.Options;

/// <summary>
/// 消息签名配置选项
/// </summary>
public class SigningOptions
{
    /// <summary>
    /// 签名密钥，建议从配置或密钥管理服务获取
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// 是否启用消息签名，默认启用
    /// </summary>
    public bool EnableSigning { get; set; } = true;

    /// <summary>
    /// 签名算法类型，默认HMACSHA256
    /// </summary>
    public string Algorithm { get; set; } = "HMACSHA256";

    /// <summary>
    /// 验证配置是否有效
    /// </summary>
    public void Validate()
    {
        if (EnableSigning && string.IsNullOrWhiteSpace(SecretKey))
        {
            throw new InvalidOperationException("SigningOptions.SecretKey cannot be null or empty when signing is enabled");
        }

        if (EnableSigning && SecretKey.Length < 32)
        {
            throw new InvalidOperationException("SigningOptions.SecretKey should be at least 32 characters for security");
        }
    }
}
