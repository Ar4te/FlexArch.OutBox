namespace FlexArch.OutBox.Abstractions;

public interface ISignatureProvider
{
    public string Sign(string content);
    public bool Verify(string content, string signature);
}
