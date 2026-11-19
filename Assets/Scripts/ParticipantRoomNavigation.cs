using UnityEngine;
using UnityEngine.SceneManagement;
public class ParticipantRoomNavigation : MonoBehaviour
{
    void OnGUI()
    {
        GUI.Box(new Rect(12,12,480,65),"PARTICIPANT ROOM — desktop placeholder; XR not enabled");
        if(GUI.Button(new Rect(22,44,450,25),"Return to engineering view (new experiment)"))
            SceneManager.LoadScene("EngineeringTwin");
    }
}
