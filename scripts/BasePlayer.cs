using Godot;

public partial class BasePlayer : CharacterBody2D
{
	private TileMapController tileMap;
	private AnimatedSprite2D animatedSprite2D;
	private int direction = 0;
	private Godot.Collections.Array<Vector2I> currentPath;

	public override void _Ready()
	{
		tileMap = GetTree().Root.GetNode<TileMapController>("GameScene/TileMap");
		animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
	}

	public override void _Process(double delta)
	{
		HandlePlayerInput();
		HandlePlayerMovement();
		HandleAnimation();
	}

	private void HandlePlayerInput()
	{
		Vector2 clickPos = GetGlobalMousePosition();

		if (Input.IsActionJustPressed("move_to"))
		{
			// GD.Print("Click here " + clickPos);
			if (tileMap.IsWalkable(clickPos))
			{
				// GD.Print("Added destination");
				currentPath = tileMap.GetAStarGrid2D().GetIdPath(
					tileMap.LocalToMap(GlobalPosition), tileMap.LocalToMap(clickPos)
				).Slice(1);

				// Calculate direction based on the difference between current position and target position
				Vector2 directionVector = (tileMap.MapToLocal(currentPath[0]) - GlobalPosition).Normalized();
				direction = directionVector.X > 0 ? 1 : -1; // Right or Left
			}
		}
	}

	private void HandlePlayerMovement() {
		if (currentPath == null)
		{
			return;
		}

		Vector2 targetPos = tileMap.MapToLocal(currentPath[0]);
		GlobalPosition = GlobalPosition.MoveToward(targetPos, 1);

		if (GlobalPosition == targetPos && currentPath.Count > 1)
			currentPath.RemoveAt(0);
	}

	private void HandleAnimation() {
		animatedSprite2D.FlipH = direction < 0 ? true: false;

		if (currentPath != null && currentPath.Count > 1) {
			animatedSprite2D.Play("run");
		} else {
			animatedSprite2D.Play("idle");
		}
	}
}
