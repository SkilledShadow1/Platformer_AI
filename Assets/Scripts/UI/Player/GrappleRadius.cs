using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleRadius : MonoBehaviour
{
    //Set Nodes to have the GrappleNode Tag
    [SerializeField] float grappleRadius;
    [SerializeField] float launchForce;
    [SerializeField] GameObject arrowPrefab;
    [SerializeField] GameObject pivotPrefab;
    [SerializeField] float arrowYOffset;
    Collider2D currentNode; 
    public bool isGrappling;
    Rigidbody2D rb;
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 255, 0);
        Gizmos.DrawWireSphere(transform.position, grappleRadius);
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void FindNodes()
    {
        currentNode = FindClosestNode();
        if(currentNode != null) 
        {
            Debug.Log("workin");
            //This is to keep track if the current of which node the player needs to touch
            //If the player touches another node the it is not launching at, it could mess up the code
            Grapple(currentNode.transform);
            //Put code here
        }

        
    }

    public void Update()
    {
        GrappleAssist();
    }

    Collider2D FindClosestNode() 
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, grappleRadius);
        float closestNodeDistance = Mathf.Infinity;
        Collider2D chosenNode = null;
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].CompareTag("GrappleNode"))
            {
                float nodeDistance = Vector2.Distance(colliders[i].transform.position, transform.position);
                if (closestNodeDistance > nodeDistance)
                {
                    chosenNode = colliders[i];
                }
            }
        
        }

        return chosenNode;
    }

    //Cancel out all other forces action upon you
    //Find angle between player and node and launch yourself
    //Work on giving a force based on angle
    void Grapple(Transform node) 
    {
        //Changing the state to grappling
        PlayerState.Instance.ChangeState(PlayerState.playerState.GRAPPLING);
        Debug.Log(node.gameObject.name);
        Vector2 direction = node.position - transform.position;
        direction.Normalize();
        Debug.Log(direction);
        rb.velocity = new Vector2(direction.x * launchForce, direction.y * launchForce);

        //Fix rb.velocity to go towards the grapple node
    }


    //Changes states if the player passes through the grapple point
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(PlayerState.Instance.currentState == PlayerState.playerState.GRAPPLING) 
        {
            if(collision == currentNode) 
            {
                PlayerState.Instance.ChangeState(PlayerState.playerState.IDLE);
            }    
        }
    }

    GameObject pivot;
    GameObject currentArrow;
    Collider2D previousNode;
    private void GrappleAssist() 
    {
        Collider2D closestNode = FindClosestNode();
        if (closestNode != null && closestNode == previousNode) 
        {
            pivot.transform.eulerAngles = new Vector3(0, 0, -Vector2.SignedAngle(previousNode.transform.position - transform.position, Vector2.up));
            //Debug.Log(-Vector2.SignedAngle(previousNode.transform.position - transform.position, Vector2.up));
        }
        else if(closestNode != null) 
        {
            if (pivot != null) 
            {
                Destroy(pivot);
            }

            pivot = Instantiate(pivotPrefab, closestNode.transform.position, Quaternion.identity);
            pivot.transform.parent = closestNode.transform;
            currentArrow = Instantiate(arrowPrefab, new Vector2(closestNode.transform.position.x, closestNode.transform.position.y + arrowYOffset), arrowPrefab.transform.rotation);
            currentArrow.transform.parent = pivot.transform;
                
            previousNode = closestNode;
        }
        else 
        {
            if (pivot != null) 
            {
                Destroy(pivot);
                previousNode = null;
            }
                
            
        }
    }

}   
