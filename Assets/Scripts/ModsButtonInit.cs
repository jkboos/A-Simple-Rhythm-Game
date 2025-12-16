using UnityEngine;
using UnityEngine.UI;

public class ModsButtonInit : MonoBehaviour
{
    public Material outline_material;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (Transform button in transform.GetChild(1).transform)
        {
            if (StateController.mods[button.name])
            {
                button.transform.localScale = new Vector3(1, 1, 1.05f);
                button.transform.rotation = Quaternion.Euler(0, 0, -8);
                button.tag = "mod-active";
                button.gameObject.GetComponent<Image>().material = outline_material;
            }
        }
    }
}
