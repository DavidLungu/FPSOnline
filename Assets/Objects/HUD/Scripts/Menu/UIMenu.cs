using UnityEngine;

public class UIMenu : MonoBehaviour
{
    [SerializeField] private string menuName;
    [SerializeField] private bool isOpen;

    public string GetMenuName() => menuName;
    public bool IsOpen() => isOpen;


    public void OpenMenu() {
        isOpen = true;
        gameObject.SetActive(true);
        Debug.LogFormat("Opened {0}", menuName);
    }

    public void CloseMenu() {
        isOpen = false;
        gameObject.SetActive(false);
        Debug.LogFormat("Closed {0}", menuName);
    }
}
