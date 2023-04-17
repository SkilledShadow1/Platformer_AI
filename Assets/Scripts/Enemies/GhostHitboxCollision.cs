using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GhostHitboxCollision : MonoBehaviour
{
    //Get the tilemap
    [SerializeField] GameObject tilemapObject;
    [SerializeField] float groundedRaycastDist;
    Tilemap tilemap;
    TilemapCollider2D tilemapCollider;
    Collider2D thisCollider;
    Vector3Int start;
    Vector3Int end;
    Vector3Int collisionPoint;
    public bool pathWorking;
    public float yBound;

    //Make it only have one reference to the box collider (attach to enemy at some point)
    private void Awake()
    {
        thisCollider = GetComponent<Collider2D>();
        tilemapObject = GameObject.FindGameObjectWithTag("Tilemap");
        tilemap = tilemapObject.GetComponent<Tilemap>();
        tilemapCollider = tilemapObject.GetComponent<TilemapCollider2D>();
        Vector3Int collisionPoint = new Vector3Int(0, 0, 0);

    }

    public void InitializeProjection(Vector3Int start, Vector3Int end) 
    {
        this.start = start;
        this.end = end;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.collider == tilemapCollider)
            return;

        Vector3 hitPosition = collision.contacts[0].point;

        // Convert the hit position to the grid coordinates of the tilemap
        Vector3Int hitTile = tilemap.WorldToCell(hitPosition);

        if (hitTile != start && hitTile != end) 
        {
            Debug.Log(collisionPoint + "  " + start + "  " + end);
            pathWorking = false;
        }
        else 
        {
            RaycastHit2D hit = Physics2D.Raycast(new Vector3(thisCollider.bounds.center.x, thisCollider.bounds.min.y, 0), Vector2.down, groundedRaycastDist);

            hit = Physics2D.Raycast(new Vector3(thisCollider.bounds.center.x, thisCollider.bounds.min.y, 0), Vector2.down, groundedRaycastDist, ~0);
            if (!hit)
                pathWorking = false;
        }

        //this is to make sure that even if you come into contact with the object that you aren't hitting the bottom of it

            //Debug.DrawRay(new Vector3(actualEnemyHitbox.bounds.min.x, actualEnemyHitbox.bounds.min.y, 0), new Vector3(actualEnemyHitbox.bounds.min.x, actualEnemyHitbox.bounds.min.y, 0), Color.cyan);

    }
}
