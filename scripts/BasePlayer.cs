using Godot;

public partial class BasePlayer : CharacterBody2D
{
	private TileMapController tileMap;
	private Godot.Collections.Array<Vector2I> currentPath;

	public override void _Ready()
	{
		tileMap = GetTree().Root.GetNode<TileMapController>("GameScene/TileMap");
	}

	public override void _Process(double delta)
	{
		HandlePlayerInput();
		HandlePlayerMovement();
	}

	private void HandlePlayerInput()
	{
		Vector2 clickPos = GetGlobalMousePosition();

		if (Input.IsActionJustPressed("move_to"))
		{
			GD.Print("Click here " + clickPos);
			if (tileMap.IsWalkable(clickPos))
			{
				GD.Print("Added destination");
				currentPath = tileMap.GetAStarGrid2D().GetIdPath(
					tileMap.LocalToMap(GlobalPosition), tileMap.LocalToMap(clickPos)
				).Slice(1);
			}
		}
	}

	private void HandlePlayerMovement() {
		if (currentPath == null)
		{
			return;
		}

		Vector2 targetPos = tileMap.MapToLocal(currentPath[0]);
		GlobalPosition = GlobalPosition.MoveToward(targetPos, 5);

		if (GlobalPosition == targetPos && currentPath.Count > 1)
			currentPath.RemoveAt(0);
	}
}
