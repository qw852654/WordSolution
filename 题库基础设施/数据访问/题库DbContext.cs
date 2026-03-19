using Microsoft.EntityFrameworkCore;
using 题库核心.标签模块.领域;
using 题库核心.题目模块.领域;

namespace 题库基础设施.数据访问
{
    public class 题库DbContext : DbContext
    {
        public 题库DbContext(DbContextOptions<题库DbContext> options) : base(options)
        {
        }

        public DbSet<题目> 题目表 => Set<题目>();

        public DbSet<标签> 标签表 => Set<标签>();

        public DbSet<标签种类> 标签种类表 => Set<标签种类>();

        public DbSet<题目标签关系> 题目标签关系表 => Set<题目标签关系>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<题目>(builder =>
            {
                builder.ToTable("Questions");
                builder.HasKey(题目 => 题目.Id);
                builder.Property(题目 => 题目.Description);
                builder.Property(题目 => 题目.CreatedTime);
                builder.Property(题目 => 题目.UpdateTime);
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
        }
    }
}
