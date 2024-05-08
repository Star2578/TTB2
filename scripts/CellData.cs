using Godot;

public class CellData
{
    public Vector2I position;
    public bool isValid;

    public CellData(Vector2I pos, bool valid)
    {
        position = pos;
        isValid = valid;
    }
}
