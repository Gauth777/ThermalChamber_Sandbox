using UnityEngine;
public class InspectionCamera : MonoBehaviour
{
    public Vector3 target;
    public float reservedLeftPixels=365;
    public float distance = 12, yaw = 40, pitch = 25;
    Renderer selected;
    Camera view;
    void Start()
    {
        view = GetComponent<Camera>();
        Vector3 forward = transform.forward;
        yaw = transform.eulerAngles.y;
        pitch = Mathf.DeltaAngle(0,transform.eulerAngles.x);
        target = transform.position + forward * distance;
    }
    void LateUpdate()
    {
        if (!view || Input.mousePosition.x < reservedLeftPixels) return;
        if (Input.GetMouseButton(1))
        {
            yaw += Input.GetAxis("Mouse X")*4;
            pitch = Mathf.Clamp(pitch-Input.GetAxis("Mouse Y")*4,-80,80);
        }
        if (Input.GetMouseButton(2)) target -=
            (transform.right*Input.GetAxis("Mouse X")+transform.up*Input.GetAxis("Mouse Y"))*distance*.025f;
        distance = Mathf.Clamp(distance*Mathf.Exp(-Input.mouseScrollDelta.y*.12f),.1f,5000);
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray=view.ScreenPointToRay(Input.mousePosition);
            float closest=float.PositiveInfinity;
            selected=null;
            foreach (var renderer in FindObjectsOfType<Renderer>())
            {
                float hit;
                if (!renderer.enabled || renderer.GetComponent<ThermalZone>()) continue;
                if (renderer.bounds.IntersectRay(ray,out hit) && hit < closest) { closest=hit; selected=renderer; }
            }
        }
        if (Input.GetKeyDown(KeyCode.F) && selected) { target=selected.bounds.center; distance=Mathf.Max(.2f,selected.bounds.size.magnitude*1.5f); }
        transform.rotation=Quaternion.Euler(pitch,yaw,0);
        transform.position=target-transform.forward*distance;
    }
    void OnGUI()
    {
        if (selected) GUI.Box(new Rect(370,12,Mathf.Max(150,Screen.width-390),30),
            "Selected: "+selected.name+"  |  F to frame (bounds selection)");
    }
}
