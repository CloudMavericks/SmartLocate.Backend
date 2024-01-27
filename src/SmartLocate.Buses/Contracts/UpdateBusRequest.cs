using System.ComponentModel.DataAnnotations;
using SmartLocate.Buses.Enums;

namespace SmartLocate.Buses.Contracts;

public class UpdateBusRequest
{
    public Guid Id { get; set; }
    
    [Required(ErrorMessage = "Vehicle number is required")]
    public string VehicleNumber { get; set; }
    
    [Required(ErrorMessage = "Vehicle model is required")]
    public string VehicleModel { get; set; }   
    
    [Required(ErrorMessage = "Vehicle status is required")]
    public VehicleStatus Status { get; set; }
}