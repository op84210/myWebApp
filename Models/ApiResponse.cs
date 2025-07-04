namespace myWebApp.Models;

public class ApiResponse
{
    public bool success { get; set; }
    public string? message { get; set; }
    public IEnumerable<string>? errors { get; set; }
    public object? data { get; set; }
    public static ApiResponse Ok(object? data = null, string? message = null)
        => new ApiResponse { success = true, data = data, message = message };

    public static ApiResponse Fail(string? message = null, IEnumerable<string>? errors = null)
        => new ApiResponse { success = false, message = message, errors = errors };
}
