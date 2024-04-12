using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempTeleportPlayer : MonoBehaviour
{
    public GameObject spawnPoint;
    private Transform camera;

    // Start is called before the first frame update
    void Start()
    {
        camera = GameObject.FindWithTag("PosController").transform;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Click()
    {
        camera.position = spawnPoint.transform.position;
    }
}
