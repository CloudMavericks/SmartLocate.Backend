using SmartLocate.Commons.Attributes;
using SmartLocate.Infrastructure.Commons.Contracts;

namespace SmartLocate.Identity.Entities;

[Collection("StudentActivationCodes")]
public class StudentActivationCode : IEntity
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public int ActivationCode { get; set; }
    public DateTime CreatedAt { get; set; }
}