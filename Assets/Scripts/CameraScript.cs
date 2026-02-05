using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    [SerializeField]
    private float cameraControlDistance = 1.1f, normalSize = 15, combatSize = 10, cameraTransitionDamp = 0.25f;
    [SerializeField]
    private UIAnimationController UI;
    private Camera cam;
    private float nullFloat = 0f;

    public Transform target, respawn;
    private GameObject player;
    private Rigidbody2D playerRb;
    [SerializeField, Range(0f, 10f)]
    private float aheadLead = 0.1f;
    [SerializeField, Range(0.1f, 10f)]
    private float interpolatorMultiplier = 5f, smoothTime = 5f;
    private Vector3 targetPosition, refVelocity = Vector2.zero;

    private void cameraAhead()
    {
        if (player.activeSelf == true)
        {
            targetPosition = target.position + Vector3.ClampMagnitude(new Vector3(playerRb.velocity.x, playerRb.velocity.y, -10), aheadLead);
            targetPosition.z = -10f;
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref refVelocity, smoothTime);
        }
    }
    private void cameraControl()
    {
        targetPosition.x = target.position.x + Input.GetAxisRaw("Horizontal") * cameraControlDistance;
        targetPosition.y = target.position.y + Input.GetAxisRaw("Vertical") * cameraControlDistance;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref refVelocity, Time.deltaTime * interpolatorMultiplier);
    }

    public void combatCamera()
    {
        cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, combatSize, ref nullFloat, cameraTransitionDamp);
    }
    public void normalCamera()
    {
        cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, normalSize, ref nullFloat, cameraTransitionDamp);
    }


    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerRb = player.GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        
        if (Input.GetButton("Fire 5"))
        {
            normalCamera();
            cameraControl();
        }
        else
        {
            normalCamera();
            cameraAhead();
        }

        
    }
    
    public IEnumerator respawnPositionReset(float timer)
    {
        yield return new WaitForSeconds(timer);
        transform.position = new Vector3(respawn.position.x, respawn.position.y, -10);
        yield return null;
    }


}
