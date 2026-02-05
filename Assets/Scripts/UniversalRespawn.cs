using System.Collections;
using UnityEngine;

public class UniversalRespawn : MonoBehaviour
{
    private CameraScript cam;
    private UIAnimationController UI;
    private UIHealthController HealthController;

    private void Awake()
    {
        UI = GameObject.FindGameObjectWithTag("Canvas").GetComponent<UIAnimationController>();
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraScript>();
        HealthController = GameObject.FindGameObjectWithTag("UI Health Controller").GetComponent <UIHealthController>();
    }

    public void respawn(bool isNpc, GameObject @object, float timer)
    {
        StartCoroutine(CoroutineRespawn(isNpc, @object, timer));
    }

    private IEnumerator CoroutineRespawn(bool isNpc, GameObject @object, float timer)
    {
        if (!isNpc)
        {
            UI.TransitionIn();
            @object.SetActive(false);
            StartCoroutine(cam.respawnPositionReset(timer));
            yield return new WaitForSeconds(timer);
            @object.GetComponent<UniversalHealthController>().respawned();
            HealthController.respawn();
            @object.transform.position = transform.position;
            @object.SetActive(true);
            UI.TransitionOut();
            yield return null;
        }
    }
}
