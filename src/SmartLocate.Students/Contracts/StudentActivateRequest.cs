namespace SmartLocate.Students.Contracts;

public class StudentActivateRequest
{
    public Guid StudentId { get; set; }
    public string Password { get; set; }
}