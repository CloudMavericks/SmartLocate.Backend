using SmartLocate.Commons.Attributes;
using SmartLocate.Infrastructure.Commons.Contracts;

namespace SmartLocate.Identity.Entities;

[Collection("BusDriverActivationCodes")]
public class BusDriverActivationCode : IEntity
{
    public Guid Id { get; set; }
    public Guid BusDriverId { get; set; }
    public int ActivationCode { get; set; }
    public DateTime CreatedAt { get; set; }
}