using UnityEngine;
using System.Collections;

public class Demo2DJumpAndRun : MonoBehaviour 
{
    public Transform location;

    void OnJoinedRoom()
    {
        if( PhotonNetwork.isMasterClient == false )
        {
            return;
        }

        PhotonNetwork.InstantiateSceneObject("Physics Box", location.position, Quaternion.identity, 0, null);
        PhotonNetwork.InstantiateSceneObject("Physics Box", location.position, Quaternion.identity, 0, null);
        PhotonNetwork.InstantiateSceneObject("Physics Box", location.position, Quaternion.identity, 0, null);
        PhotonNetwork.InstantiateSceneObject("Physics Box", location.position, Quaternion.identity, 0, null);
        PhotonNetwork.InstantiateSceneObject("Physics Box", location.position, Quaternion.identity, 0, null);
            PhotonNetwork.InstantiateSceneObject("Physics Box", location.position, Quaternion.identity, 0, null);
            PhotonNetwork.InstantiateSceneObject("Physics Box", location.position, Quaternion.identity, 0, null);
            PhotonNetwork.InstantiateSceneObject("Physics Box", location.position, Quaternion.identity, 0, null);
            PhotonNetwork.InstantiateSceneObject("Physics Box", location.position, Quaternion.identity, 0, null);
            PhotonNetwork.InstantiateSceneObject("Physics Box", location.position, Quaternion.identity, 0, null);
        PhotonNetwork.InstantiateSceneObject("Physics Box", location.position, Quaternion.identity, 0, null);
        PhotonNetwork.InstantiateSceneObject("Physics Box", location.position, Quaternion.identity, 0, null);
        PhotonNetwork.InstantiateSceneObject("Physics Box", location.position, Quaternion.identity, 0, null);
        PhotonNetwork.InstantiateSceneObject("Physics Box", location.position, Quaternion.identity, 0, null);
        PhotonNetwork.InstantiateSceneObject("Physics Box", location.position, Quaternion.identity, 0, null);
            PhotonNetwork.InstantiateSceneObject("Physics Box", location.position, Quaternion.identity, 0, null);
            PhotonNetwork.InstantiateSceneObject("Physics Box", location.position, Quaternion.identity, 0, null);
            PhotonNetwork.InstantiateSceneObject("Physics Box", location.position, Quaternion.identity, 0, null);
            PhotonNetwork.InstantiateSceneObject("Physics Box", location.position, Quaternion.identity, 0, null);
            PhotonNetwork.InstantiateSceneObject("Physics Box", location.position, Quaternion.identity, 0, null);
        PhotonNetwork.InstantiateSceneObject("Physics Box", location.position, Quaternion.identity, 0, null);
        PhotonNetwork.InstantiateSceneObject("Physics Box", location.position, Quaternion.identity, 0, null);
        PhotonNetwork.InstantiateSceneObject("Physics Box", location.position, Quaternion.identity, 0, null);
        PhotonNetwork.InstantiateSceneObject("Physics Box", location.position, Quaternion.identity, 0, null);
        PhotonNetwork.InstantiateSceneObject("Physics Box", location.position, Quaternion.identity, 0, null);
            PhotonNetwork.InstantiateSceneObject("Physics Box", location.position, Quaternion.identity, 0, null);
            PhotonNetwork.InstantiateSceneObject("Physics Box", location.position, Quaternion.identity, 0, null);
            PhotonNetwork.InstantiateSceneObject("Physics Box", location.position, Quaternion.identity, 0, null);
            PhotonNetwork.InstantiateSceneObject("Physics Box", location.position, Quaternion.identity, 0, null);
            PhotonNetwork.InstantiateSceneObject("Physics Box", location.position, Quaternion.identity, 0, null);
    }
}
