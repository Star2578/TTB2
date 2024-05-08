using Godot;

public partial class BasePlayer : BaseEntity
{
	private TileMapController tileMap;
	private AnimatedSprite2D animatedSprite2D;
	private Godot.Collections.Array<Vector2I> currentPath;
	private int direction = 0;


	public bool IsPlayerSelected { get; set; }

	public BasePlayer()
	{
		currentPath = new Godot.Collections.Array<Vector2I>();
	}

	public BasePlayer(string name, string description) : base(name, description)
	{
		
	}

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
			if (!tileMap.IsWalkable(clickPos)) return;

			currentPath = tileMap.astar.GetIdPath(
				tileMap.LocalToMap(GlobalPosition), tileMap.LocalToMap(clickPos)
			).Slice(1);
		}
	}

	private void HandlePlayerMovement()
	{
		if (currentPath == null || currentPath.Count == 0) return;


		Vector2 targetPos = tileMap.MapToLocal(currentPath[0]);
		GlobalPosition = GlobalPosition.MoveToward(targetPos, 1);


		if (GlobalPosition == targetPos && currentPath.Count > 1)
			currentPath.RemoveAt(0);
		else
		{
			// Calculate direction based on the difference between current position and target position
			Vector2 directionVector = (targetPos - GlobalPosition).Normalized();
			if (directionVector.X != 0)
				direction = directionVector.X > 0 ? 1 : -1; // Right or Left
		}
	}

	private void HandleAnimation()
	{
		animatedSprite2D.FlipH = direction < 0 ? true : false;

		if (currentPath != null && currentPath.Count > 1)
		{
			animatedSprite2D.Play("run");
		}
		else
		{
			animatedSprite2D.Play("idle");
		}
	}

	public void ClearPlayerPath()
	{
		if (currentPath != null && currentPath.Count > 0)
			currentPath.Clear();
	}
}
