// 引入 Entity Framework Core 套件
using Microsoft.EntityFrameworkCore;

// 建立 WebApplicationBuilder 物件，讀取組態與啟動參數
var builder = WebApplication.CreateBuilder(args);

// 註冊 MVC Controller 與 View，並設定 JSON 序列化命名規則（維持原屬性名稱）
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });
// 註冊自訂 Repository 服務（DI 注入）
builder.Services.AddScoped<IMaintainRecordRepository, MaintainRecordRepository>();
builder.Services.AddScoped<IDropdownDataRepository, DropdownDataRepository>();
// 註冊 DbContext，使用 SQL Server 連線字串
builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 建立 WebApplication 物件
var app = builder.Build();

// 非開發環境時啟用全域例外處理頁面
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
// 啟用靜態檔案服務（wwwroot）
app.UseStaticFiles();

// 啟用路由中介軟體
app.UseRouting();

// 啟用授權驗證（如有需要）
app.UseAuthorization();

// 設定預設路由規則（controller/action/id）
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// 啟動應用程式
app.Run();
