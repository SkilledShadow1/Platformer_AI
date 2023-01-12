using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlaceholderScript : MonoBehaviour
{
    public Tile redSquare;
    [SerializeField] Vector3Int position;
    [SerializeField] Tilemap tilemap;
    [ContextMenu("Paint")]
    void Paint() 
    {
        tilemap.SetTile(position, redSquare);
    }
}
