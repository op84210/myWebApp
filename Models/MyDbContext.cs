using Microsoft.EntityFrameworkCore;
using myWebApp.Models;

public class MyDbContext : DbContext
{
    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) { }
    public DbSet<MaintainRecord> MaintainRecords { get; set; }
    public DbSet<Department> Departments { get; set; }
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