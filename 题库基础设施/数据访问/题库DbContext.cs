using Microsoft.EntityFrameworkCore;
using 题库核心.内容块模块.领域;
using 题库核心.小节模块.领域;
using 题库核心.标签模块.领域;
using 题库核心.题目模块.领域;
using 题库核心.试卷导入模块.领域;

using 题库核心.讲义模块.领域;

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

        public DbSet<内容块> 内容块表 => Set<内容块>();

        public DbSet<内容块版本> 内容块版本表 => Set<内容块版本>();

        public DbSet<内容块子项> 内容块子项表 => Set<内容块子项>();

        public DbSet<内容块标签关系> 内容块标签关系表 => Set<内容块标签关系>();

        public DbSet<元数据选项> 元数据选项表 => Set<元数据选项>();

        public DbSet<小节> 小节表 => Set<小节>();

        public DbSet<小节项> 小节项表 => Set<小节项>();

        public DbSet<讲义> 讲义表 => Set<讲义>();

        public DbSet<讲义项> 讲义项表 => Set<讲义项>();

        public DbSet<讲义生成记录> 讲义生成记录表 => Set<讲义生成记录>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<讲义>(builder =>
            {
                builder.ToTable("Handouts");
                builder.HasKey(讲义 => 讲义.Id);
                builder.Property(讲义 => 讲义.标题).HasColumnName("Title");
                builder.Property(讲义 => 讲义.摘要).HasColumnName("Summary");
                builder.Property(讲义 => 讲义.状态).HasColumnName("Status");
                builder.Property(讲义 => 讲义.创建时间).HasColumnName("CreatedTime");
                builder.Property(讲义 => 讲义.更新时间).HasColumnName("UpdateTime");
                builder.HasIndex(讲义 => 讲义.状态);
                builder.HasIndex(讲义 => 讲义.更新时间);
            });

            modelBuilder.Entity<讲义项>(builder =>
            {
                builder.ToTable("HandoutItems");
                builder.HasKey(讲义项 => 讲义项.Id);
                builder.Property(讲义项 => 讲义项.讲义ID).HasColumnName("HandoutId");
                builder.Property(讲义项 => 讲义项.目标类型).HasColumnName("TargetType");
                builder.Property(讲义项 => 讲义项.目标ID).HasColumnName("TargetId");
                builder.Property(讲义项 => 讲义项.引用版本模式).HasColumnName("ReferenceMode");
                builder.Property(讲义项 => 讲义项.锁定内容块版本ID).HasColumnName("LockedContentBlockVersionId");
                builder.Property(讲义项 => 讲义项.角色).HasColumnName("Role");
                builder.Property(讲义项 => 讲义项.排序).HasColumnName("SortOrder");
                builder.Property(讲义项 => 讲义项.创建时间).HasColumnName("CreatedTime");
                builder.HasIndex(讲义项 => new { 讲义项.讲义ID, 讲义项.排序 });
                builder.HasIndex(讲义项 => new { 讲义项.目标类型, 讲义项.目标ID });
                builder.HasIndex(讲义项 => new { 讲义项.讲义ID, 讲义项.目标类型, 讲义项.目标ID });
            });

            modelBuilder.Entity<讲义生成记录>(builder =>
            {
                builder.ToTable("HandoutGenerations");
                builder.HasKey(记录 => 记录.Id);
                builder.Property(记录 => 记录.讲义ID).HasColumnName("HandoutId");
                builder.Property(记录 => 记录.文件路径).HasColumnName("FilePath");
                builder.Property(记录 => 记录.版本清单Json).HasColumnName("VersionManifestJson");
                builder.Property(记录 => 记录.生成时间).HasColumnName("GeneratedTime");
                builder.HasIndex(记录 => new { 记录.讲义ID, 记录.生成时间 });
            });

            modelBuilder.Entity<小节>(builder =>
            {
                builder.ToTable("Sections");
                builder.HasKey(小节 => 小节.Id);
                builder.Property(小节 => 小节.标题).HasColumnName("Title");
                builder.Property(小节 => 小节.摘要).HasColumnName("Summary");
                builder.Property(小节 => 小节.章节标签ID).HasColumnName("ChapterTagId");
                builder.Property(小节 => 小节.状态).HasColumnName("Status");
                builder.Property(小节 => 小节.创建时间).HasColumnName("CreatedTime");
                builder.Property(小节 => 小节.更新时间).HasColumnName("UpdateTime");
                builder.HasIndex(小节 => new { 小节.状态, 小节.章节标签ID });
                builder.HasIndex(小节 => 小节.更新时间);
            });

            modelBuilder.Entity<小节项>(builder =>
            {
                builder.ToTable("SectionItems");
                builder.HasKey(小节项 => 小节项.Id);
                builder.Property(小节项 => 小节项.小节ID).HasColumnName("SectionId");
                builder.Property(小节项 => 小节项.内容块ID).HasColumnName("ContentBlockId");
                builder.Property(小节项 => 小节项.内容块版本ID).HasColumnName("ContentBlockVersionId");
                builder.Property(小节项 => 小节项.引用版本模式).HasColumnName("ReferenceMode");
                builder.Property(小节项 => 小节项.角色).HasColumnName("Role");
                builder.Property(小节项 => 小节项.排序).HasColumnName("SortOrder");
                builder.Property(小节项 => 小节项.创建时间).HasColumnName("CreatedTime");
                builder.HasIndex(小节项 => new { 小节项.小节ID, 小节项.排序 });
                builder.HasIndex(小节项 => 小节项.内容块ID);
                builder.HasIndex(小节项 => new { 小节项.小节ID, 小节项.内容块ID });
            });

            modelBuilder.Entity<内容块>(builder =>
            {
                builder.ToTable("ContentBlocks");
                builder.HasKey(内容块 => 内容块.Id);
                builder.Property(内容块 => 内容块.标题).HasColumnName("Title");
                builder.Property(内容块 => 内容块.摘要).HasColumnName("Summary");
                builder.Property(内容块 => 内容块.类型).HasColumnName("Type");
                builder.Property(内容块 => 内容块.状态).HasColumnName("Status");
                builder.Property(内容块 => 内容块.当前版本ID).HasColumnName("CurrentVersionId");
                builder.Property(内容块 => 内容块.结构类型).HasColumnName("StructureType");
                builder.Property(内容块 => 内容块.是否允许子块).HasColumnName("AllowChildren");
                builder.Property(内容块 => 内容块.RoleOptionId).HasColumnName("RoleOptionId");
                builder.Property(内容块 => 内容块.DifficultyOptionId).HasColumnName("DifficultyOptionId");
                builder.Property(内容块 => 内容块.UsageOptionId).HasColumnName("UsageOptionId");
                builder.Property(内容块 => 内容块.QuestionTypeOptionId).HasColumnName("QuestionTypeOptionId");
                builder.Property(内容块 => 内容块.DefaultIncluded).HasColumnName("DefaultIncluded");
                builder.Property(内容块 => 内容块.Note).HasColumnName("Note");
                builder.Property(内容块 => 内容块.创建时间).HasColumnName("CreatedTime");
                builder.Property(内容块 => 内容块.更新时间).HasColumnName("UpdateTime");
                builder.HasIndex(内容块 => new { 内容块.类型, 内容块.状态 });
                builder.HasIndex(内容块 => 内容块.当前版本ID);
                builder.HasIndex(内容块 => new { 内容块.结构类型, 内容块.是否允许子块 });
            });

            modelBuilder.Entity<元数据选项>(builder =>
            {
                builder.ToTable("MetadataOptions");
                builder.HasKey(选项 => 选项.Id);
                builder.Property(选项 => 选项.Category).HasColumnName("Category");
                builder.Property(选项 => 选项.Name).HasColumnName("Name");
                builder.Property(选项 => 选项.SortOrder).HasColumnName("SortOrder");
                builder.Property(选项 => 选项.IsActive).HasColumnName("IsActive");
                builder.Property(选项 => 选项.CreatedTime).HasColumnName("CreatedTime");
                builder.Property(选项 => 选项.UpdatedTime).HasColumnName("UpdatedTime");
                builder.HasIndex(选项 => new { 选项.Category, 选项.SortOrder });
                builder.HasIndex(选项 => new { 选项.Category, 选项.Name }).IsUnique();
            });

            modelBuilder.Entity<内容块版本>(builder =>
            {
                builder.ToTable("ContentBlockVersions");
                builder.HasKey(版本 => 版本.Id);
                builder.Property(版本 => 版本.内容块ID).HasColumnName("ContentBlockId");
                builder.Property(版本 => 版本.版本号).HasColumnName("VersionNumber");
                builder.Property(版本 => 版本.Docx路径).HasColumnName("DocxPath");
                builder.Property(版本 => 版本.Html预览路径).HasColumnName("HtmlPreviewPath");
                builder.Property(版本 => 版本.纯文本内容).HasColumnName("PlainText");
                builder.Property(版本 => 版本.创建时间).HasColumnName("CreatedTime");
                builder.Property(版本 => 版本.是否当前版本).HasColumnName("IsCurrentVersion");
                builder.HasIndex(版本 => new { 版本.内容块ID, 版本.版本号 }).IsUnique();
                builder.HasIndex(版本 => new { 版本.内容块ID, 版本.是否当前版本 });
            });

            modelBuilder.Entity<内容块子项>(builder =>
            {
                builder.ToTable("ContentBlockChildren");
                builder.HasKey(子项 => 子项.Id);
                builder.Property(子项 => 子项.父内容块ID).HasColumnName("ParentBlockId");
                builder.Property(子项 => 子项.子内容块ID).HasColumnName("ChildBlockId");
                builder.Property(子项 => 子项.子内容块版本ID).HasColumnName("ChildVersionId");
                builder.Property(子项 => 子项.引用版本模式).HasColumnName("ReferenceMode");
                builder.Property(子项 => 子项.角色).HasColumnName("Role");
                builder.Property(子项 => 子项.排序).HasColumnName("SortOrder");
                builder.Property(子项 => 子项.创建时间).HasColumnName("CreatedTime");
                builder.HasIndex(子项 => new { 子项.父内容块ID, 子项.排序 });
                builder.HasIndex(子项 => 子项.子内容块ID);
                builder.HasIndex(子项 => new { 子项.父内容块ID, 子项.子内容块ID });
            });

            modelBuilder.Entity<内容块标签关系>(builder =>
            {
                builder.ToTable("ContentBlockTags");
                builder.HasKey(关系 => new { 关系.内容块ID, 关系.标签ID });
                builder.Property(关系 => 关系.内容块ID).HasColumnName("ContentBlockId");
                builder.Property(关系 => 关系.标签ID).HasColumnName("TagId");
                builder.HasIndex(关系 => 关系.内容块ID);
                builder.HasIndex(关系 => 关系.标签ID);
            });

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
