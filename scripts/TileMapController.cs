using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;

public partial class TileMapController : TileMap
{
	private const int BOARD_SIZE = 20; // refer to game grid width, height

	public override void _Ready() {
		InitFloor();
		InitWall();
	}

	public override void _Process(double delta) {
		if (Input.IsKeyPressed(Key.F2)) {
			InitWall();
		}
	}

	private void InitFloor() {
		for (int row = 0; row < BOARD_SIZE; row++)
		{
			for (int col = 0; col < BOARD_SIZE; col++)
			{
				// Calculate the position of the tile
                Vector2I tilePosition = new Vector2I(row, col);

                // Set the tile at the calculated position
                SetCell(0, tilePosition, 2, GetRandomFloor());
			}
		}
	}

	private Vector2I GetRandomFloor() {
	    // Define the floors with their respective weights
	    KeyValuePair<Vector2I, float>[] floorsWithWeights = {
	        new KeyValuePair<Vector2I, float>(new Vector2I(4, 3), 8f),
	        new KeyValuePair<Vector2I, float>(new Vector2I(6, 3), 1f),
	        new KeyValuePair<Vector2I, float>(new Vector2I(7, 3), 1f),
	        new KeyValuePair<Vector2I, float>(new Vector2I(4, 4), 1f),
	        new KeyValuePair<Vector2I, float>(new Vector2I(5, 4), 1f),
	        new KeyValuePair<Vector2I, float>(new Vector2I(6, 4), 1f),
	        new KeyValuePair<Vector2I, float>(new Vector2I(7, 4), 1f)
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

	private void InitWall() {
		bool[,] dungeonGrid = GenerateDungeon();

		for (int row = 0; row < BOARD_SIZE; row++)
		{
			for (int col = 0; col < BOARD_SIZE; col++)
			{
				// Calculate the position of the tile
                Vector2I tilePosition = new Vector2I(row, col);

				if (!dungeonGrid[row, col])
                	// Set the tile at the calculated position
                	SetCell(1, tilePosition, 2, GetWall());
				else
					SetCell(1, tilePosition, 0);
			}
		}
	}

	private bool[,] GenerateDungeon() {
	    // Define dungeon parameters
	    int width = BOARD_SIZE;
	    int height = BOARD_SIZE;
	    int maxRooms = 7;
	    int minRoomSize = 4;
	    int maxRoomSize = 6;

	    // Initialize dungeon grid
	    bool[,] dungeonGrid = new bool[width, height];

		Random random = new Random();

	    // Generate rooms
	    List<Rect2I> rooms = new List<Rect2I>();
	    for (int i = 0; i < maxRooms; i++)
	    {
	        int roomWidth = random.Next(minRoomSize, maxRoomSize);
	        int roomHeight = random.Next(minRoomSize, maxRoomSize);
	        int x = random.Next(0, width - roomWidth - 1);
	        int y = random.Next(0, height - roomHeight - 1);

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

	private Vector2I GetWall() {
		return new Vector2I(0, 0);
	}
}
