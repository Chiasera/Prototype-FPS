using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanyonPathTrigger : MonoBehaviour
{
    public bool isTriggered = false;
    
    //This triggers the first floating stone that will hover towards the player once every target has been destroyed
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.name == "MainCharacter")
        {
            MainCharacterController player = other.GetComponent<MainCharacterController>();
            string score = player.score.text;
            //Not optimal, we could use a variable in this case, but we only check it once
            if (!isTriggered && score == "14")
            {
                isTriggered = true;
                player.triggerStone.animator.SetTrigger("rise");
                player.canRespawn = false;
            }
        }
    }
}
