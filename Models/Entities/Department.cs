using System.ComponentModel.DataAnnotations;

namespace myWebApp.Models;

public class Department
{

    [Required, StringLength(10)]
    public string depart_code { get; set; } = string.Empty;// depart_code	varchar(10)

    [Required, StringLength(20)]
    public string depart_name { get; set; } = string.Empty;// depart_name	nvarchar(20)

}
