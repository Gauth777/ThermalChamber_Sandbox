using UnityEngine;

public class HeaterVisual : MonoBehaviour
{
    public ChamberController chamberController;

    private Renderer heaterRenderer;

    void Start()
    {
        heaterRenderer = GetComponent<Renderer>();
    }

    void Update()
    {
        if (chamberController == null)
            return;

        if (chamberController.heaterOn)
        {
            heaterRenderer.material.color = Color.red;
        }
        else
        {
            heaterRenderer.material.color = Color.gray;
        }
    }
}