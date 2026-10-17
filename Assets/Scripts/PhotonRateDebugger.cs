
using UnityEngine;
using Photon.Pun;

public class PhotonRateDebugger : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("Photon Send Rate: " + PhotonNetwork.SendRate);
        Debug.Log("Photon Serialization Rate: " + PhotonNetwork.SerializationRate);
    }
}
