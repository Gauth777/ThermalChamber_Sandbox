using UnityEngine;
public class FanVisual : MonoBehaviour
{
    public ChamberController chamberController;
    public float maxRotationSpeed = 800f;
    public Vector3 localRotationAxis = Vector3.up;
    void Start() { if (!chamberController) chamberController = FindObjectOfType<ChamberController>(); }
    void Update()
    {
        if (!chamberController || !chamberController.Simulation) return;
        var sim = chamberController.Simulation;
        if (!sim.HasReading) return;
        transform.Rotate(localRotationAxis.normalized,
            (float)sim.Reading.FanPercent / 100f * maxRotationSpeed * Time.deltaTime, Space.Self);
    }
}
