using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class Projection : MonoBehaviour
{
    [SerializeField] List<GameObject> enemyAIHitboxes = new List<GameObject>();
    private Scene simulationScene;
    private PhysicsScene2D physicsScene;
    private GameObject[] ghostHitboxes;
    [SerializeField] GameObject gridObject;
    // Start is called before the first frame update
    void Start()
    {
        CreatePhysicsScene();
    }

    private void CreatePhysicsScene() 
    {
        simulationScene = SceneManager.CreateScene("PhysicsSim " + transform.parent.name.Substring(5, transform.parent.name.Length - 5), new CreateSceneParameters(LocalPhysicsMode.Physics2D));
        physicsScene = simulationScene.GetPhysicsScene2D();
        ghostHitboxes = new GameObject[enemyAIHitboxes.Count];
        for(int i = 0; i < enemyAIHitboxes.Count; i++)
        {
            GameObject ghostHitbox = Instantiate(enemyAIHitboxes[i]);
            ghostHitbox.GetComponent<Renderer>().enabled = false;
            SceneManager.MoveGameObjectToScene(ghostHitbox, simulationScene);
            ghostHitboxes[i] = ghostHitbox;
           
        }

        GameObject ghostObj = Instantiate(gridObject, gridObject.transform.position, gridObject.transform.rotation);
        foreach(Transform child in ghostObj.transform) 
        {
            if (child.tag != "Tilemap")
                Destroy(child.gameObject);
        }
        ghostObj.GetComponent<Renderer>().enabled = false;
        ghostObj.GetComponent<WaypointInitializer>().enabled = false;
        ghostObj.transform.GetChild(0).tag = "Untagged";
        ghostObj.transform.GetChild(0).GetComponent<TilemapRenderer>().enabled = false;

        SceneManager.MoveGameObjectToScene(ghostObj, simulationScene);
    }

    private List<LineRenderer> lineRenderers = new List<LineRenderer>();
    public bool SimulateTrajectory(GameObject hitBox, Vector3Int startPos, Vector3Int endPos, Vector2 waypointOffset, Vector2 velocity, float totalTime) 
    {
        Vector2 waypointPos = new Vector2(startPos.x + waypointOffset.x, startPos.y + waypointOffset.y);


        Physics2D.simulationMode = SimulationMode2D.Script;

        GameObject currentHitbox = null;
        foreach(GameObject ghostHitbox in ghostHitboxes) 
        {
            if (hitBox.GetComponent<BoxCollider2D>().size == ghostHitbox.GetComponent<BoxCollider2D>().size) 
            {
                currentHitbox = ghostHitbox;
            }
        }
        if (currentHitbox == null) 
        {
            Debug.Log("No Hitbox Matches This Object");
            return false;
        }
        currentHitbox.transform.position = new Vector2(waypointPos.x, waypointPos.y + currentHitbox.GetComponent<BoxCollider2D>().bounds.extents.y);
        
        LineRenderer lr = new GameObject().AddComponent<LineRenderer>();
        lr.GetComponent<LineRenderer>().startWidth = 0.1f;
        lr.GetComponent<LineRenderer>().endWidth = 0.1f;
        lineRenderers.Add(lr);

        GhostHitboxCollision colliderInfo = currentHitbox.GetComponent<GhostHitboxCollision>();
        colliderInfo.pathWorking = true;
        currentHitbox.GetComponent<BoxCollider2D>().enabled = true;
        currentHitbox.transform.position = new Vector2(waypointPos.x, waypointPos.y + currentHitbox.GetComponent<BoxCollider2D>().bounds.extents.y);
        int frames = (int)(totalTime / Time.fixedDeltaTime);
        colliderInfo.InitializeProjection(startPos, endPos);
        currentHitbox.GetComponent<Rigidbody2D>().velocity = Vector3.zero;
        currentHitbox.GetComponent<Rigidbody2D>().AddForce(velocity, ForceMode2D.Impulse);
        lr.positionCount = frames - 1;

        for (int t = 0; t < frames - 1; t += 1) 
        {
            lr.SetPosition(t, currentHitbox.transform.position);
                
            physicsScene.Simulate(Time.fixedDeltaTime);
            if (colliderInfo.pathWorking == false)
            {
                Debug.Log("false  " + t + "   " + frames);
                lr.positionCount = t + 1;
                Destroy(lr.gameObject);
                Physics2D.simulationMode = SimulationMode2D.FixedUpdate;
                return false;
            }
                
        }
        Physics2D.simulationMode = SimulationMode2D.FixedUpdate;
        return true;
    }

    private void DeleteLineRenderers()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            for (int i = 0; i < lineRenderers.Count; i++) 
            {  
                Destroy(lineRenderers[i]);
            }
            lineRenderers.Clear();
        }

        
    }

    private void Update()
    {
        DeleteLineRenderers();
    }
}
