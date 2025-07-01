namespace myWebApp.Models;

public class MaintainRecordViewModel
{

    public int? record_id { get; set; }// record_id	int
    public DateTime? apply_date { get; set; }// apply_date	datetime
    public string? serial_no { get; set; }// serial_no	char(3)
    public string? depart_code { get; set; }// depart_code	varchar(10)
    public int? staff_id { get; set; }// staff_id	int
    public string? tel { get; set; }// tel	varchar(20)
    public string? problem_type { get; set; }// problem_type	varchar(2)
    public int? record_staff_id { get; set; }// record_staff_id	int
    public int? processing_staff_id { get; set; }// processing_staff_id	int
    public string? processing_type { get; set; }// processing_type	char(1)
    public string? description { get; set; }// description	ntext
    public string? solution { get; set; }// solution	ntext
    public string? called_firm { get; set; }// called_firm	nvarchar(30)
    public DateTime? completion_date { get; set; }// completion_date	datetime
    public int? processing_minutes { get; set; }// processing_minutes	smallint
    public int? update_user_id { get; set; }// update_user_id	int
    public DateTime? update_date { get; set; }// update_date	datetime
    public string? satisfaction { get; set; }// satisfaction	char(1)
    public string? recommendation { get; set; }// recommendation	ntext
    public int? satisfaction_update_user_id { get; set; }// satisfaction_update_user_id	int
    public DateTime? satisfaction_update_date { get; set; }// satisfaction_update_date	datetime


}
