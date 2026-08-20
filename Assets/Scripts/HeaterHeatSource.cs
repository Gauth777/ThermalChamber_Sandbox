using UnityEngine;

public class HeaterHeatSource : MonoBehaviour
{
    public ChamberController chamberController;

    [Header("Heater")]
    public float heatOutput = 2.5f;
    public float heatingRadius = 6f;

    private ThermalZone[] zones;

    void Start()
    {
        zones = FindObjectsOfType<ThermalZone>();

        if (chamberController == null)
        {
            chamberController =
                FindObjectOfType<ChamberController>();
        }
    }

    void Update()
    {
        if (chamberController == null)
            return;

        if (!chamberController.heaterOn)
            return;

        foreach (ThermalZone zone in zones)
        {
            // Horizontal distance only
            Vector2 heaterPosition =
                new Vector2(
                    transform.position.x,
                    transform.position.z
                );

            Vector2 zonePosition =
                new Vector2(
                    zone.transform.position.x,
                    zone.transform.position.z
                );

            float distance =
                Vector2.Distance(
                    heaterPosition,
                    zonePosition
                );

            if (distance > heatingRadius)
                continue;

            // Strong near heater, weak farther away
            float strength =
                1f -
                (distance / heatingRadius);

            float addedHeat =
                heatOutput *
                strength *
                Time.deltaTime;

            zone.AddHeat(addedHeat);
        }
    }
}