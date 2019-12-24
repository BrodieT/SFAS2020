using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CellManager
{
    static public void GoToCell(string cellID)
    {
        if (SceneManager.GetSceneByName(cellID) != null)
        {
            if (SceneManager.GetActiveScene().name != cellID)
            {
                SceneManager.LoadScene(cellID);
            }
            else
            {
                Debug.LogWarning("Already in Cell");
            }
        }
        else
        {
            Debug.LogError("Invalid Cell");
        }
    }
}
