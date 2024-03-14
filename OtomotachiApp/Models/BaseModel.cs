namespace OtomotachiApp.Models;

public partial class BaseModel
{
    public string _name = string.Empty;
    public DeviceType Type { get; set; }
    public enum DeviceType
    {
        Pen,
        Grass,
        Table
    }
}
