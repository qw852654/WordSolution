using Microsoft.EntityFrameworkCore;
using 题库核心.标签模块.领域;
using 题库核心.题目模块.领域;
using 题库核心.试卷导入模块.领域;

namespace 题库基础设施.数据访问
{
    public class 题库DbContext : DbContext
    {
        public 题库DbContext(DbContextOptions<题库DbContext> options) : base(options)
        {
        }

        public DbSet<题目> 题目表 => Set<题目>();

        public DbSet<题型定义> 题型定义表 => Set<题型定义>();

        public DbSet<标签> 标签表 => Set<标签>();

        public DbSet<标签种类> 标签种类表 => Set<标签种类>();

        public DbSet<题目标签关系> 题目标签关系表 => Set<题目标签关系>();

        public DbSet<试卷记录> 试卷记录表 => Set<试卷记录>();

        public DbSet<试卷源文件记录> 试卷源文件记录表 => Set<试卷源文件记录>();

        public DbSet<试卷题目项> 试卷题目项表 => Set<试卷题目项>();

        public DbSet<知识点映射> 知识点映射表 => Set<知识点映射>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<题型定义>(builder =>
            {
                builder.ToTable("QuestionTypes");
                builder.HasKey(题型 => 题型.Id);
                builder.Property(题型 => 题型.名称).HasColumnName("Name");
                builder.Property(题型 => 题型.描述).HasColumnName("Description");
                builder.Property(题型 => 题型.排序值).HasColumnName("SortOrder");
            });

            modelBuilder.Entity<题目>(builder =>
            {
                builder.ToTable("Questions");
                builder.HasKey(题目 => 题目.Id);
                builder.Property(题目 => 题目.Description);
                builder.Property(题目 => 题目.题型ID).HasColumnName("TypeId");
                builder.Property(题目 => 题目.CreatedTime);
                builder.Property(题目 => 题目.UpdateTime);
                builder.HasOne<题型定义>()
                    .WithMany()
                    .HasForeignKey(题目 => 题目.题型ID)
                    .OnDelete(DeleteBehavior.Restrict);
                builder.Ignore(题目 => 题目.标签ID列表);
            });

            modelBuilder.Entity<标签>(builder =>
            {
                builder.ToTable("Tags");
                builder.HasKey(标签 => 标签.Id);
                builder.Property(标签 => 标签.标签种类ID).HasColumnName("GroupId");
                builder.Property(标签 => 标签.名称).HasColumnName("Name");
                builder.Property(标签 => 标签.Description).HasColumnName("Description");
                builder.Property(标签 => 标签.ParentId).HasColumnName("ParentId");
                builder.Property(标签 => 标签.同级排序值).HasColumnName("SiblingOrder");
                builder.Property(标签 => 标签.NumericValue).HasColumnName("NumericValue");
                builder.Property(标签 => 标签.IsEnabled).HasColumnName("IsEnabled");
                builder.Ignore(标签 => 标签.子标签列表);
            });

            modelBuilder.Entity<标签种类>(builder =>
            {
                builder.ToTable("TagKinds");
                builder.HasKey(标签种类 => 标签种类.Id);
                builder.Property(标签种类 => 标签种类.Id).ValueGeneratedNever();
                builder.Property(标签种类 => 标签种类.名称).HasColumnName("Name");
                builder.Property(标签种类 => 标签种类.是否树形).HasColumnName("IsTree");
                builder.Property(标签种类 => 标签种类.是否允许多选).HasColumnName("AllowMultiple");
                builder.Property(标签种类 => 标签种类.是否系统内置).HasColumnName("IsSystemBuiltIn");
                builder.Property(标签种类 => 标签种类.是否在正式工作流中可见).HasColumnName("VisibleInFormalWorkflow");
            });

            modelBuilder.Entity<题目标签关系>(builder =>
            {
                builder.ToTable("QuestionTags");
                builder.HasKey(关系 => new { 关系.题目ID, 关系.标签ID });
                builder.Property(关系 => 关系.题目ID).HasColumnName("QuestionId");
                builder.Property(关系 => 关系.标签ID).HasColumnName("TagId");
            });

            modelBuilder.Entity<试卷记录>(builder =>
            {
                builder.ToTable("Papers");
                builder.HasKey(试卷 => 试卷.Id);
                builder.Property(试卷 => 试卷.年份标签ID).HasColumnName("YearTagId");
                builder.Property(试卷 => 试卷.来源标签ID).HasColumnName("SourceTagId");
                builder.Property(试卷 => 试卷.显示名称).HasColumnName("DisplayName");
                builder.Property(试卷 => 试卷.总题数).HasColumnName("TotalCount");
                builder.Property(试卷 => 试卷.已确认数).HasColumnName("ConfirmedCount");
                builder.Property(试卷 => 试卷.已跳过数).HasColumnName("SkippedCount");
                builder.Property(试卷 => 试卷.状态).HasColumnName("Status");
                builder.HasIndex(试卷 => new { 试卷.年份标签ID, 试卷.来源标签ID }).IsUnique();
            });

            modelBuilder.Entity<试卷源文件记录>(builder =>
            {
                builder.ToTable("PaperSourceFiles");
                builder.HasKey(记录 => 记录.Id);
                builder.Property(记录 => 记录.试卷记录ID).HasColumnName("PaperId");
                builder.Property(记录 => 记录.原始文件名).HasColumnName("OriginalFileName");
                builder.Property(记录 => 记录.存储相对路径).HasColumnName("RelativePath");
                builder.Property(记录 => 记录.导入时间).HasColumnName("ImportedAt");
                builder.HasIndex(记录 => 记录.试卷记录ID);
            });

            modelBuilder.Entity<试卷题目项>(builder =>
            {
                builder.ToTable("PaperQuestions");
                builder.HasKey(题目项 => 题目项.Id);
                builder.Property(题目项 => 题目项.试卷记录ID).HasColumnName("PaperId");
                builder.Property(题目项 => 题目项.顺序号).HasColumnName("Sequence");
                builder.Property(题目项 => 题目项.题号文本).HasColumnName("QuestionNumberText");
                builder.Property(题目项 => 题目项.题目摘要).HasColumnName("QuestionSummary");
                builder.Property(题目项 => 题目项.完整Ooxml内容).HasColumnName("FullOoxml");
                builder.Property(题目项 => 题目项.题目正文Ooxml内容).HasColumnName("QuestionBodyOoxml");
                builder.Property(题目项 => 题目项.原始难度文本).HasColumnName("DifficultyRawText");
                builder.Property(题目项 => 题目项.原始知识点Json).HasColumnName("KnowledgeRawTextJson");
                builder.Property(题目项 => 题目项.推荐题型ID).HasColumnName("SuggestedTypeId");
                builder.Property(题目项 => 题目项.推荐题型名称).HasColumnName("SuggestedTypeName");
                builder.Property(题目项 => 题目项.识别说明).HasColumnName("RecognitionReason");
                builder.Property(题目项 => 题目项.置信度).HasColumnName("Confidence");
                builder.Property(题目项 => 题目项.状态).HasColumnName("Status");
                builder.Property(题目项 => 题目项.正式题目ID).HasColumnName("CreatedQuestionId");
                builder.HasIndex(题目项 => new { 题目项.试卷记录ID, 题目项.顺序号 }).IsUnique();
                builder.HasIndex(题目项 => new { 题目项.试卷记录ID, 题目项.状态 });
            });

            modelBuilder.Entity<知识点映射>(builder =>
            {
                builder.ToTable("KnowledgeMappings");
                builder.HasKey(映射 => 映射.Id);
                builder.Property(映射 => 映射.原始文本).HasColumnName("RawText");
                builder.Property(映射 => 映射.归一化原始文本).HasColumnName("NormalizedRawText");
                builder.Property(映射 => 映射.目标标签ID).HasColumnName("TargetTagId");
                builder.Property(映射 => 映射.是否抛弃).HasColumnName("IsDiscarded");
                builder.HasIndex(映射 => 映射.归一化原始文本).IsUnique();
            });
        }
    }
}
