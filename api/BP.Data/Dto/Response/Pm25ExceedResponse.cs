﻿namespace BP.Data.Dto.Response;

public class Pm25ExceedResponse
{
    public SensorDto Sensor { get; set; } = null!;
    public int Exceed { get; set; }
}