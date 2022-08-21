using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerInteractionCollision : MonoBehaviour
{
    private UIPlayer playerHUD;
    private PlayerController playerController;

    private void Start()
    {
        playerController = transform.root.GetComponent<PlayerController>();
        playerHUD = playerController.playerHUD;
    }

    private void OnTriggerStay(Collider other) 
    {
        Interactable interactable;

        if (!playerController.GetComponent<PhotonView>().IsMine) return;
        
        if (other.GetComponent<Interactable>() == null) return;

        if (other.GetComponent<WeaponSpawner>() != null) // if the interactable object is a weapon spawner and the weapon is disabled, return.
        {
            playerHUD.DisableInteractionPrompt();
            if(other.GetComponent<WeaponSpawner>().isDisabled) return;
        }

        interactable = other.GetComponent<Interactable>();

        playerHUD.DisplayInteractionPrompt(interactable.interactableName, interactable.isEmphasised);

        if (Input.GetButtonDown(InputManager.INTERACT))
        {
            playerHUD.DisableInteractionPrompt();
            interactable.Interact(this.gameObject);
        }
    }

    private void OnTriggerExit(Collider other) 
    {
        if (other.GetComponent<Interactable>() != null)
        {
            playerHUD.DisableInteractionPrompt();
        }
    }
}
