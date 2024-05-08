using Godot;
using System;

public partial class MenuController : Node2D
{
    private void OnStartButtonPressed()
    {
        GetTree().ChangeSceneToFile("res://scenes/game_scene.tscn");
    }

    private void OnQuitButtonPressed()
    {
        GetTree().Quit();
    }
}
