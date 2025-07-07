using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneManager : MonoBehaviour
{
    [SerializeField] List<GameObject> planes = new List<GameObject>();
    void Start()
    {

    }

    // Update is called once per frame
    void Update() { }
    public List<GameObject> GetPlanes()
    {
        return planes;
    }
}
