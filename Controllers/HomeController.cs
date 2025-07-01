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

    private string? GetConnectionString()
    {
        var config = HttpContext.RequestServices.GetService<IConfiguration>();
        if (config == null)
            throw new Exception("無法取得設定服務 (IConfiguration)。");
        var connStr = config.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connStr))
            throw new Exception("找不到連線字串 'DefaultConnection'。");
        return connStr;
    }

    [HttpGet]
    public async Task<IActionResult> GetDropdownOptions()
    {
        try
        {
            var connStr = GetConnectionString();

            var result = new
            {
                departments = new List<object>(),
                problemTypes = new List<object>(),
                processingStaffIds = new List<object>(),
                processingTypes = new List<object>()
            };

            using (var conn = new SqlConnection(connStr))
            {
                await conn.OpenAsync();

                // 取得部門
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT depart_code, depart_name FROM ism_department ORDER BY depart_name";
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        result.departments.Add(new
                        {
                            code = reader["depart_code"].ToString(),
                            name = reader["depart_name"].ToString()
                        });
                    }
                }

                // 取得問題類別
                cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT code, description FROM ism_code WHERE kind = 'QUESTION' ORDER BY description";
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        result.problemTypes.Add(new
                        {
                            code = reader["code"].ToString(),
                            name = reader["description"].ToString()
                        });
                    }
                }

                // 取得處理人員
                cmd = conn.CreateCommand();
                cmd.CommandText = @"SELECT 
                                    staff_id, staff_name 
                                FROM 
                                    ism_staff
                                WHERE EXISTS (
                                    SELECT 1 FROM ism_maintain_record
                                    WHERE processing_staff_id = ism_staff.staff_id
                                )
                                ORDER BY 
                                    staff_name";
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        result.processingStaffIds.Add(new
                        {
                            code = reader["staff_id"].ToString(),
                            name = reader["staff_name"].ToString()
                        });
                    }
                }

                // 取得處理類別
                cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT code, description FROM ism_code WHERE kind = 'PROCESSING' ORDER BY description";
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        result.processingTypes.Add(new
                        {
                            code = reader["code"].ToString(),
                            name = reader["description"].ToString()
                        });
                    }
                }
            }

            return Json(result);

        }
        catch (Exception ex)
        {
            return Content("連線失敗：" + ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Search([FromForm] SearchConditionViewModel model)
    {
        try
        {
            var results = new List<object>();

            var connStr = GetConnectionString();
            using (var conn = new SqlConnection(connStr))
            {
                await conn.OpenAsync();
                var cmd = conn.CreateCommand();

                cmd.CommandText = @"
                SELECT 
                    A.record_id as recordId,
                    A.apply_date as applyDate,
                    B.depart_name as departName,
                    C.staff_name as userName,
                    D.description as problemType,
                    E.description as processingType,
                    F.staff_name as processingStaff,
                    A.completion_date as completionDate
                FROM 
                    ism_maintain_record A LEFT JOIN 
                    ism_department B ON A.depart_code = B.depart_code LEFT JOIN 
                    ism_staff C ON A.staff_id = C.staff_id LEFT JOIN 
                    ism_code D ON A.problem_type = D.code AND D.kind = 'QUESTION' LEFT JOIN 
                    ism_code E ON A.processing_type = E.code AND E.kind = 'PROCESSING' LEFT JOIN 
                    ism_staff F ON A.processing_staff_id = F.staff_id
                WHERE 
                    (@applyStartDate IS NULL OR A.apply_date >= @applyStartDate) AND 
                    (@applyEndDate IS NULL OR A.apply_date <= @applyEndDate) AND 
                    (@completionStartDate IS NULL OR A.completion_date >= @completionStartDate) AND 
                    (@completionEndDate IS NULL OR A.completion_date <= @completionEndDate) AND 
                    (@departCode IS NULL OR A.depart_code = @departCode) AND 
                    (@problemType IS NULL OR A.problem_type = @problemType) AND 
                    (@processingStaffId IS NULL OR A.processing_staff_id = @processingStaffId) AND 
                    (@processingType IS NULL OR A.processing_type = @processingType)";

                cmd.Parameters.AddWithValue("@applyStartDate", (object?)model.ApplyStartDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@applyEndDate", (object?)model.ApplyEndDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@completionStartDate", (object?)model.CompletionStartDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@completionEndDate", (object?)model.CompletionEndDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@departCode", (object?)model.DepartCode ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@problemType", (object?)model.ProblemType ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@processingStaffId", (object?)model.ProcessingStaffId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@processingType", (object?)model.ProcessingType ?? DBNull.Value);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        results.Add(new SearchResultViewModel
                        {
                            // 根據你的資料表欄位調整
                            RecordId = (int)reader["recordId"],//序號
                            ApplyDate = (DateTime)reader["applyDate"],//申報日期
                            DepartName = (String)reader["departName"],//使用單位
                            UserName = (String)reader["userName"],//使用者
                            ProblemType = (String)reader["problemType"],//問題類別
                            ProcessingType = (String)reader["processingType"],//處理類別
                            ProcessingStaff = (String)reader["processingStaff"],//處理人員
                            CompletionDate = (DateTime)reader["completionDate"]//完成日期
                        });
                    }
                }
            }
            // ...原本邏輯...
            return Json(results);
        }
        catch (Exception ex)
        {
            return Content("連線失敗：" + ex.Message);
        }
    }
}
