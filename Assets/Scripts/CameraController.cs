using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    bool gameStart = false;
    public GameObject player = new GameObject();
    Vector3 Direction = new Vector3();
    // Update is called once per frame
    void Update()
    {
        if(!player)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            gameStart = true;
        }

        if (gameStart)
        {
           

        }
    }
}
