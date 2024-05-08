using Godot;

public partial class Knight : BasePlayer
{
    public Knight() : base("Knight", "A Powerful Hero")
    {
        MaxHealth = 20;
        MaxMana = 10;
        MaxStamina = 10;
    }

    public override void _Ready()
    {
        base._Ready();
    }
}