using UnityEngine;

public class ChamberController : MonoBehaviour
{
    [Header("Actuators")]

    [Range(0f, 100f)]
    public float fanSpeed = 0f;

    public bool heaterOn = false;
}