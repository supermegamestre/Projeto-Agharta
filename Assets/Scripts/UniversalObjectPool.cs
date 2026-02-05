using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UniversalObjectPool : MonoBehaviour
{
    [SerializeField]
    private new string tag;
    [SerializeField]
    private GameObject prefab;
    
    private GameObject[] aimGenObjects;
    [HideInInspector]
    public ObjectPool pool;

    private void Awake()
    {
        aimGenObjects = GameObject.FindGameObjectsWithTag(tag);
        pool = new ObjectPool(prefab, aimGenObjects.Length, transform);
    }
}
