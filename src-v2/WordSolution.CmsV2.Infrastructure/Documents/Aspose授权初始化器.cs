using Aspose.Words;

namespace WordSolution.CmsV2.Infrastructure.Documents;

public class Aspose授权初始化器
{
    private readonly string _授权文件路径;

    public Aspose授权初始化器(string 授权文件路径)
    {
        if (string.IsNullOrWhiteSpace(授权文件路径))
        {
            throw new ArgumentException("授权文件路径不能为空。", nameof(授权文件路径));
        }

        _授权文件路径 = 授权文件路径;
    }

    public void 初始化授权()
    {
        var 授权 = new License();
        try
        {
            授权.SetLicense(_授权文件路径);
        }
        catch (Exception ex) when (
            ex is InvalidOperationException ||
            ex is FileNotFoundException ||
            ex is DirectoryNotFoundException ||
            ex is IOException ||
            ex is UnauthorizedAccessException)
        {
            Console.Error.WriteLine(
                $"Aspose.Words license was not applied. Evaluation mode will be used. Reason: {ex.Message}");
        }
    }
}
