namespace WordSolution.CmsV2.Domain.Documents;

public interface IOutputTemplatePathResolver
{
    string ResolveTemplateDocxPath(string templateDocxPath);
}
