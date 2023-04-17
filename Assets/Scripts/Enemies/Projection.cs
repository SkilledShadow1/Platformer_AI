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
    [SerializeField] GameObject tilemapObject;
    // Start is called before the first frame update
    void Start()
    {
        CreatePhysicsScene();
    }

    private void CreatePhysicsScene() 
    {
        simulationScene = SceneManager.CreateScene("PhysicsSimulation", new CreateSceneParameters(LocalPhysicsMode.Physics2D));
        physicsScene = simulationScene.GetPhysicsScene2D();
        ghostHitboxes = new GameObject[enemyAIHitboxes.Count];
        for(int i = 0; i < enemyAIHitboxes.Count; i++)
        {
            GameObject ghostHitbox = Instantiate(enemyAIHitboxes[i]);
            ghostHitbox.GetComponent<Renderer>().enabled = false;
            SceneManager.MoveGameObjectToScene(ghostHitbox, simulationScene);
            ghostHitboxes[i] = ghostHitbox;
           
        }

        GameObject ghostObj = Instantiate(tilemapObject, tilemapObject.transform.position, tilemapObject.transform.rotation);
        ghostObj.GetComponent<Renderer>().enabled = false;
        ghostObj.transform.GetChild(0).tag = "Untagged";
        ghostObj.transform.GetChild(0).GetComponent<TilemapRenderer>().enabled = false;

        SceneManager.MoveGameObjectToScene(ghostObj, simulationScene);
    }

    [SerializeField] private LineRenderer line;
    LineRenderer lr;
    private List<LineRenderer> lineRenderers = new List<LineRenderer>();
    public bool SimulateTrajectory(GameObject hitBox, Vector2 waypointPos, Vector2 endPos, Vector2 waypointOffset, Vector2 velocity, float totalTime) 
    {
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
        
        lr = new GameObject().AddComponent<LineRenderer>();
        lr.GetComponent<LineRenderer>().startWidth = 0.1f;
        lr.GetComponent<LineRenderer>().endWidth = 0.1f;
        lineRenderers.Add(lr);

        GhostHitboxCollision colliderInfo = currentHitbox.GetComponent<GhostHitboxCollision>();
        colliderInfo.pathWorking = true;
        currentHitbox.GetComponent<BoxCollider2D>().enabled = true;
        currentHitbox.transform.position = new Vector2(waypointPos.x, waypointPos.y + currentHitbox.GetComponent<BoxCollider2D>().bounds.extents.y);
        int frames = (int)(totalTime / Time.fixedDeltaTime);
        colliderInfo.InitializeProjection(Vector3Int.RoundToInt(waypointPos - waypointOffset), Vector3Int.RoundToInt(endPos - waypointOffset));
        currentHitbox.GetComponent<Rigidbody2D>().velocity = Vector3.zero;
        currentHitbox.GetComponent<Rigidbody2D>().AddForce(velocity, ForceMode2D.Impulse);
        lr.positionCount = frames;
        for (int t = 0; t < frames; t += 1) 
        {
            lr.SetPosition(t, currentHitbox.transform.position);
            physicsScene.Simulate(Time.fixedDeltaTime);
            if (colliderInfo.pathWorking == false)
            {
                //Debug.Log("false");
                //lr.positionCount = t + 1;
                Destroy(lr);
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
