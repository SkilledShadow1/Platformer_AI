using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WaypointInitializer : MonoBehaviour
{
    public int gridNumber = 0;
    public int enemyHeight;
    [SerializeField] float xSpeed = 20;
    [SerializeField] GameObject waypointSprite;
    [SerializeField] Projection projection;
    [SerializeField] GameObject enemyHitbox;
    //TODO - Make the waypoint offset based on the tile size
    public static Vector3 waypointBaseOffset = new Vector3(0.5f, 1f, 0);
    [HideInInspector] public Vector3 waypointOffset;
    [HideInInspector] public GameObject grid;
    [HideInInspector] public Tilemap tilemap;
    [HideInInspector] public Dictionary<Vector3Int, GameObject> waypointDictionary = new Dictionary<Vector3Int, GameObject>();

    GridLayout gridLayout;
    bool[,] positionArray;
    Vector3Int origin;

    public void Init()
    {
        waypointBaseOffset = new Vector3(0.5f, 1f, 0);
        waypointDictionary.Clear();
        tilemap = gameObject.GetComponentInChildren<Tilemap>();
        grid = this.gameObject;
        Physics2D.IgnoreLayerCollision(6, 6, true);
        gridLayout = grid.GetComponent<GridLayout>();
        waypointOffset = new Vector3(waypointBaseOffset.x + transform.position.x, waypointBaseOffset.y + transform.position.y, waypointBaseOffset.z + transform.position.z);
        FindAllCells();
        FindOpenCells();
        GetWaypointTypes();
        FindGridConnections();
    }

    private void FindOpenCells()
    {

        for (int row = 0; row < positionArray.GetLength(1); row++)
        {
            for (int col = 0; col < positionArray.GetLength(0); col++)
            {

                if (positionArray[col, row])
                {
                    if (row + enemyHeight <= positionArray.GetLength(1) - 1)
                    {
                        for (int i = 0; i < enemyHeight; i++)
                        {
                            if (positionArray[col, row + 1 + i])
                            {
                                break;
                            }

                            if (i == enemyHeight - 1)
                            {
                                GameObject waypoint = Instantiate(waypointSprite, new Vector3(col + origin.x + waypointOffset.x, row + origin.y + waypointOffset.y, 0), Quaternion.identity);
                                //waypoint.AddComponent<CircleCollider2D>();
                                waypointDictionary.Add(new Vector3Int(col + origin.x, row + origin.y, 0), waypoint);
                                //Debug.Log(new Vector3Int(col + origin.x, row + origin.y, 0));
                            }

                        }

                    }

                }
            }
        }

    }

    private void FindAllCells()
    {
        positionArray = new bool[tilemap.cellBounds.size.x, tilemap.cellBounds.size.y];
        bool originChecker = false;
        foreach (Vector3Int position in tilemap.cellBounds.allPositionsWithin)
        {
            if (!originChecker)
            {
                origin = position;
                originChecker = true;
            }


            if (tilemap.GetTile(position))
            {
                positionArray[position.x - origin.x, position.y - origin.y] = true;
            }

        }
    }

    private void GetWaypointTypes()
    {
        foreach (KeyValuePair<Vector3Int, GameObject> waypoint in waypointDictionary)
        {
            Vector3Int left = new Vector3Int(waypoint.Key.x - 1, waypoint.Key.y, waypoint.Key.z);
            Vector3Int right = new Vector3Int(waypoint.Key.x + 1, waypoint.Key.y, waypoint.Key.z);

            if (waypointDictionary.ContainsKey(left) && waypointDictionary.ContainsKey(right))
            {
                waypoint.Value.tag = "Platform";
            }
            else if (waypointDictionary.ContainsKey(left))
            {
                waypoint.Value.tag = "RightEdge";
            }
            else if (waypointDictionary.ContainsKey(right))
            {
                waypoint.Value.tag = "LeftEdge";
            }
            else
            {
                waypoint.Value.tag = "Solo";
            }

            waypoint.Value.GetComponent<SpriteRenderer>().color = WaypointColor(waypoint.Value.tag);
        }


    }

    Color WaypointColor(string waypointType)
    {
        Color color = new Color(255, 255, 255);
        switch (waypointType)
        {
            case "Platform":
                color = new Color(0, 0, 0);
                break;
            case "LeftEdge":
                color = new Color(255, 0, 0);
                break;
            case "RightEdge":
                color = new Color(255, 0, 0);
                break;
            case "Solo":
                color = new Color(0, 100, 0);
                break;

        }

        return color;
    }

    private Dictionary<Vector3Int, (Vector2, float)> WalkWaypoints(Vector3Int cellPos, Vector3 waypointPos)
    {
        Dictionary<Vector3Int, (Vector2, float)> waypointTimeDictionary = new Dictionary<Vector3Int, (Vector2, float)>();
        List<Vector3Int> walkWaypoints = new List<Vector3Int>();
        if (waypointDictionary.ContainsKey(new Vector3Int(cellPos.x - 1, cellPos.y, cellPos.z)))
        {
            Debug.DrawLine(waypointPos, new Vector3(waypointPos.x - 1, waypointPos.y), new Color(255, 0, 0), 10f);
            walkWaypoints.Add(new Vector3Int(cellPos.x - 1, cellPos.y, cellPos.z));
        }
        if (waypointDictionary.ContainsKey(new Vector3Int(cellPos.x + 1, cellPos.y, cellPos.z)))
        {
            Debug.DrawLine(waypointPos, new Vector3(waypointPos.x + 1, waypointPos.y), new Color(255, 0, 0), 10f);
            walkWaypoints.Add(new Vector3Int(cellPos.x + 1, cellPos.y, cellPos.z));
        }

        foreach (Vector3Int waypoint in walkWaypoints)
        {
            Vector2 start = gridLayout.CellToWorld(cellPos);
            Vector2 end = gridLayout.CellToWorld(waypoint);

            float dist = Vector2.Distance(start, end);
            float time = dist / xSpeed;

            if (end.x >= start.x)
                waypointTimeDictionary.Add(waypoint, (new Vector2(xSpeed, 0), time));
            else
                waypointTimeDictionary.Add(waypoint, (new Vector2(-xSpeed, 0), time));
        }
        return waypointTimeDictionary;
    }


    private Vector3Int CheckLeftFall(Vector3Int cellPos, Vector3 waypointPos)
    {
        for (int y = enemyHeight; y >= 0; y--)
        {
            if (positionArray[cellPos.x - 1 - origin.x, cellPos.y - origin.y + y])
            {
                return Vector3Int.zero;
            }

        }

        //Debug.Log("a");
        for (int i = cellPos.y - 1; i >= origin.y; i--) //this gets the bottom of the tilemap
        {
            // Debug.Log(i);
            if (waypointDictionary.ContainsKey(new Vector3Int(cellPos.x - 1, i, cellPos.z)))
            {
                Debug.DrawLine(waypointPos, waypointDictionary[new Vector3Int(cellPos.x - 1, i, cellPos.z)].transform.position, new Color(0, 255, 0), 10f);
                return new Vector3Int(cellPos.x - 1, i, cellPos.z);
            }
        }

        return Vector3Int.zero;
    }



    private Vector3Int CheckRightFall(Vector3Int cellPos, Vector3 waypointPos)
    {
        for (int y = enemyHeight; y >= 0; y--)
        {
            if (positionArray[cellPos.x + 1 - origin.x, cellPos.y - origin.y + y])
            {
                return Vector3Int.zero;
            }

        }

        for (int i = cellPos.y - 1; i >= origin.y; i--) //this gets the bottom of the tilemap
        {
            if (waypointDictionary.ContainsKey(new Vector3Int(cellPos.x + 1, i, cellPos.z)))
            {
                Debug.DrawLine(waypointPos, waypointDictionary[new Vector3Int(cellPos.x + 1, i, cellPos.z)].transform.position, new Color(0, 255, 0), 10f);
                return new Vector3Int(cellPos.x + 1, i, cellPos.z);
            }
        }

        return Vector3Int.zero;
    }

    private Dictionary<Vector3Int, (Vector2, float)> FallWaypoints(Vector3Int cellPos, Vector3 waypointPos, GameObject waypointObject)
    {
        Dictionary<Vector3Int, (Vector2, float)> fallWaypoints = new Dictionary<Vector3Int, (Vector2, float)>();

        List<Vector3Int> possibleFallWaypoints = new List<Vector3Int>();
        switch (waypointObject.tag)
        {
            //Recode this to where it iterates through list to see if any equal Vector3.zero
            case ("LeftEdge"):
                possibleFallWaypoints.Add(CheckLeftFall(cellPos, waypointPos));
                break;
            case ("RightEdge"):
                possibleFallWaypoints.Add(CheckRightFall(cellPos, waypointPos));
                break;

            case ("Solo"):
                possibleFallWaypoints.Add(CheckLeftFall(cellPos, waypointPos));
                possibleFallWaypoints.Add(CheckRightFall(cellPos, waypointPos));
                break;
        }

        List<Vector3Int> verifiedWaypoints = new List<Vector3Int>();
        foreach (Vector3Int waypoint in possibleFallWaypoints)
            if (waypoint != Vector3Int.zero)
                verifiedWaypoints.Add(waypoint);

        //This is if the enemhy is too slow and hits the ground before it reaches its destination
        float fullDistance = gridLayout.cellSize.x;
        float fullXTime = fullDistance / xSpeed;

        float distBeforeFall = (gridLayout.cellSize.x / 2) + enemyHitbox.GetComponent<BoxCollider2D>().bounds.extents.x;
        float xTime = distBeforeFall / xSpeed;

        foreach (Vector3Int end in verifiedWaypoints)
        {
            //GET CELL SIZE
            float yDist = Mathf.Abs(tilemap.CellToWorld(cellPos).y - tilemap.CellToWorld(end).y);
            float yTime = Mathf.Abs(yDist / -Physics.gravity.y);


            if (end.x >= cellPos.x)
                fallWaypoints.Add(end, (new Vector2(xSpeed, 0), fullXTime + yTime));
            else
                fallWaypoints.Add(end, (new Vector2(-xSpeed, 0), fullXTime + yTime));


        }


        return fallWaypoints;
    }

    [SerializeField] int linePoints = 25;
    [SerializeField] int raycastPoints = 5;
    [SerializeField] float timeBetweenPoints = 0.2f;
    [SerializeField] float jumpStrength = 10f;
    [SerializeField] float timePer1Distance = 0.5f;
    [SerializeField] float rangeBetweenTimePerDistance = 0.1f;
    private Dictionary<Vector3Int, (Vector2, float)> GetJumpWaypoints(Vector3Int cellPos, Vector3 waypointPos, GameObject waypointObject)
    {
        Dictionary<Vector3Int, (Vector2, float)> possibleWaypointJumps = new Dictionary<Vector3Int, (Vector2, float)>();
        Vector2 velocityVector = new Vector2();

        foreach (KeyValuePair<Vector3Int, GameObject> waypoint in waypointDictionary)
        {

            if (waypointObject.CompareTag("Platform"))
            {
                if (waypoint.Key.y <= cellPos.y)
                    continue;
            }
            if (waypoint.Key == cellPos)
                continue;

            for (float t = timeIncrement; t <= maxTime; t += timeIncrement) 
            {
                if (CalculateJumpTrajectory(cellPos, waypoint.Key, t, ref velocityVector)) 
                {
                    possibleWaypointJumps.Add(waypoint.Key, (velocityVector, t));
                    break;
                }
            }

            
        }

        return possibleWaypointJumps;
    }

    [SerializeField] float timeIncrement;
    [SerializeField] float maxTime;


    [SerializeField] float maxVelocityX;
    [SerializeField] float maxVelocityY;

    bool CalculateJumpTrajectory(Vector3Int startPos, Vector3Int endPos, float time, ref Vector2 velocityVector)
    {

        Vector3 startWaypointPos = startPos + waypointOffset;
        Vector3 endWaypointPos = endPos + waypointOffset;
        float velocityX = (endPos.x - startPos.x) / time;
        float velocityY = ((endPos.y - startPos.y) / time) + (0.5f * -Physics.gravity.y * time);

        if (Mathf.Abs(velocityX) > maxVelocityX || velocityY > maxVelocityY || velocityY < 0)
            return false;

        velocityVector = new Vector2(velocityX, velocityY);
        float vertexTime = velocityY / -Physics.gravity.y;
        Vector2 vertexPos = new Vector2(startWaypointPos.x + (vertexTime * velocityX),
                                        startWaypointPos.y + (vertexTime * velocityY) +
                                        (Physics.gravity.y * 0.5f * vertexTime * vertexTime));

        
        if (!Physics2D.Linecast(new Vector2(startWaypointPos.x, startWaypointPos.y + 0.01f), vertexPos) && !Physics2D.Linecast(vertexPos, new Vector2(endWaypointPos.x, endWaypointPos.y + 0.01f))) 
        {
            if (projection.SimulateTrajectory(enemyHitbox, startPos, endPos, waypointOffset, new Vector2(velocityX, velocityY), time))
            {
                return true;
            }

        }
        return false;
       



    }

    private Dictionary<Vector3Int, (Vector2, float)> FindOpenWaypoints(Vector3Int cellPos)
    {
        Dictionary<Vector3Int, (Vector2, float)> movementOptions = new Dictionary<Vector3Int, (Vector2, float)>();
        GameObject waypointObject = waypointDictionary[cellPos];
        Vector3 waypointPos = waypointDictionary[cellPos].transform.position;
        //Draw a raycast between the 2 walkable points

        Dictionary<Vector3Int, (Vector2, float)> walkWaypoints = new Dictionary<Vector3Int, (Vector2, float)>();
        Dictionary<Vector3Int, (Vector2, float)> fallWaypoints = new Dictionary<Vector3Int, (Vector2, float)>();
        Dictionary<Vector3Int, (Vector2, float)> jumpWaypoints = new Dictionary<Vector3Int, (Vector2, float)>();

        walkWaypoints = WalkWaypoints(cellPos, waypointPos);
        fallWaypoints = FallWaypoints(cellPos, waypointPos, waypointObject);
        jumpWaypoints = GetJumpWaypoints(cellPos, waypointPos, waypointObject);


        foreach (Vector3Int walkWaypoint in walkWaypoints.Keys)
        {
            if (jumpWaypoints.ContainsKey(walkWaypoint))
            {
                jumpWaypoints.Remove(walkWaypoint);
            }
        }

        foreach (Vector3Int fallWaypoint in fallWaypoints.Keys)
        {
            if (jumpWaypoints.ContainsKey(fallWaypoint))
            {
                jumpWaypoints.Remove(fallWaypoint);
            }
        }
        foreach (KeyValuePair<Vector3Int, (Vector2, float)> movement in walkWaypoints)
        {
            movementOptions.Add(movement.Key, movement.Value);
            //Debug.Log(movement.Key + " " + movement.Value);
        }

        foreach (KeyValuePair<Vector3Int, (Vector2, float)> movement in fallWaypoints)
        {
            movementOptions.Add(movement.Key, movement.Value);
            //Debug.Log(movement.Key + " " + movement.Value);
        }

        foreach (KeyValuePair<Vector3Int, (Vector2, float)> movement in jumpWaypoints)
        {
            movementOptions.Add(movement.Key, movement.Value);
            //Debug.Log(movement.Key + " " +  movement.Value);
        }

        return movementOptions;
    }


    public Dictionary<Vector3Int, Dictionary<Vector3Int, (Vector2, float)>> waypointInfo;
    private void FindGridConnections()
    {
        waypointInfo = new Dictionary<Vector3Int, Dictionary<Vector3Int, (Vector2, float)>>();
        foreach (KeyValuePair<Vector3Int, GameObject> waypoint in waypointDictionary)
            waypointInfo.Add(waypoint.Key, FindOpenWaypoints(waypoint.Key));

        GameManager gm = GetComponentInParent<GameManager>();
        gm.levelWaypointInfo[int.Parse(gameObject.name.Substring(5, gameObject.name.Length - 5))] = waypointInfo;
        //Debug.Log(gameObject.name.Substring(5, gameObject.name.Length - 5));
    }
}
