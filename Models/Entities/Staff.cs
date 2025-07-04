using System.ComponentModel.DataAnnotations;

namespace myWebApp.Models;

public class Staff
{

    [Required]
    public int staff_id { get; set; } 

    [Required, StringLength(20)]
    public string staff_name { get; set; } = string.Empty;

}
