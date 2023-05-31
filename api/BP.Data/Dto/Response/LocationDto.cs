﻿namespace BP.Data.Dto.Response;

public class LocationDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}