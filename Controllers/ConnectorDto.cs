// Controllers/ChargePointsController.cs
public class ConnectorDto
{
    public int Id { get; set; }
    public int ConnectorId { get; set; }
    public string Type { get; set; }
    public string Status { get; set; }
    public double MaxPower { get; set; }
    public int ChargePointId { get; internal set; }
    public string ChargePointName { get; internal set; }
}