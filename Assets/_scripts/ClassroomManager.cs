using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Pun.UtilityScripts;

namespace Photon.Pun.Demo.Asteroids
{
    public class ClassroomManager : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
            
        {
            Debug.Log("StarT");
            Hashtable props = new Hashtable
            {
                {AsteroidsGame.PLAYER_LOADED_LEVEL, true}
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
            var spacing = 3;
            Vector3 position = Vector3.zero + Vector3.right * PhotonNetwork.LocalPlayer.GetPlayerNumber() * spacing;
            var p = PhotonNetwork.Instantiate("ThirdPersonController", position, Quaternion.identity, 0);      // avoid this call on rejoin (ship was network instantiated before)
            Debug.Log("P?"+p);

        }

            // Update is called once per frame
        void Update()
        {
        
        }
    }

}
