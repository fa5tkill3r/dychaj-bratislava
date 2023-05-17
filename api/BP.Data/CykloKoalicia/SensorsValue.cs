using System;
using System.Collections.Generic;

namespace BP.Data.CykloKoalicia;

public partial class SensorsValue
{
    public uint Id { get; set; }

    public uint SensorId { get; set; }

    public decimal Pm10 { get; set; }

    public decimal Pm25 { get; set; }

    public decimal Temperature { get; set; }

    public decimal Humidity { get; set; }

    public uint Pressure { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
