using Microsoft.EntityFrameworkCore;
using myWebApp.Models;

/// <summary>
/// EF Core 資料庫操作 DbContext，對應各資料表與關聯設定。
/// </summary>
public class MyDbContext : DbContext
{
    /// <summary>
    /// 建構式，注入 DbContextOptions
    /// </summary>
    /// <param name="options">DbContext 選項</param>
    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) { }

    /// <summary>
    /// 維護紀錄資料表
    /// </summary>
    public DbSet<MaintainRecord> MaintainRecords { get; set; }
    /// <summary>
    /// 單位資料表
    /// </summary>
    public DbSet<Department> Departments { get; set; }
    /// <summary>
    /// 人員資料表
    /// </summary>
    public DbSet<Staff> Staffs { get; set; }
    /// <summary>
    /// 代碼資料表
    /// </summary>
    public DbSet<Code> Codes { get; set; }

    /// <summary>
    /// 設定資料表主鍵、關聯、表名等 Fluent API 設定
    /// </summary>
    /// <param name="modelBuilder">模型建構器</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 設定主鍵、資料表名稱
        modelBuilder.Entity<MaintainRecord>()
            .ToTable("ism_maintain_record")
            .HasKey(x => x.record_id);

        modelBuilder.Entity<Department>()
            .ToTable("ism_department")
            .HasKey(x => x.depart_code);

        modelBuilder.Entity<Staff>()
            .ToTable("ism_staff")
            .HasKey(x => x.staff_id);

        modelBuilder.Entity<Code>()
            .ToTable("ism_code")
            .HasKey(x => new { x.kind, x.code });

        // 設定 MaintainRecord 與 Department 的關聯
        modelBuilder.Entity<MaintainRecord>()
            .HasOne(m => m.Department)
            .WithMany()
            .HasForeignKey(m => m.depart_code)
            .HasPrincipalKey(d => d.depart_code);

        // 設定 MaintainRecord 與 Staff 的關聯
        modelBuilder.Entity<MaintainRecord>()
            .HasOne(m => m.Staff)
            .WithMany()
            .HasForeignKey(m => m.staff_id)
            .HasPrincipalKey(s => s.staff_id);

        modelBuilder.Entity<MaintainRecord>()
            .HasOne(m => m.ProcessingStaff)
            .WithMany()
            .HasForeignKey(m => m.processing_staff_id)
            .HasPrincipalKey(s => s.staff_id);

        // 設定 MaintainRecord 與 Code 的關聯
        modelBuilder.Entity<MaintainRecord>()
            .HasOne(m => m.ProblemTypeCode)
            .WithMany()
            .HasForeignKey(m => m.problem_type)
            .HasPrincipalKey(c => c.code);

        modelBuilder.Entity<MaintainRecord>()
            .HasOne(m => m.ProcessingTypeCode)
            .WithMany()
            .HasForeignKey(m => m.processing_type)
            .HasPrincipalKey(c => c.code);

    }
}