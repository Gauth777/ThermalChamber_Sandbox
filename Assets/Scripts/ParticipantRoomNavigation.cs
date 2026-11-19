using UnityEngine;
using UnityEngine.SceneManagement;
public class ParticipantRoomNavigation : MonoBehaviour
{
    void OnGUI()
    {
        GUI.Box(new Rect(12,12,480,65),"PARTICIPANT ROOM — desktop placeholder; XR not enabled");
        GUI.enabled=Application.CanStreamedLevelBeLoaded("EngineeringTwin");
        if(GUI.Button(new Rect(22,44,450,25),"Return to engineering view (new experiment)"))
            SceneManager.LoadSceneAsync("EngineeringTwin");
        GUI.enabled=true;
        if(!Application.CanStreamedLevelBeLoaded("EngineeringTwin")) GUI.Label(new Rect(12,80,600,30),"EngineeringTwin is not in Build Settings. Run scene generation first.");
    }
}
