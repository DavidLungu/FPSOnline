using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Interactable
{
    string interactableName
    {
        get;
        set;
    }

    bool isEmphasised
    {
        get;
        set;
    }
    
    void Interact(GameObject player);
}
