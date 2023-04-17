using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
public class EnemyPathfinding : MonoBehaviour
{
    GameManager gameManager;
    [SerializeField] EnemyGroundCollision enemyGroundCollision;
    public Tilemap tilemap;
    List<Vertex> movementQueue;
    Collider2D actualEnemyHitbox;
    Rigidbody2D enemyRB;
    Vector3Int selectedCell;
    GameObject currentGrid;
    Vector3 waypointOffset;
    int gridNum;
    void Start()
    {
        currentGrid = transform.parent.parent.gameObject;
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        gridNum = int.Parse(currentGrid.name.Substring(5, transform.parent.parent.name.Length - 5));
        Physics2D.queriesStartInColliders = false;
        clicked = false;
        //tilemap = wInitializer.tilemap;
        selectedCell = new Vector3Int(0, 0, 0);
        
        //Know that these are changed
        enemyRB = GetComponent<Rigidbody2D>();
        actualEnemyHitbox = GetComponent<Collider2D>();
        currentlyMoving = false;
        nextWaypointPos = Vector3.positiveInfinity;
        movementQueue = new List<Vertex>(0);

        enemyState = EnemyState.isStopping;
        waypointOffset = new Vector3(currentGrid.transform.position.x + WaypointInitializer.waypointBaseOffset.x, currentGrid.transform.position.y +
                                                                    WaypointInitializer.waypointBaseOffset.y, currentGrid.transform.position.z + WaypointInitializer.waypointBaseOffset.z);
        gameObject.GetComponent<BoxCollider2D>().enabled = true;
    }



    

    private Vector3Int GetClickedWaypoint()
    {
        if (int.Parse(tilemap.transform.parent.name.Substring(5, tilemap.transform.parent.name.Length - 5)) == gridNum) 
        {
            Vector3 worldCoord = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            selectedCell = tilemap.WorldToCell(worldCoord);
            if (tilemap.HasTile(selectedCell))
            {
                //tilemap.SetColor(selectedCell, new Color(0, 0, 0));
                return selectedCell;
            }

            
        }

        return new Vector3Int(0, 0, 0);
    }

    

    private void FindPathToPlayer(Vector3Int enemyCellPos, Vector3 playerCellPos) 
    {
        List<Vertex> vertexDebugger = new List<Vertex>();
        List<Vertex> queue = new List<Vertex>();

        Vertex currentVertex = new Vertex(0, new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue), enemyCellPos, gameManager.levelWaypointInfo[gridNum][enemyCellPos]);
        queue.Add(currentVertex);
        vertexDebugger.Add(currentVertex);
        //Change this into for every waypoint and get their connections
        foreach (KeyValuePair<Vector3Int, Dictionary<Vector3Int, (Vector2, float)>> connections in gameManager.levelWaypointInfo[gridNum]) 
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
        //Debug.Log(string.Format("Here is the path starting from " + enemyCellPos + ": ({0}).", string.Join(", ", cellPositions)));

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

    Vector3Int playerWaypoint;

    bool clicked;
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
                //Debug.Log("Starting Tile: " + enemyGroundCollision.currentGroundTile + "   Ending Tile:" + playerWaypoint);
                if (playerWaypoint != new Vector3Int(0, 0, 0)) 
                {
                    //Debug.Log("Hello");
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

    //Log when velocities hit 0

    private void NextWaypoint() 
    {
        enemyState = EnemyState.isStopping;
        currentlyMoving = false;
        enemyRB.velocity = new Vector2(0, enemyRB.velocity.y);
        transform.position = new Vector3(nextWaypointPos.x, transform.position.y, transform.position.z);
        nextWaypointPos = Vector3.positiveInfinity;
        latestVertex = movementQueue[0];
        movementQueue.Remove(movementQueue[0]);
    }

    private void EnemyMovement()
    {
        if (movementQueue.Count <= 0)
            return;
        Vector2 enemyVelocity = latestVertex.connectedCells[movementQueue[0].cellPos].Item1;
        EnemyStateChange();

        if (nextWaypointPos != Vector3.positiveInfinity)
        {
            if(enemyState == EnemyState.isRunning) 
            {
                //if moving left
                if (enemyVelocity.x < 0 && transform.position.x - nextWaypointPos.x < 0)
                {
                    NextWaypoint();
                    return;
                }
            
                
                //if moving right
                else if(enemyVelocity.x > 0 && transform.position.x - nextWaypointPos.x > 0) 
                {
                    NextWaypoint();
                    return;
                }

            }

            else if (enemyState == EnemyState.isJumping && IsBetween(new Vector2(actualEnemyHitbox.bounds.center.x, actualEnemyHitbox.bounds.min.y), nextWaypointPos, enemyBottomDistanceFromWaypoint) && enemyGroundCollision.grounded) 
            {
                NextWaypoint();
                return;
            }
                       
        }

        if (currentlyMoving)
            return;

        if(movementQueue.Count > 0) 
        {
           
            if (enemyVelocity.y != 0)
            {
                //Debug.Log("Now Jumping");
                enemyState = EnemyState.isJumping;
            }

            else 
            {
                //Debug.Log("Now Running");
                enemyState = EnemyState.isRunning;
            }
                

            enemyRB.velocity = enemyVelocity;
            currentlyMoving = true;
            nextWaypointPos = movementQueue[0].cellPos + waypointOffset;
            //Debug.Log(nextWaypointPos);
            
        }
            
    }

    private void EnemyStateChange() 
    {
        if(enemyState == EnemyState.isRunning && !enemyGroundCollision.grounded) 
        {
            //Debug.Log("Now Falling");
            enemyState = EnemyState.isFalling;
            enemyRB.velocity = new Vector2(0, enemyRB.velocity.y);
        }

        if(enemyState == EnemyState.isFalling && enemyGroundCollision.grounded)
        {
            //Debug.Log("Stopped Falling");
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