using Microsoft.EntityFrameworkCore;
using myWebApp.Models;

public class MyDbContext : DbContext
{
    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) { }
    public DbSet<MaintainRecordViewModel> MaintainRecords { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 設定主鍵、關聯、資料表名稱等
        modelBuilder.Entity<MaintainRecordViewModel>()
            .ToTable("ism_maintain_record")
            .HasKey(x => x.record_id);

    }
}