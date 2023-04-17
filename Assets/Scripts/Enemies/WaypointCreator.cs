using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
public class WaypointCreator : MonoBehaviour
{
    //Put the Next waypoints on the enemy
    //Make it so that when a waypoint is clicked on, it finds all the paths but chooses the best one
    [SerializeField] GameObject actualEnemy;
    [SerializeField] Projection projection;
    [SerializeField] bool showRaycasts = true;
    [SerializeField] GameObject waypointSprite;
    [SerializeField] GameObject grid;
    [SerializeField] GameObject enemyHitbox;
    [SerializeField] int enemyHeight;
    [SerializeField] GridLayout gridLayout;
    [SerializeField] EnemyGroundCollision enemyGroundCollision;
    Tilemap tilemap;
    LineRenderer lineRenderer;
    List<Vector3Int> cellPositions = new List<Vector3Int>();
    bool[,] positionArray;
    Vector3Int origin;
    Vector3Int clickedWaypoint;
    List<Vertex> movementQueue;

    Collider2D actualEnemyHitbox;
    Rigidbody2D enemyRB;
    TilemapCollider2D tilemapCollider;
    void Start()
    {
        Physics2D.queriesStartInColliders = false;
        clicked = false;
        gridLayout = grid.GetComponent<GridLayout>();
        tilemap = grid.GetComponentInChildren<Tilemap>();
        tilemapCollider = grid.GetComponentInChildren<TilemapCollider2D>();
        lineRenderer = grid.GetComponent<LineRenderer>();

        FindAllCells();
        FindOpenCells();
        GetWaypointTypes();

        selectedCell = new Vector3Int(0, 0, 0);
        FindGridConnections();
        enemyRB = actualEnemy.GetComponent<Rigidbody2D>();
        actualEnemyHitbox = actualEnemy.GetComponent<Collider2D>();
        currentlyMoving = false;
        nextWaypointPos = Vector3.positiveInfinity;
        movementQueue = new List<Vertex>(0);
        Debug.Log(origin);

        enemyState = EnemyState.isStopping;
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

    public Vector3 waypointOffset = new Vector3(0.5f, 1f, 0);

    Dictionary<Vector3Int, GameObject> waypointDictionary = new Dictionary<Vector3Int, GameObject>();
    private void FindOpenCells() 
    {

        for (int row = 0; row < positionArray.GetLength(1); row++) 
        {
            for(int col = 0; col < positionArray.GetLength(0); col++) 
            {
                
                if(positionArray[col, row]) 
                {
                    if (row + enemyHeight <= positionArray.GetLength(1) - 1)
                    {
                        for(int i = 0; i < enemyHeight; i++) 
                        {
                            if (positionArray[col, row + 1 + i])
                            {
                                break ;
                            }

                            if(i == enemyHeight - 1) 
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
    Vector3Int selectedCell;
    private Vector3Int GetClickedWaypoint() 
    {
        Vector3 worldCoord = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        selectedCell = tilemap.WorldToCell(worldCoord);
        if (tilemap.HasTile(selectedCell)) 
        {
            //tilemap.SetColor(selectedCell, new Color(0, 0, 0));
            return selectedCell;
        }

        return new Vector3Int(0,0,0);
    }

    private void GetWaypointTypes() 
    {
        foreach(KeyValuePair<Vector3Int, GameObject> waypoint in waypointDictionary) 
        {
            if (showRaycasts) 
            {
                //Debug.Log(waypoint.Key);
                //Debug.Log(waypoint.Value.transform);
            }
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

    [SerializeField] float xSpeed = 20;

    private Dictionary<Vector3Int, (Vector2,float)> WalkWaypoints(Vector3Int cellPos, Vector3 waypointPos) 
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

        foreach(Vector3Int waypoint in walkWaypoints) 
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

    private Dictionary<Vector3Int, (Vector2, float)> FallWaypoints(Vector3Int cellPos, Vector3 waypointPos, GameObject waypointObject) 
    {
        Dictionary<Vector3Int, (Vector2, float) > fallWaypoints = new Dictionary<Vector3Int, (Vector2, float)>();

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
        float time = new float();

        foreach (KeyValuePair<Vector3Int, GameObject> waypoint in waypointDictionary)
        {
            if (waypointObject.CompareTag("Platform")) 
            {
                if (waypoint.Key.y <= cellPos.y)
                    continue;
            }
               

            float dist = Vector3.Distance(cellPos, waypoint.Key);

            if (dist <= jumpStrength && dist != 0)
            {
                if(CalculateJumpParabola(cellPos, waypoint.Key, dist, timePer1Distance, ref velocityVector, ref time)) 
                {
                    possibleWaypointJumps.Add(waypoint.Key, (velocityVector, time));
                }
                else if(CalculateJumpParabola(cellPos, waypoint.Key, dist, timePer1Distance + rangeBetweenTimePerDistance, ref velocityVector, ref time))
                {
                    possibleWaypointJumps.Add(waypoint.Key, (velocityVector, time));
                }
                else if (CalculateJumpParabola(cellPos, waypoint.Key, dist, timePer1Distance - rangeBetweenTimePerDistance, ref velocityVector, ref time)) 
                {
                    possibleWaypointJumps.Add(waypoint.Key, (velocityVector, time));
                }
                else 
                {
                    //Debug.Log("Doesn't work");
                }
            }
        }

        actualEnemy.GetComponent<BoxCollider2D>().enabled = true;

        return possibleWaypointJumps;
    }

    private bool CalculateJumpParabola(Vector3 cellPos, Vector3 endPos, float dist, float timePerDistanceVar, ref Vector2 velocityVector, ref float time) 
    {
        Vector3 waypointPos = cellPos + waypointOffset;
        Vector3 endWaypointPos = endPos + waypointOffset;
        float distX = cellPos.x - endPos.x;
        float distY = cellPos.y - endPos.y;
        time = timePerDistanceVar * Mathf.Pow(dist, 1f / 3f);
        float velocityX = -distX / time;
        float velocityY = -(distY + 0.5f * Physics.gravity.y * time * time) / time;
        //Debug.Log("Distance X: " + distX + "   Distance Y: " + distY);
        //Debug.Log("Velocity X: " + velocityX + "   Velocity Y: " + velocityY);

        float timeForVertex = velocityY / -Physics.gravity.y;
        Vector3 vertexPos = new Vector3(waypointPos.x + timeForVertex * velocityX, waypointPos.y + velocityY * timeForVertex + (Physics.gravity.y * 0.5f * timeForVertex * timeForVertex));
        velocityVector = new Vector2(velocityX, velocityY);

        //Get the colliders of the 2 waypoints in the checker

        if (!Physics2D.Linecast(new Vector2(waypointPos.x, waypointPos.y + 0.01f), vertexPos) && !Physics2D.Linecast(vertexPos, new Vector3(endWaypointPos.x, endWaypointPos.y + 0.01f, 0)))
        {
            Vector3 start = waypointPos;

            for (float t = 0; t < linePoints * timeBetweenPoints; t += timeBetweenPoints)
            {
                if (t >= time)
                {
                    Vector3 destination = endWaypointPos;
                    break;
                }

                Vector3 end = new Vector3(waypointPos.x + t * velocityX, waypointPos.y + velocityY * t + (Physics.gravity.y * 0.5f * t * t));
                //Debug.DrawLine(start, end);
                start = end;
            }

            if (!projection.SimulateTrajectory(enemyHitbox, waypointPos, endWaypointPos, waypointOffset, new Vector2(velocityX, velocityY), time))
            {
                //projection doesn't work return false
                return false;
            }


            return true;
        }
        else
            //Outline of projection doesn't work return false
            return false;
    }

    Dictionary<Vector3Int, Dictionary<Vector3Int, (Vector2, float)>> waypointInfo;
    private void FindGridConnections() 
    {
        waypointInfo = new Dictionary<Vector3Int, Dictionary<Vector3Int, (Vector2, float)>>();
        foreach (KeyValuePair<Vector3Int, GameObject> waypoint in waypointDictionary)
            waypointInfo.Add(waypoint.Key, FindOpenWaypoints(waypoint.Key));

    }
    

    private void FindPathToPlayer(Vector3Int enemyCellPos, Vector3 playerCellPos) 
    {
        List<Vertex> vertexDebugger = new List<Vertex>();
        List<Vertex> queue = new List<Vertex>();
        Vertex currentVertex = new Vertex(0, new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue), enemyCellPos, waypointInfo[enemyCellPos]);
        foreach(KeyValuePair<Vector3Int, (Vector2, float)> waypoint in waypointInfo[enemyCellPos]) 
        {
            Debug.Log(waypoint.Key + " " + waypoint.Value.Item2);
        }
        queue.Add(currentVertex);
        vertexDebugger.Add(currentVertex);
        //Change this into for every waypoint and get their connections
        foreach (KeyValuePair<Vector3Int, Dictionary<Vector3Int, (Vector2, float)>> connections in waypointInfo) 
        {
            if (connections.Key == currentVertex.cellPos)
                continue;

            Vertex vertex = new Vertex(int.MaxValue, new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue), connections.Key, connections.Value);
            queue.Add(vertex);
            vertexDebugger.Add(vertex);
            //Debug.Log(connection.Value.Item2);
        }

        int errorTracker = 0;
        while (queue.Count > 0 && errorTracker < 1000) 
        {
            Vertex minVertex = FindClosestVertex(queue);
            //Debug.Log(minVertex.distance);
            queue.Remove(minVertex);
            Dictionary<Vector3Int, (Vector2, float)> connections = minVertex.connectedCells;
            foreach(KeyValuePair<Vector3Int, (Vector2, float)> connection in connections) 
            {
                Vertex connectionVertex = null;
                foreach(Vertex vertex in queue)
                {
                    if (vertex.cellPos == connection.Key)
                    {
                        connectionVertex = vertex;
                    }
                }

                if(connectionVertex == null)
                    continue;
                
                
                float temporaryDist = minVertex.distance + connection.Value.Item2;
                if (connectionVertex.distance > temporaryDist)
                {
                    connectionVertex.distance = temporaryDist;
                    connectionVertex.previous = minVertex.cellPos;
                }
                   
                
            }
            errorTracker++;
        }

        foreach (Vertex vertex in vertexDebugger)
        {
            Debug.Log("Cell Pos:" + vertex.cellPos + ", Dist:" + vertex.distance);
        }

        Vertex playerVertex = null;

        foreach(Vertex vertex in vertexDebugger) 
        {
            if(vertex.cellPos == playerCellPos) 
            {
                playerVertex = vertex;
            }
        }

        if (playerVertex == null)
            return;

        List<Vertex> movementOrder = new List<Vertex>();

        while (playerVertex.previous != new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue)) 
        {
            int index = vertexDebugger.FindIndex(vertex => vertex.cellPos == playerVertex.previous);
            movementOrder.Insert(0, playerVertex);
            playerVertex = vertexDebugger[index];
        }

        List<Vector3Int> cellPositions = new List<Vector3Int>();
        foreach (Vertex v in movementOrder)
            cellPositions.Add(v.cellPos);


        movementQueue = new List<Vertex>(movementOrder);
        latestVertex = currentVertex;
        Debug.Log(string.Format("Here is the path starting from " + enemyCellPos + ": ({0}).", string.Join(", ", cellPositions)));

    }

    Vertex FindClosestVertex(List<Vertex> queue) 
    {
        Vertex minDistVertex = null;
        float dist = int.MaxValue;
        foreach(Vertex vertex in queue) 
        {
            if(vertex.distance <= dist) 
            {
                dist = vertex.distance;
                minDistVertex = vertex;
            }
        }

        return minDistVertex;
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


        foreach(Vector3Int walkWaypoint in walkWaypoints.Keys) 
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
        foreach(KeyValuePair<Vector3Int, (Vector2, float)> movement in walkWaypoints) 
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

    // Update is called once per frame
    Vector3Int playerWaypoint;

    bool clicked;

    public bool grounded;
    private void FixedUpdate()
    {
        if (clicked)
            EnemyMovement();
    }


    void Update()
    {
        if(enemyGroundCollision.grounded)
            if (Input.GetMouseButtonDown(1))
            {
                playerWaypoint = GetClickedWaypoint();
                Debug.Log("Starting Tile: " + enemyGroundCollision.currentGroundTile + "   Ending Tile:" + playerWaypoint);
                if (playerWaypoint != new Vector3Int(0, 0, 0)) 
                {
                    Debug.Log("Hello");
                    tilemap.SetColor(playerWaypoint, new Color(255, 0, 0));
                    FindPathToPlayer(enemyGroundCollision.currentGroundTile, playerWaypoint);
                }
                    
                clicked = true;
            }

        

    }


    public bool IsBetween(Vector2 valueChecking, Vector2 goal, float offset)
    {
        return (Mathf.Abs(Vector2.Distance(valueChecking, goal)) < offset);
    }

    private Vertex latestVertex;
    bool currentlyMoving;

    Vector3 nextWaypointPos;

    public enum EnemyState
    {
        isStopping,
        isJumping,
        isFalling,
        isRunning
    }

    public EnemyState enemyState;

    [SerializeField] float enemyBottomDistanceFromWaypoint;

    private void EnemyMovement() 
    {
        EnemyStateChange();

        if (nextWaypointPos != Vector3.positiveInfinity)
        {
            
            if (IsBetween(new Vector2(actualEnemyHitbox.bounds.center.x, actualEnemyHitbox.bounds.min.y), nextWaypointPos, enemyBottomDistanceFromWaypoint)) 
            {
                Debug.Log("Hi");
                currentlyMoving = false;
                nextWaypointPos = Vector3.positiveInfinity;
                enemyRB.velocity = new Vector2(0, enemyRB.velocity.y);
                latestVertex = movementQueue[0];
                movementQueue.Remove(movementQueue[0]);
                
            }
                       
        }

        if (currentlyMoving)
            return;

        if(movementQueue.Count > 0) 
        {
            Vector2 enemyVelocity = latestVertex.connectedCells[movementQueue[0].cellPos].Item1;
            if (enemyVelocity.y != 0)
                enemyState = EnemyState.isJumping;
            else
                enemyState = EnemyState.isRunning;

            enemyRB.velocity = enemyVelocity;
            currentlyMoving = true;
            nextWaypointPos = movementQueue[0].cellPos + waypointOffset;
            Debug.Log(nextWaypointPos);
            
        }
            
    }

    private void EnemyStateChange() 
    {
        if(enemyState == EnemyState.isRunning && !enemyGroundCollision.grounded) 
        {
            enemyState = EnemyState.isFalling;
            enemyRB.velocity = new Vector2(0, enemyRB.velocity.y);
        }

        if(enemyState == EnemyState.isFalling && enemyGroundCollision.grounded)
        {
            enemyState = EnemyState.isRunning;
            enemyRB.velocity = new Vector2(latestVertex.connectedCells[movementQueue[0].cellPos].Item1.x, enemyRB.velocity.y);
        }

    }

    
}

public class Vertex
{
    public float distance;
    public Vector3Int previous;
    public Vector3Int cellPos;
    public Dictionary<Vector3Int, (Vector2, float)> connectedCells;

    public Vertex(int distance, Vector3Int previous, Vector3Int cellPos, Dictionary<Vector3Int, (Vector2, float)> connectedCells) 
    {
        this.connectedCells = new Dictionary<Vector3Int, (Vector2, float)>();
        this.distance = distance;
        this.previous = previous;
        this.cellPos = cellPos;
        this.connectedCells = connectedCells;
    }
}