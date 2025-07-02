using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using myWebApp.Models;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace myWebApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var connStr = GetConnectionString();

            using (var conn = new SqlConnection(connStr))
            {
                await conn.OpenAsync();

                ViewBag.Departments = await GetDepartments(conn);
                ViewBag.problemTypes = await GetProblemTypes(conn);
                ViewBag.processingStaffIds = await GetProcessingStaffIds(conn);
                ViewBag.processingTypes = await GetProcessingTypes(conn);

                return View();
            }
        }
        catch (Exception ex)
        {
            return Content("連線失敗：" + ex.Message);
        }
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

    // [HttpGet]
    // public async Task<IActionResult> GetDropdownOptions()
    // {
    //     try
    //     {
    //         var connStr = GetConnectionString();

    //         using (var conn = new SqlConnection(connStr))
    //         {
    //             await conn.OpenAsync();

    //             var departments = await GetDepartments(conn);
    //             var problemTypes = await GetProblemTypes(conn);
    //             var processingStaffIds = await GetProcessingStaffIds(conn);
    //             var processingTypes = await GetProcessingTypes(conn);

    //             var result = new
    //             {
    //                 departments,
    //                 problemTypes,
    //                 processingStaffIds,
    //                 processingTypes
    //             };

    //             return Json(result);
    //         }
    //     }
    //     catch (Exception ex)
    //     {
    //         return Content("連線失敗：" + ex.Message);
    //     }
    // }

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

            return Json(results);
        }
        catch (Exception ex)
        {
            return Content("連線失敗：" + ex.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var connStr = GetConnectionString();
            using (var conn = new SqlConnection(connStr))
            {
                await conn.OpenAsync();

                ViewBag.Departments = await GetDepartments(conn);
                ViewBag.problemTypes = await GetProblemTypes(conn);
                ViewBag.processingStaffIds = await GetProcessingStaffIds(conn);
                ViewBag.processingTypes = await GetProcessingTypes(conn);

                var cmd = conn.CreateCommand();
                cmd.CommandText = @"SELECT 
                                        *
                                    FROM 
                                        ism_maintain_record
                                    WHERE 
                                        record_id = @id";
                cmd.Parameters.AddWithValue("@id", id);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        var model = new MaintainRecordViewModel
                        {
                            record_id = reader["record_id"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["record_id"]),
                            apply_date = reader["apply_date"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["apply_date"]),
                            serial_no = reader["serial_no"] == DBNull.Value ? null : reader["serial_no"].ToString(),
                            depart_code = reader["depart_code"] == DBNull.Value ? null : reader["depart_code"].ToString(),
                            staff_id = reader["staff_id"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["staff_id"]),
                            tel = reader["tel"] == DBNull.Value ? null : reader["tel"].ToString(),
                            problem_type = reader["problem_type"] == DBNull.Value ? null : reader["problem_type"].ToString(),
                            record_staff_id = reader["record_staff_id"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["record_staff_id"]),
                            processing_staff_id = reader["processing_staff_id"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["processing_staff_id"]),
                            processing_type = reader["processing_type"] == DBNull.Value ? null : reader["processing_type"].ToString(),
                            description = reader["description"] == DBNull.Value ? null : reader["description"].ToString(),
                            solution = reader["solution"] == DBNull.Value ? null : reader["solution"].ToString(),
                            called_firm = reader["called_firm"] == DBNull.Value ? null : reader["called_firm"].ToString(),
                            completion_date = reader["completion_date"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["completion_date"]),
                            processing_minutes = reader["processing_minutes"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["processing_minutes"]),
                            update_user_id = reader["update_user_id"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["update_user_id"]),
                            update_date = reader["update_date"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["update_date"]),
                            satisfaction = reader["satisfaction"] == DBNull.Value ? null : reader["satisfaction"].ToString(),
                            recommendation = reader["recommendation"] == DBNull.Value ? null : reader["recommendation"].ToString(),
                            satisfaction_update_user_id = reader["satisfaction_update_user_id"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["satisfaction_update_user_id"]),
                            satisfaction_update_date = reader["satisfaction_update_date"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["satisfaction_update_date"])
                        };
                        return View(model);
                    }
                    else
                    {
                        return NotFound();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            return Content("查詢失敗：" + ex.Message);
        }
    }

    /// <summary>
    /// 取得部門下拉選單資料（depart_code, depart_name）
    /// </summary>
    /// <param name="conn">已開啟的 SQL 連線</param>
    /// <returns>部門 SelectListItem 清單</returns>
    private async Task<List<SelectListItem>> GetDepartments(SqlConnection conn)
    {
        var list = new List<SelectListItem>();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT depart_code, depart_name FROM ism_department ORDER BY depart_name";
        using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                list.Add(new SelectListItem
                {
                    Value = reader["depart_code"].ToString(),
                    Text = reader["depart_name"].ToString()
                });
            }
        }
        return list;
    }
    private async Task<List<SelectListItem>> GetProblemTypes(SqlConnection conn)
    {
        var list = new List<SelectListItem>();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT code, description FROM ism_code WHERE kind = 'QUESTION' ORDER BY description";
        using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                list.Add(new SelectListItem
                {
                    Value = reader["code"].ToString(),
                    Text = reader["description"].ToString()
                });
            }
        }
        return list;
    }
    private async Task<List<SelectListItem>> GetProcessingStaffIds(SqlConnection conn)
    {
        var list = new List<SelectListItem>();
        var cmd = conn.CreateCommand();
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
                list.Add(new SelectListItem
                {
                    Value = reader["staff_id"].ToString(),
                    Text = reader["staff_name"].ToString()
                });
            }
        }
        return list;
    }
    private async Task<List<SelectListItem>> GetProcessingTypes(SqlConnection conn)
    {
        var list = new List<SelectListItem>();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT code, description FROM ism_code WHERE kind = 'PROCESSING' ORDER BY description";
        using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                list.Add(new SelectListItem
                {
                    Value = reader["code"].ToString(),
                    Text = reader["description"].ToString()
                });
            }
        }
        return list;
    }
}
