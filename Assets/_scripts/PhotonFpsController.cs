// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Spaceship.cs" company="Exit Games GmbH">
//   Part of: Asteroid Demo,
// </copyright>
// <summary>
//  Spaceship
// </summary>
// <author>developer@exitgames.com</author>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections;

using UnityEngine;

using Photon.Pun.UtilityScripts;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Photon.Pun.Demo.Asteroids
{
    public class PhotonFpsController : MonoBehaviour
    {
       
        public float moveSpeed = 100;
        public float jumpForce = 1000;
        private PhotonView photonView;

#pragma warning disable 0109
        private new Rigidbody rigidbody;
        private new Collider collider;
        private new Renderer renderer;
#pragma warning restore 0109

        private bool controllable = true;

        #region UNITY

        public void Awake()
        {
            photonView = GetComponent<PhotonView>();

            rigidbody = GetComponent<Rigidbody>();
            collider = GetComponent<Collider>();
            renderer = GetComponent<Renderer>();
        }

        public void Start()
        {
            foreach (Renderer r in GetComponentsInChildren<Renderer>())
            {
                r.material.color = AsteroidsGame.GetColor(photonView.Owner.GetPlayerNumber());
            }
        }

        public void Update()
        {
            if (!photonView.AmOwner || !controllable)
            {
                return;
            }

            // we don't want the master client to apply input to remote ships while the remote player is inactive
            if (this.photonView.CreatorActorNr != PhotonNetwork.LocalPlayer.ActorNumber)
            {
                return;
            }

            if (Input.GetButton("Jump"))
            {
               

                photonView.RPC("Fire", RpcTarget.AllViaServer, rigidbody.position, rigidbody.rotation);
            }

            
        }

        public void FixedUpdate()
        {

            //if (!photonView.IsMine)
            //{
            //    return;
            //}

            //if (!controllable)
            //{
            //    return;
            //}

            
            var z = Input.GetAxis("Vertical"); // up down
            var x = Input.GetAxis("Horizontal"); // left right
            var y = Input.GetKeyDown(KeyCode.Space) ? 1f : 0f;

            
            
            x *= moveSpeed;
            y *= jumpForce;
            z *= moveSpeed;

            rigidbody.AddForce(new Vector3(x,0,z) * Time.deltaTime);
           

           

            CheckExitScreen();
        }

        #endregion

        #region COROUTINES

        private IEnumerator WaitForRespawn()
        {
            yield return new WaitForSeconds(AsteroidsGame.PLAYER_RESPAWN_TIME);

            photonView.RPC("RespawnSpaceship", RpcTarget.AllViaServer);
        }

        #endregion

        #region PUN CALLBACKS

        [PunRPC]
        public void ExpelStudent()
        {
            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;

            collider.enabled = false;
            renderer.enabled = false;

            controllable = false;

           

            if (photonView.IsMine)
            {
                object lives;
                if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(AsteroidsGame.PLAYER_LIVES, out lives))
                {
                    PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable {{AsteroidsGame.PLAYER_LIVES, ((int) lives <= 1) ? 0 : ((int) lives - 1)}});

                    if (((int) lives) > 1)
                    {
                        StartCoroutine("WaitForRespawn");
                    }
                }
            }
        }

        [PunRPC]
        public void Fire(Vector3 position, Quaternion rotation, PhotonMessageInfo info)
        {
            float lag = (float) (PhotonNetwork.Time - info.SentServerTime);
            GameObject bullet;

            /** Use this if you want to fire one bullet at a time **/
            //bullet = Instantiate(BulletPrefab, position, Quaternion.identity) as GameObject;
            //bullet.GetComponent<Bullet>().InitializeBullet(photonView.Owner, (rotation * Vector3.forward), Mathf.Abs(lag));


        }

        [PunRPC]
        public void RespawnSpaceship()
        {
            collider.enabled = true;
            renderer.enabled = true;

            controllable = true;

        }
        
        #endregion

        private void CheckExitScreen()
        {
            if (Camera.main == null)
            {
                return;
            }
            
            if (Mathf.Abs(rigidbody.position.x) > (Camera.main.orthographicSize * Camera.main.aspect))
            {
                rigidbody.position = new Vector3(-Mathf.Sign(rigidbody.position.x) * Camera.main.orthographicSize * Camera.main.aspect, 0, rigidbody.position.z);
                rigidbody.position -= rigidbody.position.normalized * 0.1f; // offset a little bit to avoid looping back & forth between the 2 edges 
            }

            if (Mathf.Abs(rigidbody.position.z) > Camera.main.orthographicSize)
            {
                rigidbody.position = new Vector3(rigidbody.position.x, rigidbody.position.y, -Mathf.Sign(rigidbody.position.z) * Camera.main.orthographicSize);
                rigidbody.position -= rigidbody.position.normalized * 0.1f; // offset a little bit to avoid looping back & forth between the 2 edges 
            }
        }
    }
}