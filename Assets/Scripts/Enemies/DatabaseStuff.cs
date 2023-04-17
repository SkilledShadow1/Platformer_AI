using System.Collections;
using UnityEngine;
using Mono.Data.Sqlite;
using System.IO;
using System.Data;
using System.Collections.Generic;
using UnityEditor;

public class DatabaseStuff
{
    //number identifies which grid goes to what
    static int gridNumber;
    //static IDbConnection connection;
    public static SqliteConnection connection;
    static IDbCommand command;
    static IDataReader dataReader;
    static string dbName = "Test";
    static string dbPath = (Application.dataPath + "/" + dbName + ".db");
    //static Dictionary<Vector3Int, Dictionary<Vector3Int, (Vector2, float)>> waypointInfo = new Dictionary<Vector3Int, Dictionary<Vector3Int, (Vector2, float)>>();
    static int currentLevel;
    static int currentGrid;
    static GameManager gameManager;


    public static void Init(int levelNumber, GameManager gm, bool createWaypoints)
    {
        gameManager = gm;
        currentLevel = levelNumber;
        //foreach() FInd every instance of the script and then keep that info
        bool databaseExists = File.Exists(dbPath);
        if (!databaseExists)
        {
            SqliteConnection.CreateFile(dbPath);
        }

        connection = new SqliteConnection("URI=file:" + dbPath);

        connection.Open();

        if (createWaypoints) 
        {
            DropTables();
            Create();
            Put();
            GetDistinct();
        }
        else 
        {
            Get();
        }
    }

    public static void CloseConnection() 
    {
        connection.Close();
    }

    public static void Get()
    {
        
        command = connection.CreateCommand();
        command.CommandText = @$"
        SELECT * FROM waypoint_information_{currentLevel}
        ";
        dataReader = command.ExecuteReader();
        while (dataReader.Read()) 
        {
            int level = dataReader.GetInt32(0);
            int grid = dataReader.GetInt32(1);
            Vector3Int pos1 = new Vector3Int(dataReader.GetInt32(2), dataReader.GetInt32(3), dataReader.GetInt32(4));
            Vector3Int pos2 = new Vector3Int(dataReader.GetInt32(5), dataReader.GetInt32(6), dataReader.GetInt32(7));
            Vector2 velocity = new Vector2(dataReader.GetFloat(8), dataReader.GetFloat(9));
            float time = dataReader.GetFloat(10);

            if(!gameManager.levelWaypointInfo.ContainsKey(grid))
            {
                gameManager.levelWaypointInfo[grid] = new Dictionary<Vector3Int, Dictionary<Vector3Int, (Vector2, float)>>();
            }

            if (!gameManager.levelWaypointInfo[grid].ContainsKey(pos1)) 
            {
                gameManager.levelWaypointInfo[grid][pos1] = new Dictionary<Vector3Int, (Vector2, float)>();
            }
            
            gameManager.levelWaypointInfo[grid][pos1][pos2] = (velocity, time);
            
            Debug.Log(string.Format("level: {0}, grid: {1} pos1: {2}, pos2: {3}, velocity: {4}, time: {5}", level, grid, pos1, pos2, velocity, time));
            Debug.Log(gameManager.levelWaypointInfo[grid][pos1][pos2]);
        }

        foreach (int gNum in gameManager.levelWaypointInfo.Keys)
        {
            Debug.Log(gNum);
        }

        dataReader.Close();
        dataReader = null;
        command.Dispose();
        command = null;
    }

    public static void DropTables()
    {
        var command = connection.CreateCommand();
        command.CommandType = CommandType.Text;
        command.CommandText = $"DROP TABLE IF EXISTS waypoint_information_{currentLevel}";

        // Execute the SQL command
        command.ExecuteNonQuery();

        command = connection.CreateCommand();
        command.CommandType = CommandType.Text;
        command.CommandText = $"DROP TABLE IF EXISTS waypoint_info{currentLevel}";

        // Execute the SQL command
        command.ExecuteNonQuery();
    }

    public static void Create()
    {
        command = connection.CreateCommand();
        command.CommandText = $@"
            CREATE TABLE waypoint_info_{currentLevel} (
              level INT NOT NULL,
              grid INT NOT NULL,
              posx1 INT NOT NULL,
              posy1 INT NOT NULL,
              posz1 INT NOT NULL,
              posx2 INT NOT NULL,
              posy2 INT NOT NULL,
              posz2 INT NOT NULL,
              vx REAL NOT NULL,
              vy REAL NOT NULL,
              dist REAL NOT NULL
            );
            ";
        command.ExecuteNonQuery();

    }

    public static void Put()
    {
        foreach (var grid in gameManager.levelWaypointInfo)
        {
            int gridIndex = grid.Key;

            // Iterate through the second-level dictionary
            foreach (var waypoint in grid.Value)
            {
                Vector3Int waypointPosition = waypoint.Key;

                // Iterate through the third-level dictionary
                foreach (var info in waypoint.Value)
                {
                    Vector3Int targetPosition = info.Key;
                    Vector2 velocity = info.Value.Item1;
                    float time = info.Value.Item2;

                    // Do something with the data
                    Debug.LogFormat("Grid {0}, Waypoint {1}, Target {2}: Velocity {3}, Time {4}",
                        gridIndex, waypointPosition, targetPosition, velocity, time);
                    command = connection.CreateCommand();
                    command.CommandText = $@"
                    INSERT INTO waypoint_info_{currentLevel} (level, grid, posx1, posy1, posz1, posx2, posy2, posz2, vx, vy, dist) " +
                        $"VALUES ({currentLevel}, {gridIndex}, {waypointPosition.x}, {waypointPosition.y}, {waypointPosition.z}, {targetPosition.x}, {targetPosition.y}, {targetPosition.z}, {velocity.x}, {velocity.y}, {time})";
                    dataReader = command.ExecuteReader();
                }
            }
        }
    }

    public static void GetDistinct() 
    {
        command = connection.CreateCommand();
        command.CommandText =
        $@"
            CREATE TABLE waypoint_information_{currentLevel} AS SELECT DISTINCT * FROM waypoint_info_{currentLevel};
        ";
        command.ExecuteNonQuery();
        
        command = connection.CreateCommand();
        command.CommandText =
        $@"
           DROP TABLE waypoint_info_{currentLevel};
        ";
        command.ExecuteNonQuery();

    }

    static public void DeleteWaypoints(int level, int grid)
    {
        command = connection.CreateCommand();
        string query = $"DELETE FROM waypoint_information_{currentLevel} WHERE level = @level AND grid = @grid";
        using (SqliteCommand command = new SqliteCommand(query, connection))
        {
            command.Parameters.AddWithValue("@level", level);
            command.Parameters.AddWithValue("@grid", grid);
            int rowsAffected = command.ExecuteNonQuery();
            Debug.Log($"Deleted {rowsAffected} rows from waypoint_information_{currentLevel}");
        }
        DropTables();
        Create();
        Put();
        GetDistinct();

    }
}
