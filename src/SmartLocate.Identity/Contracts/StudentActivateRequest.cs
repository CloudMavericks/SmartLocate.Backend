namespace SmartLocate.Identity.Contracts;

public class StudentActivateRequest
{
    public Guid StudentId { get; set; }
    public int ActivationCode { get; set; }
    
    public string Password { get; set; }
    
    public string ConfirmPassword { get; set; }
}