using WordSolution.CmsV2.Domain.Enums;

namespace WordSolution.CmsV2.Domain.Documents;

public sealed class QuestionOutputStyleOptions
{
    public static QuestionOutputStyleOptions Default { get; } = new(
        ExampleStemStyleName: "例题",
        VariantStemStyleName: "变式",
        PracticeStemStyleName: "练习题");

    public QuestionOutputStyleOptions(
        string ExampleStemStyleName,
        string VariantStemStyleName,
        string PracticeStemStyleName)
    {
        this.ExampleStemStyleName = NormalizeRequired(ExampleStemStyleName, nameof(ExampleStemStyleName));
        this.VariantStemStyleName = NormalizeRequired(VariantStemStyleName, nameof(VariantStemStyleName));
        this.PracticeStemStyleName = NormalizeRequired(PracticeStemStyleName, nameof(PracticeStemStyleName));
    }

    public string ExampleStemStyleName { get; }

    public string VariantStemStyleName { get; }

    public string PracticeStemStyleName { get; }

    public string? ResolveForTeachingRole(AtomicSectionTeachingRole teachingRole)
    {
        return teachingRole switch
        {
            AtomicSectionTeachingRole.Example => ExampleStemStyleName,
            AtomicSectionTeachingRole.Variant => VariantStemStyleName,
            AtomicSectionTeachingRole.PreClassQuiz => null,
            AtomicSectionTeachingRole.Practice
                or AtomicSectionTeachingRole.Homework
                or AtomicSectionTeachingRole.Unclassified => PracticeStemStyleName,
            _ => null
        };
    }

    public string? ResolveForContentBlockType(ContentBlockType blockType)
    {
        return blockType switch
        {
            ContentBlockType.Question => PracticeStemStyleName,
            ContentBlockType.ExampleGroup => ExampleStemStyleName,
            ContentBlockType.VariantGroup => VariantStemStyleName,
            ContentBlockType.ExerciseGroup => PracticeStemStyleName,
            _ => null
        };
    }

    private static string NormalizeRequired(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Style name cannot be empty.", parameterName);
        }

        return value.Trim();
    }
}
