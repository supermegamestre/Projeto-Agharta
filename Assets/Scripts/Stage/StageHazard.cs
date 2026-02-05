using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class StageHazard : MonoBehaviour
{
    [SerializeField]
    private Transform[] plataformCheckpoints;
    [SerializeField]
    private LayerMask playerLayer;
    [SerializeField, Range(1, 10)]
    private int damage;
    [SerializeField]
    private bool showCheckPoints = true;

    private int checkpointIndex = 0;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        if(showCheckPoints)
        {
            foreach (Transform t in plataformCheckpoints)
            {
                Gizmos.DrawCube(t.position, t.localScale);
            }
        }
    }

    private void Update()
    {
        for (int i = 0; i < plataformCheckpoints.Length; i++)
        {
            if (Physics2D.OverlapBox(plataformCheckpoints[i].position, plataformCheckpoints[i].localScale, 0f, playerLayer))
            {
                checkpointIndex = i;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.transform.position = plataformCheckpoints[checkpointIndex].position;
            collision.GetComponent<UniversalHealthController>().gotHit(damage);
        }
    }
}
