using UnityEngine;

public class DeathTransition : MonoBehaviour
{

    public GameObject fail_canvas;

    public void OpenFailPanel()
    {
        fail_canvas.SetActive(true);
    }
}
