using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEndTrigger : MonoBehaviour
{
    //Triggers the end of the game, see Player's class.
    private void OnTriggerEnter(Collider other)
    {
        if(other.name == "MainCharacter")
        {
            other.GetComponent<MainCharacterController>().WinGame();
        }
    }
}
