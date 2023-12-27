namespace SmartLocate.Commons.Models;

public record Point(double Latitude, double Longitude)
{
    public override string ToString()
    {
        return $"[{Latitude},{Longitude}]";
    }
}