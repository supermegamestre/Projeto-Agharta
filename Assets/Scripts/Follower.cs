using UnityEngine;
public class Follower : MonoBehaviour
{
    public Transform target;
    void Update() => transform.position = target.position;
}
