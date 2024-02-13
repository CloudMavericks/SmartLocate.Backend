namespace SmartLocate.Commons.Models;

public class Point
{
    public Point()
    {
        
    }

    public Point(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }
    
    public Point(double latitude, double longitude, string name)
    {
        Name = name;
        Latitude = latitude;
        Longitude = longitude;
    }
    
    public double Latitude { get; set; }
    
    public double Longitude { get; set; }
    
    public string Name { get; set; }
    
    public override string ToString()
    {
        return $"[{Latitude},{Longitude}]";
    }
}