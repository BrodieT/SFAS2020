using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    bool gameStart = false;
    public GameObject player;

    // Update is called once per frame   
    void Update()
    {
        if(!player && !gameStart)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            gameStart = true;
        }

        if (gameStart && player)
        {
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, new Vector3(player.transform.position.x, Camera.main.transform.position.y, player.transform.position.z - 135), 0.5f);
        }
    }
}
