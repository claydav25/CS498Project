using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleMenu : MonoBehaviour
{
    public Canvas teleportMenu;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            Cursor.visible = true;
            teleportMenu.gameObject.SetActive(!teleportMenu.gameObject.activeSelf);
        }
        
    }
}
