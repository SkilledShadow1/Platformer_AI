using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemyGroundCollision : MonoBehaviour
{
    [SerializeField] Tile groundedTile;
    [SerializeField] WaypointCreator waypointCreator;
    [SerializeField] GameObject tilemap;
    Tilemap t;
    Collider2D enemyCollider;
    public Vector3Int lastHitWaypoint;
    public Vector3Int currentGroundTile;

    private void Awake()
    {
        tilemap = GameObject.FindGameObjectWithTag("Tilemap");
        t = tilemap.GetComponent<Tilemap>();
        enemyCollider = GetComponent<Collider2D>();
        lastHitWaypoint = Vector3Int.CeilToInt(Vector3.positiveInfinity);
    }

    private void Start()
    {
        foreach (Vector3Int tile in t.cellBounds.allPositionsWithin)
        {
            t.SetTileFlags(tile, TileFlags.None);
            
        }
    }


    private void FindCollidingTile()
    {
        Vector3Int cell;
        RaycastHit2D hit;
        hit = Physics2D.Raycast(new Vector2(enemyCollider.bounds.center.x, enemyCollider.bounds.min.y), Vector2.down, 0.1f);
        if (hit) 
        { 

            cell = t.WorldToCell(new Vector3(hit.point.x, hit.point.y, 0));
            currentGroundTile = cell;
        }
        else 
        {
            currentGroundTile = Vector3Int.CeilToInt(Vector3.positiveInfinity);
        }

    }

    private void ColorTiles() 
    {

        foreach (Vector3Int pos in t.cellBounds.allPositionsWithin)
        {
            if (t.GetColor(pos) == new Color(0, 0, 0))
            {
                t.SetColor(pos, new Color(255, 255, 255));
            }
        }

        if (t.ContainsTile(t.GetTile(currentGroundTile)))
        {
            t.SetColor(currentGroundTile, new Color(0, 0, 0));
        }
    }
    public bool grounded;

    void FixedUpdate()
    {
        grounded = Grounded();

        FindCollidingTile();
        ColorTiles();

    }


    [SerializeField] float groundedRaycastDist;
    public bool Grounded()
    {
        //Debug.DrawRay(new Vector3(actualEnemyHitbox.bounds.min.x, actualEnemyHitbox.bounds.min.y, 0), new Vector3(actualEnemyHitbox.bounds.min.x, actualEnemyHitbox.bounds.min.y, 0), Color.cyan);
        RaycastHit2D hit = Physics2D.Raycast(new Vector3(enemyCollider.bounds.min.x - 0.02f, enemyCollider.bounds.min.y, 0), Vector2.down, groundedRaycastDist);
        if (hit)
            return true;

        hit = Physics2D.Raycast(new Vector3(enemyCollider.bounds.center.x, enemyCollider.bounds.min.y, 0), Vector2.down, groundedRaycastDist);
        if (hit)
            return true;

        hit = Physics2D.Raycast(new Vector3(enemyCollider.bounds.max.x + 0.02f, enemyCollider.bounds.min.y, 0), Vector2.down, groundedRaycastDist);
        if (hit)
            return true;

        return false;
    }
}
