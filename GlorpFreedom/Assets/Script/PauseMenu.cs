using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject _PauseMenu;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (_PauseMenu.activeSelf)
            {
                _PauseMenu.SetActive(false);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else if (!_PauseMenu.activeSelf)
            {
                _PauseMenu.SetActive(true);
                Cursor.lockState = CursorLockMode.None; 
                Cursor.visible = true;
            }
        }
    }
}
