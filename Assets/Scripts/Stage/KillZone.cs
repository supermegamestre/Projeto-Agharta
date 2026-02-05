using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillZone : MonoBehaviour
{
    [SerializeField]
    private Transform respawnPoint;
    [SerializeField, Range(0f, 10f)]
    private float respawnTime = 5;
    private GameObject player;
    private CameraScript cam;
    private UIAnimationController UI;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraScript>();
        UI = GameObject.FindGameObjectWithTag("Canvas").GetComponent<UIAnimationController>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            StartCoroutine(outOfBounds());
        }
    }
    private IEnumerator outOfBounds()
    {
        UI.TransitionIn();
        player.SetActive(false);
        StartCoroutine(cam.respawnPositionReset(respawnTime));
        yield return new WaitForSeconds(respawnTime);
        player.transform.position = respawnPoint.position;
        player.SetActive(true);
        UI.TransitionOut();
        yield break;
    }
    
}
