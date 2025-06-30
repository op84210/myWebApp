using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using myWebApp.Models;
using Microsoft.Data.SqlClient;

namespace myWebApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public IActionResult DbTest()
    {
        var config = this.HttpContext.RequestServices.GetService<IConfiguration>();
        if (config == null)
        {
            return Content("連線失敗：無法取得設定服務 (IConfiguration)。");
        }
        string? connStr = config.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connStr))
        {
            return Content("連線失敗：找不到連線字串 'DefaultConnection'。");
        }
        string result;
        try
        {
            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();
                result = $"連線成功！伺服器版本：{conn.ServerVersion}";
            }
        }
        catch (Exception ex)
        {
            result = $"連線失敗：{ex.Message}";
        }
        return Content(result);
    }

    [HttpPost]
    public async Task<IActionResult> Search([FromForm] SearchViewModel model)
    {
        var config = HttpContext.RequestServices.GetService<IConfiguration>();
        if (config == null)
        {
            return Content("連線失敗：無法取得設定服務 (IConfiguration)。");
        }

        var connStr = config.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connStr))
        {
            return Content("連線失敗：找不到連線字串 'DefaultConnection'。");
        }

        var results = new List<object>();

        using (var conn = new SqlConnection(connStr))
        {
            await conn.OpenAsync();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
            SELECT TOP 100 *
            FROM Case
            WHERE (@reportDate IS NULL OR ReportDate = @reportDate)
              AND (@finishDate IS NULL OR FinishDate = @finishDate)
              AND (@department IS NULL OR Department LIKE '%' + @department + '%')
              AND (@issueType IS NULL OR IssueType LIKE '%' + @issueType + '%')
              AND (@handler IS NULL OR Handler LIKE '%' + @handler + '%')
              AND (@handleType IS NULL OR HandleType LIKE '%' + @handleType + '%')
        ";
            cmd.Parameters.AddWithValue("@reportDate", (object?)model.ReportDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@finishDate", (object?)model.FinishDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@department", (object?)model.Department ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@issueType", (object?)model.IssueType ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@handler", (object?)model.Handler ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@handleType", (object?)model.HandleType ?? DBNull.Value);

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    results.Add(new
                    {
                        // 根據你的資料表欄位調整
                        Id = reader["Id"],
                        ReportDate = reader["ReportDate"],
                        FinishDate = reader["FinishDate"],
                        Department = reader["Department"],
                        IssueType = reader["IssueType"],
                        Handler = reader["Handler"],
                        HandleType = reader["HandleType"]
                    });
                }
            }
        }
        return Json(results);
    }
}
