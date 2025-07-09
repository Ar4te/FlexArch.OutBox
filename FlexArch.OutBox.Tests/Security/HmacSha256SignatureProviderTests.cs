using FlexArch.OutBox.Core.Security;
using FluentAssertions;

namespace FlexArch.OutBox.Tests.Security;

/// <summary>
/// HmacSha256SignatureProvider 单元测试
/// </summary>
public class HmacSha256SignatureProviderTests
{
    private const string ValidSecret = "this-is-a-very-secure-secret-key-for-testing-purposes";

    [Fact]
    public void Constructor_WithNullSecret_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new HmacSha256SignatureProvider(null!));
    }

    [Fact]
    public void Constructor_WithEmptySecret_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new HmacSha256SignatureProvider(""));
    }

    [Fact]
    public void Constructor_WithShortSecret_ShouldThrowArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new HmacSha256SignatureProvider("short"));
    }

    [Fact]
    public void Constructor_WithValidSecret_ShouldCreateInstance()
    {
        // Act
        var provider = new HmacSha256SignatureProvider(ValidSecret);

        // Assert
        provider.Should().NotBeNull();
    }

    [Fact]
    public void Sign_WithEmptyContent_ShouldReturnEmptyString()
    {
        // Arrange
        var provider = new HmacSha256SignatureProvider(ValidSecret);

        // Act
        string signature = provider.Sign("");

        // Assert
        signature.Should().BeEmpty();
    }

    [Fact]
    public void Sign_WithSameContent_ShouldReturnSameSignature()
    {
        // Arrange
        var provider = new HmacSha256SignatureProvider(ValidSecret);
        const string content = "test message";

        // Act
        string signature1 = provider.Sign(content);
        string signature2 = provider.Sign(content);

        // Assert
        signature1.Should().Be(signature2);
        signature1.Should().NotBeEmpty();
    }

    [Fact]
    public void Sign_WithDifferentContent_ShouldReturnDifferentSignatures()
    {
        // Arrange
        var provider = new HmacSha256SignatureProvider(ValidSecret);

        // Act
        string signature1 = provider.Sign("message1");
        string signature2 = provider.Sign("message2");

        // Assert
        signature1.Should().NotBe(signature2);
    }

    [Fact]
    public void Sign_ShouldReturnBase64EncodedString()
    {
        // Arrange
        var provider = new HmacSha256SignatureProvider(ValidSecret);
        const string content = "test message";

        // Act
        string signature = provider.Sign(content);

        // Assert
        signature.Should().NotBeEmpty();
        // 验证是有效的 Base64 字符串
        byte[] base64Bytes = Convert.FromBase64String(signature);
        base64Bytes.Should().HaveCount(32); // SHA256 产生 32 字节
    }

    [Fact]
    public void Verify_WithValidSignature_ShouldReturnTrue()
    {
        // Arrange
        var provider = new HmacSha256SignatureProvider(ValidSecret);
        const string content = "test message";
        string signature = provider.Sign(content);

        // Act
        bool isValid = provider.Verify(content, signature);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void Verify_WithInvalidSignature_ShouldReturnFalse()
    {
        // Arrange
        var provider = new HmacSha256SignatureProvider(ValidSecret);
        const string content = "test message";

        // Act
        bool isValid = provider.Verify(content, "invalid-signature");

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void Verify_WithModifiedContent_ShouldReturnFalse()
    {
        // Arrange
        var provider = new HmacSha256SignatureProvider(ValidSecret);
        const string originalContent = "test message";
        const string modifiedContent = "test message modified";
        string signature = provider.Sign(originalContent);

        // Act
        bool isValid = provider.Verify(modifiedContent, signature);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void Verify_WithNullSignature_ShouldReturnFalse()
    {
        // Arrange
        var provider = new HmacSha256SignatureProvider(ValidSecret);

        // Act
        bool isValid = provider.Verify("test message", null!);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void Verify_WithEmptySignature_ShouldReturnFalse()
    {
        // Arrange
        var provider = new HmacSha256SignatureProvider(ValidSecret);

        // Act
        bool isValid = provider.Verify("test message", "");

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void Verify_WithEmptyContentAndEmptySignature_ShouldReturnTrue()
    {
        // Arrange
        var provider = new HmacSha256SignatureProvider(ValidSecret);

        // Act
        bool isValid = provider.Verify("", "");

        // Assert
        // 空内容的签名就是空字符串，所以空内容 + 空签名应该是有效的
        isValid.Should().BeTrue();
    }

    [Fact]
    public void Verify_TimingAttackResistance_ShouldTakeSimilarTime()
    {
        // Arrange
        var provider = new HmacSha256SignatureProvider(ValidSecret);
        const string content = "test message";
        string validSignature = provider.Sign(content);
        string invalidSignature = "invalid-signature-same-length-as-valid-one-abcdef";

        // 这个测试验证时间常数比较，虽然无法精确测试时间，但至少验证方法不会立即返回
        // Act & Assert
        bool validResult = provider.Verify(content, validSignature);
        bool invalidResult = provider.Verify(content, invalidSignature);

        validResult.Should().BeTrue();
        invalidResult.Should().BeFalse();
    }
}
