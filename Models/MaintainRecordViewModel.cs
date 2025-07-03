using System.ComponentModel.DataAnnotations;

namespace myWebApp.Models;

public class MaintainRecordViewModel
{

    public int? record_id { get; set; }// record_id	int

    [Required]
    public DateTime? apply_date { get; set; }// apply_date	datetime

    [StringLength(3)]
    public string? serial_no { get; set; }// serial_no	char(3)

    [Required, StringLength(10)]
    public string? depart_code { get; set; }// depart_code	varchar(10)

    [Required]
    public int? staff_id { get; set; }// staff_id	int

    [Required, StringLength(20)]
    public string? tel { get; set; }// tel	varchar(20)

    [Required, StringLength(2)]
    public string? problem_type { get; set; }// problem_type	varchar(2)

    [Required]
    public int? record_staff_id { get; set; }// record_staff_id	int

    [Required]
    public int? processing_staff_id { get; set; }// processing_staff_id	int

    [Required, StringLength(1)]
    public string? processing_type { get; set; }// processing_type	char(1)

    [Required]
    public string? description { get; set; }// description	ntext

    [Required]
    public string? solution { get; set; }// solution	ntext

    [StringLength(30)]
    public string? called_firm { get; set; }// called_firm	nvarchar(30)
    
    public DateTime? completion_date { get; set; }// completion_date	datetime
  
    public int? processing_minutes { get; set; }// processing_minutes	smallint

    [Required]
    public int? update_user_id { get; set; }// update_user_id	int

    [Required]
    public DateTime? update_date { get; set; }// update_date	datetime
    
    [StringLength(1)]
    public string? satisfaction { get; set; }// satisfaction	char(1)
    
    public string? recommendation { get; set; }// recommendation	ntext
    
    public int? satisfaction_update_user_id { get; set; }// satisfaction_update_user_id	int
    
    public DateTime? satisfaction_update_date { get; set; }// satisfaction_update_date	datetime


}
