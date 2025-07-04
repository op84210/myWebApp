using System.ComponentModel.DataAnnotations;

namespace myWebApp.Models;

public class Code
{

    [Required, StringLength(20)]
    public string kind { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string code { get; set; } = string.Empty;
    
    [Required, StringLength(20)]
    public string description { get; set; } = string.Empty;

}
