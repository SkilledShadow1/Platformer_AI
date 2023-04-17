using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public List<GameObject> grids = new List<GameObject>();
    public int levelToDelete;
    public int gridToDelete;
    public int gridAmount;
    public bool createWaypoints;
    public Dictionary<int, Dictionary<Vector3Int, Dictionary<Vector3Int, (Vector2, float)>>> levelWaypointInfo = new Dictionary<int, Dictionary<Vector3Int, Dictionary<Vector3Int, (Vector2, float)>>>();
    // Create a new instance of the Random class to generate random numbers
    System.Random random = new System.Random();

    // Generate random values for the keys and values of the dictionary
    public bool resetGrids;

    int sceneIndex;
    private void Awake()
    {
        Physics2D.simulationMode = SimulationMode2D.FixedUpdate;
        levelWaypointInfo.Clear();
        createWaypoints = PlayerPrefs.GetInt("CreateWaypoints") != 0 ? true : false;
        sceneIndex = SceneManager.GetActiveScene().buildIndex;
        if(createWaypoints)
            DatabaseStuff.Init(sceneIndex, this, true);
        else
            DatabaseStuff.Init(sceneIndex, this, false);
            
    }

    void Start()
    {
        levelToDelete = sceneIndex;
        if (createWaypoints)
        {
            grids.Clear();
            gridAmount = 0;
            Debug.Log("HELLO");
            foreach (Transform child in transform)
            {
                if (child.tag == "Grid")
                {
                    child.name = "Grid " + gridAmount;
                    grids.Add(child.gameObject);
                    gridAmount += 1;

                    WaypointInitializer initializer = child.GetComponent<WaypointInitializer>();
                    initializer.Init();
                    Debug.Log("Hello");
                }
            }
        }
    }

    private void OnApplicationQuit()
    {
        DatabaseStuff.CloseConnection();
    }
}


[CustomEditor(typeof(GameManager))]
public class MyScriptEditor : Editor
{
    private SerializedObject scriptObj;
    private SerializedProperty gridtoDeleteVar;
    private SerializedProperty createWaypoints;

    private void OnEnable()
    {
        scriptObj = new SerializedObject(target);
        gridtoDeleteVar = scriptObj.FindProperty(nameof(GameManager.gridToDelete));
        createWaypoints = scriptObj.FindProperty(nameof(GameManager.createWaypoints));
    }

    public override void OnInspectorGUI()
    {
        GameManager script = (GameManager)target;


        serializedObject.Update();


        EditorGUILayout.BeginHorizontal();


        GUIStyle rightStyle = GUI.skin.GetStyle("Label");
        rightStyle.alignment = TextAnchor.MiddleRight;
        rightStyle.fontStyle = FontStyle.Bold;

        EditorGUILayout.LabelField("Create Waypoints", rightStyle);
        PlayerPrefs.SetInt("CreateWaypoints", EditorGUILayout.Toggle(createWaypoints.boolValue) ? 1 : 0);
        createWaypoints.boolValue = PlayerPrefs.GetInt("CreateWaypoints") != 0 ? true : false;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        GUI.skin.box.normal.background = EditorGUIUtility.whiteTexture;
        GUI.skin.box.padding = new RectOffset(10, 10, 10, 10);

        EditorGUILayout.BeginVertical(GUI.skin.box);

        GUIStyle centeredStyle = GUI.skin.GetStyle("Label");
        centeredStyle.alignment = TextAnchor.MiddleCenter;
        centeredStyle.fontStyle = FontStyle.Bold;

        EditorGUILayout.LabelField("WaypointDeleter", centeredStyle);
        EditorGUILayout.Space();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUIUtility.labelWidth = 100f;
        EditorGUILayout.PropertyField(gridtoDeleteVar);
        if (GUILayout.Button("Delete Grid Waypoints"))
        {
           DatabaseStuff.DeleteWaypoints(script.levelToDelete, script.gridToDelete);
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        EditorGUILayout.Space();

        if (GUILayout.Button("Delete All Current Level Waypoints"))
        {
            for(int i = 0; i < script.gridAmount; i++) 
            {
                DatabaseStuff.DeleteWaypoints(script.levelToDelete, script.gridToDelete);
                
            }
        }

        EditorGUILayout.EndVertical();
        serializedObject.ApplyModifiedProperties();

    }
}
