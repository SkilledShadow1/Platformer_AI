using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GhostHitboxCollision : MonoBehaviour
{
    //Get the tilemap
    [SerializeField] GameObject tilemapObject;
    Tilemap tilemap;
    TilemapCollider2D tilemapCollider;
    Vector3Int start;
    Vector3Int end;
    public bool pathWorking;
    public float yBound;

    //Make it only have one reference to the box collider (attach to enemy at some point)
    private void Awake()
    {
        tilemapObject = GameObject.FindGameObjectWithTag("Tilemap");
        tilemap = tilemapObject.GetComponent<Tilemap>();
        tilemapCollider = tilemapObject.GetComponent<TilemapCollider2D>();
        
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

        Vector3Int collisionPoint = new Vector3Int(0, 0, 0);
        
        if (collisionPoint != start && collisionPoint != end) 
        {
            pathWorking = false;
        }
            

    }
}
