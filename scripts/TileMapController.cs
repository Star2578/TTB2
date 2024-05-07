using System;
using System.Collections.Generic;
using Godot;

public partial class TileMapController : TileMap
{
	private const int BOARD_SIZE = 20; // refer to game grid width, height
	private AStarGrid2D astar;
	private Rect2I mapRect;

	public override void _Ready()
	{
		InitFloor();
		InitWall(GenerateDungeon());
		InitAstar();
	}

	public override void _Process(double delta)
	{
		if (Input.IsKeyPressed(Key.F2))
		{
			ToNewLevel();
		}
	}

	private void InitFloor()
	{
		for (int row = 0; row < BOARD_SIZE; row++)
		{
			for (int col = 0; col < BOARD_SIZE; col++)
			{
				// Calculate the position of the tile
				Vector2I tilePosition = new Vector2I(row, col);

				// Set the tile at the calculated position
				SetCell(0, tilePosition, 1, GetRandomFloor());
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
								1,
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
					SetCell(1, new Vector2I(col - 1, row - 1), 0);
				}
			}
		}
	}

	private bool[,] GenerateDungeon()
	{
		// Define dungeon parameters
		int width = BOARD_SIZE;
		int height = BOARD_SIZE;
		int maxRooms = 7;
		int minRoomSize = 4;
		int maxRoomSize = 6;
		int borderSize = 2; // Size of the safe border

		// Initialize dungeon grid
		bool[,] dungeonGrid = new bool[width, height];

		Random random = new Random();

		// Generate rooms
		List<Rect2I> rooms = new List<Rect2I>();
		for (int i = 0; i < maxRooms; i++)
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

		CraftPath();
	}

	private void CraftPath()
	{
		for (int i = 0; i < BOARD_SIZE; i++)
		{
			for (int j = 0; j < BOARD_SIZE; j++)
			{
				Vector2I coord = new Vector2I(i, j);
				TileData tileData = GetCellTileData(1, coord);

				if (tileData != null && !tileData.GetCustomData("walkable").AsBool())
				{
					astar.SetPointSolid(coord);
				}
				else
				{
					astar.SetPointSolid(coord, false);
				}
			}
		}
	}

	public void ToNewLevel()
	{
		InitWall(GenerateDungeon());
		CraftPath();
	}

	public bool IsWalkable(Vector2 pos)
	{
		Vector2I mapPos = LocalToMap(pos);

		if (mapRect.HasPoint(mapPos) && !astar.IsPointSolid(mapPos))
		{
			// GD.Print("This is Walkable");
			return true;
		}

		// GD.Print("This is not Walkable");
		return false;
	}

	public AStarGrid2D GetAStarGrid2D()
	{
		return astar;
	}
}
