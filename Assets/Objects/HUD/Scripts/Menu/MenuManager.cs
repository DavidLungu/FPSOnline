using UnityEngine;

public class MenuManager : MonoBehaviour
{
   [SerializeField] private UIMenu[] menus;

   public static MenuManager Instance;

   private void Awake() {
      if (Instance != null) {
         Destroy(this);
         return;
      }

      Instance = this;
   }

   public void OpenMenu(string _menuName) {
      for (int i = 0; i < menus.Length; i++) 
      {
         if (menus[i].GetMenuName() == _menuName) {
            OpenMenu(menus[i]);
         
         } else if (menus[i].IsOpen()) {
            
            CloseMenu(menus[i]);
         }
      }
   }

   public void OpenMenu(UIMenu _menu) {
      for (int i = 0; i < menus.Length; i++) 
      {
         if(menus[i].IsOpen()) {
            CloseMenu(menus[i]);
         }
      }

      _menu.OpenMenu();
   }

   public void CloseMenu(UIMenu _menu) {
      _menu.CloseMenu();
   }
}
