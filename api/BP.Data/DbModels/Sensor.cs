// ReSharper disable InconsistentNaming
// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable ClassNeverInstantiated.Global

#pragma warning disable CS8618

using ValueType = BP.Data.DbHelpers.ValueType;

namespace BP.Data.DbModels;

public class Sensor
{
    public int Id { get; set; }
    public string UniqueId { get; set; }
    public int ModuleId { get; set; }
    public Module Module { get; set; }
    public string? Name { get; set; }
    public bool Default { get; set; } = false;
    public string? Description { get; set; }
    public ValueType Type { get; set; }
    public List<Reading> Readings { get; set; }
}