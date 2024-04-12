/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportPlayer : MonoBehaviour
{
    private GameObject spawnPoint;
    private GameObject player;
    

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("PosController");
        //spawnPoint = GameObject.FindWithTag("PosController");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeCameraPos()
    {
        Debug.Log("Hello?");
        player.Teleport(spawnPoint.transform.position, false);
    }
}
*/
