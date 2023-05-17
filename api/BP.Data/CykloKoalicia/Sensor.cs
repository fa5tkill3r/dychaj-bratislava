using System;
using System.Collections.Generic;

namespace BP.Data.CykloKoalicia;

public partial class Sensor
{
    public uint Id { get; set; }

    public string Number { get; set; } = null!;

    public string Location { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
