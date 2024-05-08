using Godot;
using System;

public partial class GameController : Node
{
	private static GameController instance;

	public TileMapController tileMapController { get; private set; }
	public BasePlayer player;
	public PackedScene playerScene;

	public GameController()
	{
		playerScene = (PackedScene)GD.Load("res://scenes/knight.tscn");
		player = playerScene.Instantiate().GetNode<BasePlayer>(".");
	}

	public override void _Ready()
	{
		tileMapController = GetTree().Root.GetNode<TileMapController>("GameScene/TileMap");

		GameStart();
	}

	public static GameController GetInstance()
	{
		if (instance == null)
		{
			instance = new GameController();
		}
		return instance;
	}

	public void GameStart()
	{
		tileMapController.InitCellData();
		tileMapController.InitFloor();
		tileMapController.BuildNewLevel();
	}
}
