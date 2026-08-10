using UnityEngine;

public class FanVisual : MonoBehaviour
{
    public ChamberController chamberController;

    public float maxRotationSpeed = 800f;

    void Update()
    {
        if (chamberController == null)
            return;

        float rotationSpeed =
            (chamberController.fanSpeed / 100f)
            * maxRotationSpeed;

        transform.Rotate(
            0f,
            rotationSpeed * Time.deltaTime,
            0f,
            Space.Self
        );
    }
}