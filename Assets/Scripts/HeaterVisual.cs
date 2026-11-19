using UnityEngine;
public class HeaterVisual : MonoBehaviour
{
    public ChamberController chamberController;
    Renderer heaterRenderer;
    MaterialPropertyBlock block;
    Color originalColor;
    void Start()
    {
        heaterRenderer = GetComponent<Renderer>();
        if (!chamberController) chamberController = FindObjectOfType<ChamberController>();
        block = new MaterialPropertyBlock();
        originalColor = heaterRenderer && heaterRenderer.sharedMaterial &&
            heaterRenderer.sharedMaterial.HasProperty("_Color") ? heaterRenderer.sharedMaterial.color : Color.gray;
    }
    void Update()
    {
        if (!heaterRenderer || !chamberController || !chamberController.Simulation) return;
        var sim = chamberController.Simulation;
        heaterRenderer.GetPropertyBlock(block);
        block.SetColor("_Color", sim.HasReading && sim.Reading.HeaterOn ?
            Color.Lerp(originalColor, new Color(.72f,.61f,.43f), .25f) : originalColor);
        heaterRenderer.SetPropertyBlock(block);
    }
}
