namespace OtomotachiApp.Models;

internal class AppData
{
    public static List<Pen> Pens = new List<Pen>
    {
        new Pen(){Type=BaseModel.DeviceType.Pen, _name="Pen君"},
        new Pen(){Type=BaseModel.DeviceType.Pen, _name="PenPen"},
        new Pen(){Type=BaseModel.DeviceType.Pen, _name="PenChan"},
        new Pen(){Type=BaseModel.DeviceType.Pen, _name="PenKun"}
    };
}
