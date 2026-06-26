namespace WordSolution.CmsV2.Domain.Enums;

public enum TeachingTopicStatus
{
    Active = 1,
    Archived = 2
}

public enum SectionType
{
    NormalCourse = 1,
    FirstRoundReview = 2,
    SpecialTopic = 3,
    ExamTraining = 4,
    Custom = 5
}

public enum Difficulty
{
    Unset = 0,
    Basic = 1,
    Medium = 2,
    Advanced = 3,
    Top = 4
}

public enum SectionStatus
{
    Draft = 1,
    Active = 2,
    Archived = 3
}

public enum SectionItemTargetType
{
    ContentBlock = 1,
    AtomicSection = 2
}

public enum ReferenceMode
{
    FollowLatest = 1,
    LockedVersion = 2
}

public enum SelectionLayer
{
    BasicRequired = 1,
    AdvancedSupplement = 2,
    TopExtension = 3,
    ClassroomBackup = 4,
    Homework = 5,
    FirstRoundReview = 6,
    SpecialTopic = 7,
    Custom = 8
}

public enum TeachingUse
{
    Lecture = 1,
    Exercise = 2,
    Homework = 3,
    Review = 4,
    ExamTraining = 5,
    Custom = 6
}

public enum AtomicSectionType
{
    ConceptBuild = 1,
    MethodExplain = 2,
    ExampleExplain = 3,
    MistakeAnalysis = 4,
    ExerciseArrange = 5,
    Custom = 6
}

public enum AtomicSectionStatus
{
    Draft = 1,
    Active = 2,
    Archived = 3
}

public enum AtomicSectionTeachingRole
{
    Unclassified = 0,
    Knowledge = 1,
    Example = 2,
    Variant = 3,
    Practice = 4,
    Homework = 5,
    PreClassQuiz = 6
}

public enum ContentBlockType
{
    KnowledgePoint = 1,
    Explanation = 2,
    Question = 3,
    Answer = 4,
    Analysis = 5,
    MethodSummary = 6,
    CommonMistake = 7,
    Analogy = 8,
    DiagramNote = 9,
    ExampleGroup = 10,
    ExerciseGroup = 11,
    VariantGroup = 12,
    GeneralText = 13
}

public enum QuestionType
{
    Unset = 0,
    Choice = 1,
    Blank = 2,
    Calculation = 3,
    Experiment = 4,
    Diagram = 5,
    Composite = 6
}

public enum ContentBlockStatus
{
    Draft = 1,
    Active = 2,
    Archived = 3
}

public enum ContentBlockPartType
{
    Stem = 1,
    Answer = 2,
    Analysis = 3,
    Hint = 4,
    Other = 5
}

public enum ContentBlockPartParseStatus
{
    NotApplicable = 0,
    Parsed = 1,
    ParsedWithWarnings = 2,
    Failed = 3
}

public enum SectionVariantType
{
    Lecture = 1,
    Exercise = 2,
    Homework = 3,
    Review = 4,
    ExamTraining = 5,
    Custom = 6
}

public enum SectionVariantStatus
{
    Draft = 1,
    Active = 2,
    Archived = 3
}

public enum HandoutStatus
{
    Draft = 1,
    Active = 2,
    Archived = 3
}

public enum HandoutVersionType
{
    Normal = 1,
    Review = 2,
    SpecialTopic = 3,
    ExamTraining = 4,
    Custom = 5
}

public enum HandoutVersionStatus
{
    Draft = 1,
    Active = 2,
    Archived = 3
}

public enum HandoutVersionItemTargetType
{
    SectionVariant = 1,
    ContentBlock = 2,
    Section = 3,
    AtomicSection = 4
}

public enum OutputTemplateStatus
{
    Active = 1,
    Archived = 2
}

public enum OutputAudience
{
    Student = 1,
    Teacher = 2,
    Mixed = 3
}

public enum OutputFormat
{
    Word = 1,
    Pdf = 2,
    WordAndPdf = 3
}

public enum VisibilityMode
{
    StudentNoAnswer = 1,
    TeacherWithAnswer = 2,
    Classroom = 3,
    Custom = 4
}

public enum OutputFormStatus
{
    Active = 1,
    Archived = 2
}

public enum TagStatus
{
    Active = 1,
    Archived = 2
}

public enum TagBindingTargetType
{
    ContentBlock = 1,
    AtomicSection = 2,
    Section = 3
}

public enum TeachingNoteType
{
    General = 1,
    ClassroomRecord = 2,
    LearningEffect = 3,
    TeachingReflection = 4,
    RevisionSuggestion = 5,
    QuestionReplacement = 6,
    CommonMistake = 7
}

public enum TeachingNoteEffectLevel
{
    Unknown = 0,
    Good = 1,
    Normal = 2,
    Weak = 3,
    Failed = 4
}

public enum TeachingNoteBindingTargetType
{
    ContentBlock = 1,
    Section = 2,
    AtomicSection = 3,
    AtomicSectionPanel = 4,
    AtomicSectionItem = 5,
    SectionItem = 6
}

