using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class TileMapController : TileMap
{
	private const int BOARD_SIZE = 20; // refer to game grid width, height
	public AStarGrid2D astar { get; private set; }
	public Rect2I mapRect { get; private set; }
	private CellData[][] cellData; // for quick direct access using col, row notation
	private Dictionary<Vector2I, CellData> cellDataDictionary; // for containment checks

	private int floorLayer = 0;
	private int selectionLayer = 1;
	private int hoverLayer = 2;
	private int wallLayer = 3;

	public override void _Process(double delta)
	{
		if (Input.IsKeyPressed(Key.F2))
		{
			BuildNewLevel();
		}
		MouseHoverTiles();
	}

	private void MouseHoverTiles()
	{
		var tileMapPos = LocalToMap(GetGlobalMousePosition());
		var tileLocalPos = (Vector2I)MapToLocal(tileMapPos);

		ClearLayer(hoverLayer);

		if (cellDataDictionary.ContainsKey(tileLocalPos) && cellData[tileMapPos.X][tileMapPos.Y].isValid)
		{
			// GD.Print("Hovering");
			SetCell(hoverLayer, tileMapPos, 2, new Vector2I(0, 0));
		}
	}

	public void InitCellData()
	{
		cellData = new CellData[BOARD_SIZE][];
		cellDataDictionary = new Dictionary<Vector2I, CellData>();

		for (int i = 0; i < BOARD_SIZE; i++)
		{
			cellData[i] = new CellData[BOARD_SIZE];

			for (int j = 0; j < BOARD_SIZE; j++)
			{
				Vector2 localPos = MapToLocal(new Vector2I(i, j));
				cellData[i][j] = new CellData((Vector2I)localPos, true);
				cellDataDictionary.Add((Vector2I)localPos, cellData[i][j]);
			}
		}
	}

	public void InitFloor()
	{
		for (int row = 0; row < BOARD_SIZE; row++)
		{
			for (int col = 0; col < BOARD_SIZE; col++)
			{
				// Calculate the position of the tile
				Vector2I tilePosition = new Vector2I(row, col);

				// Set the tile at the calculated position
				SetCell(floorLayer, tilePosition, 1, GetRandomFloor());
			}
		}
	}

	private Vector2I GetRandomFloor()
	{
		// Define the floors with their respective weights
		KeyValuePair<Vector2I, float>[] floorsWithWeights =
		{
			new KeyValuePair<Vector2I, float>(new Vector2I(0, 0), 8f),
			new KeyValuePair<Vector2I, float>(new Vector2I(2, 0), 1f),
			new KeyValuePair<Vector2I, float>(new Vector2I(3, 0), 1f),
			new KeyValuePair<Vector2I, float>(new Vector2I(0, 1), 1f),
			new KeyValuePair<Vector2I, float>(new Vector2I(1, 1), 1f),
			new KeyValuePair<Vector2I, float>(new Vector2I(2, 1), 1f),
			new KeyValuePair<Vector2I, float>(new Vector2I(3, 1), 1f),
		};

		// Calculate the total weight
		float totalWeight = 0f;
		foreach (var pair in floorsWithWeights)
		{
			totalWeight += pair.Value;
		}

		// Generate a random number within the total weight range
		Random random = new Random();
		float randomValue = (float)random.NextDouble() * totalWeight;

		// Select the floor based on the random value
		foreach (var pair in floorsWithWeights)
		{
			randomValue -= pair.Value;
			if (randomValue <= 0)
			{
				return pair.Key;
			}
		}

		return floorsWithWeights[0].Key;
	}

	private void InitWall(bool[,] dungeonLayout)
	{
		// Create an expanded layout to prevent null border
		bool[,] expandedLayout = new bool[BOARD_SIZE + 2, BOARD_SIZE + 2];
		for (int row = 0; row < expandedLayout.GetLength(1); row++)
		{
			// String toPrint = "";
			for (int col = 0; col < expandedLayout.GetLength(0); col++)
			{
				if (
					col == 0
					|| col == expandedLayout.GetLength(0) - 1
					|| row == 0
					|| row == expandedLayout.GetLength(1) - 1
				)
				{
					expandedLayout[col, row] = false;
				}
				else
				{
					expandedLayout[col, row] = dungeonLayout[col - 1, row - 1];
				}
				// toPrint += " " + (expandedLayout[col, row] ? "." : "#");
			}
			// GD.Print(toPrint);
		}

		// Initialize autotiling system
		for (int row = 1; row < expandedLayout.GetLength(1) - 1; row++)
		{
			for (int col = 1; col < expandedLayout.GetLength(0) - 1; col++)
			{
				if (!expandedLayout[col, row])
				{
					// Calculate bit mask
					int bitMask = 0;
					if (!expandedLayout[col, row - 1])
						bitMask += 2;
					if (!expandedLayout[col - 1, row])
						bitMask += 8;
					if (!expandedLayout[col + 1, row])
						bitMask += 16;
					if (!expandedLayout[col, row + 1])
						bitMask += 64;
					if (
						!expandedLayout[col - 1, row]
						&& !expandedLayout[col, row - 1]
						&& !expandedLayout[col - 1, row - 1]
					)
						bitMask += 1;
					if (
						!expandedLayout[col + 1, row]
						&& !expandedLayout[col, row - 1]
						&& !expandedLayout[col + 1, row - 1]
					)
						bitMask += 4;
					if (
						!expandedLayout[col - 1, row]
						&& !expandedLayout[col, row + 1]
						&& !expandedLayout[col - 1, row + 1]
					)
						bitMask += 32;
					if (
						!expandedLayout[col + 1, row]
						&& !expandedLayout[col, row + 1]
						&& !expandedLayout[col + 1, row + 1]
					)
						bitMask += 128;

					// Map bit mask to tile and add to tile map
					int[] jumpList =
					{
						2,
						8,
						10,
						11,
						16,
						18,
						22,
						24,
						26,
						27,
						30,
						31,
						64,
						66,
						72,
						74,
						75,
						80,
						82,
						86,
						88,
						90,
						91,
						94,
						95,
						104,
						106,
						107,
						120,
						122,
						123,
						126,
						127,
						208,
						210,
						214,
						216,
						218,
						219,
						222,
						223,
						248,
						250,
						251,
						254,
						255,
						0
					};
					for (int i = 0; i < jumpList.Length; i++)
					{
						if (bitMask == jumpList[i])
						{
							// Add tile to tile map
							SetCell(
								wallLayer,
								new Vector2I(col - 1, row - 1),
								0,
								GetWallTile((i + 1) % 8, (i + 1) / 8)
							);
							break;
						}
					}
				}
				else
				{
					// place empty
					SetCell(wallLayer, new Vector2I(col - 1, row - 1), 0);
				}
			}
		}

		InitAstar();
	}

	private bool[,] GenerateDungeon()
	{
		// Define dungeon parameters
		int width = BOARD_SIZE;
		int height = BOARD_SIZE;
		int minRooms = 3;
		int maxRooms = 6;
		int minRoomSize = 4;
		int maxRoomSize = 6;
		int borderSize = 2; // Size of the safe border

		// Initialize dungeon grid
		bool[,] dungeonGrid = new bool[width, height];

		Random random = new Random();
		int targetRooms = random.Next(minRooms, maxRooms);

		// Generate rooms
		List<Rect2I> rooms = new List<Rect2I>();
		for (int i = 0; i < 1000; i++)
		{
			int roomWidth = random.Next(minRoomSize, maxRoomSize);
			int roomHeight = random.Next(minRoomSize, maxRoomSize);
			int x = random.Next(borderSize, width - roomWidth - 1);
			int y = random.Next(borderSize, height - roomHeight - 1);

			Rect2I newRoom = new Rect2I(x, y, roomWidth, roomHeight);

			bool overlaps = false;
			foreach (Rect2I room in rooms)
			{
				if (newRoom.Intersects(room))
				{
					overlaps = true;
					break;
				}
			}

			if (!overlaps)
			{
				rooms.Add(newRoom);
				// Carve out the room in the dungeon grid
				for (int rX = x; rX < x + roomWidth; rX++)
				{
					for (int rY = y; rY < y + roomHeight; rY++)
					{
						dungeonGrid[rX, rY] = true;
					}
				}
			}

			if (rooms.Count == targetRooms) break;
		}

		// Generate corridors
		for (int i = 1; i < rooms.Count; i++)
		{
			Rect2I prevRoom = rooms[i - 1];
			Rect2I currentRoom = rooms[i];

			// Connect the centers of the two rooms with corridors
			Vector2I prevRoomCenter = prevRoom.GetCenter();
			Vector2I currentRoomCenter = currentRoom.GetCenter();

			while (prevRoomCenter != currentRoomCenter)
			{
				if (prevRoomCenter.X != currentRoomCenter.X)
				{
					int corridorX = prevRoomCenter.X;
					int minY = Mathf.Min(prevRoomCenter.Y, currentRoomCenter.Y);
					int maxY = Mathf.Max(prevRoomCenter.Y, currentRoomCenter.Y);

					for (int y = minY; y <= maxY; y++)
					{
						dungeonGrid[corridorX, y] = true;
					}
				}
				else if (prevRoomCenter.Y != currentRoomCenter.Y)
				{
					int corridorY = prevRoomCenter.Y;
					int minX = Mathf.Min(prevRoomCenter.X, currentRoomCenter.X);
					int maxX = Mathf.Max(prevRoomCenter.X, currentRoomCenter.X);

					for (int x = minX; x <= maxX; x++)
					{
						dungeonGrid[x, corridorY] = true;
					}
				}

				// Move towards the target room's center
				if (prevRoomCenter.X < currentRoomCenter.X)
					prevRoomCenter.X++;
				else if (prevRoomCenter.X > currentRoomCenter.X)
					prevRoomCenter.X--;
				if (prevRoomCenter.Y < currentRoomCenter.Y)
					prevRoomCenter.Y++;
				else if (prevRoomCenter.Y > currentRoomCenter.Y)
					prevRoomCenter.Y--;
			}
		}

		return dungeonGrid;
	}

	private Vector2I GetWallTile(int col, int row)
	{
		return new Vector2I(col, row);
	}

	private void InitAstar()
	{
		if (astar == null)
		{
			Vector2I tileMapSize = GetUsedRect().End - GetUsedRect().Position;
			mapRect = new Rect2I(Vector2I.Zero, tileMapSize);
			astar = new AStarGrid2D
			{
				Region = mapRect,
				CellSize = TileSet.TileSize,
				DefaultComputeHeuristic = AStarGrid2D.Heuristic.Manhattan,
				DefaultEstimateHeuristic = AStarGrid2D.Heuristic.Manhattan,
				DiagonalMode = AStarGrid2D.DiagonalModeEnum.Always
			};
			astar.Update();
		}

		CraftPath();
	}

	private void CraftPath()
	{
		for (int i = 0; i < BOARD_SIZE; i++)
		{
			for (int j = 0; j < BOARD_SIZE; j++)
			{
				Vector2I coord = new Vector2I(i, j);
				TileData tileData = GetCellTileData(3, coord);

				if (tileData != null && !tileData.GetCustomData("walkable").AsBool())
				{
					astar.SetPointSolid(coord);
					cellData[i][j].isValid = false;
					cellDataDictionary[cellData[i][j].position].isValid = false;
				}
				else
				{
					astar.SetPointSolid(coord, false);
					cellData[i][j].isValid = true;
					cellDataDictionary[cellData[i][j].position].isValid = true;
				}
			}
		}
	}

	public void BuildNewLevel()
	{
		var player = GameController.GetInstance().player;
		InitWall(GenerateDungeon());
		CraftPath();
		PlaceEntityRandomly(player);
		player.ClearPlayerPath();
	}

	public bool IsWalkable(Vector2 pos)
	{
		Vector2I mapPos = LocalToMap(pos);

		if (mapRect.HasPoint(mapPos) && !astar.IsPointSolid(mapPos))
		{
			return true;
		}

		return false;
	}

	public void PlaceEntity(Node2D entity, int col, int row)
	{
		// Check if the grid position is valid on the scene
		if (!IsGridPositionValid(new Vector2I(col, row)))
		{
			GD.Print("Invalid grid position.");
			return;
		}

		// Adjust entity position
		entity.Position = cellData[col][row].position;

		// Add the entity as a child of the tilemap controller
		// if hasn't added yet
		if (!GetTree().Root.GetNode<Node>("GameScene/PlayerContainer").GetChildren().Contains(entity))
			GetTree().Root.GetNode<Node>("GameScene/PlayerContainer").AddChild(entity);
	}

	public void PlaceEntityRandomly(BasePlayer entity)
	{
		// Generate a random position within the tile map bounds
		Vector2I randomPos = GetRandomGridPosition();

		// Place the entity at the random position
		PlaceEntity(entity, randomPos.X, randomPos.Y);
	}

	private bool IsGridPositionValid(Vector2I gridPos)
	{
		return cellData[gridPos.X][gridPos.Y].isValid;
	}

	private Vector2I GetRandomGridPosition()
	{
		Random random = new Random();
		int randomX;
		int randomY;
		do
		{
			randomX = random.Next(0, BOARD_SIZE);
			randomY = random.Next(0, BOARD_SIZE);
		} while (!cellData[randomX][randomY].isValid);

		return new Vector2I(randomX, randomY);
	}
}
