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
            //     cmd.CommandText = @"
            //     SELECT TOP 100 *
            //     FROM Case
            //     WHERE (@reportDate IS NULL OR ReportDate = @reportDate)
            //       AND (@finishDate IS NULL OR FinishDate = @finishDate)
            //       AND (@department IS NULL OR Department LIKE '%' + @department + '%')
            //       AND (@issueType IS NULL OR IssueType LIKE '%' + @issueType + '%')
            //       AND (@handler IS NULL OR Handler LIKE '%' + @handler + '%')
            //       AND (@handleType IS NULL OR HandleType LIKE '%' + @handleType + '%')
            // ";

            cmd.CommandText = @"
                SELECT TOP (10) A.[record_id]
                    ,A.[apply_date]
                    ,B.depart_name
                    ,C.staff_name
                    ,A.[problem_type]
                    ,A.[processing_type]
                    ,A.[processing_staff_id]
                    ,A.[completion_date]
                FROM [ISM].[dbo].[ism_maintain_record] A 
                LEFT JOIN [ISM].[dbo].ism_department B ON A.depart_code = B.depart_code
                LEFT JOIN [ISM].[dbo].ism_staff C ON A.staff_id = C.staff_id";

            // cmd.Parameters.AddWithValue("@reportDate", (object?)model.ReportDate ?? DBNull.Value);
            // cmd.Parameters.AddWithValue("@finishDate", (object?)model.FinishDate ?? DBNull.Value);
            // cmd.Parameters.AddWithValue("@department", (object?)model.Department ?? DBNull.Value);
            // cmd.Parameters.AddWithValue("@issueType", (object?)model.IssueType ?? DBNull.Value);
            // cmd.Parameters.AddWithValue("@handler", (object?)model.Handler ?? DBNull.Value);
            // cmd.Parameters.AddWithValue("@handleType", (object?)model.HandleType ?? DBNull.Value);

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    results.Add(new
                    {
                        // 根據你的資料表欄位調整
                        record_id = reader["record_id"],
                        apply_date = reader["apply_date"],
                        depart_name = reader["depart_name"],
                        staff_name = reader["staff_name"],
                        problem_type = reader["problem_type"],
                        processing_type = reader["processing_type"],
                        processing_staff_id = reader["processing_staff_id"],
                        completion_date = reader["completion_date"]

                        //序號 record_id
                        //申報日期 apply_date
                        //使用單位 depart_code
                        //使用者 staff_id
                        //問題類別 problem_type
                        //處理類別 processing_type
                        //處理人員 processing_staff_id
                        //完成日期 completion_date
                    });
                }
            }
        }
        return Json(results);
    }
}
