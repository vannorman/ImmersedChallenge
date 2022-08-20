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
    public class ThirdPersonController : MonoBehaviour
    {
        public float moveSpeed = 100;
        public float jumpForce = 1000;
        private PhotonView photonView;

#pragma warning disable 0109
       private float  jumpTimer = 0f;
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
            //if (!photonView.AmOwner || !controllable)
            //{
            //    return;
            //}

            //// we don't want the master client to apply input to remote ships while the remote player is inactive
            //if (this.photonView.CreatorActorNr != PhotonNetwork.LocalPlayer.ActorNumber)
            //{
            //    return;
            //}

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

            jumpTimer -= Time.deltaTime;

            var z = Input.GetAxis("Vertical"); // up down
            var x = Input.GetAxis("Horizontal"); // left right
            var y = 0f;
            if (Input.GetKey(KeyCode.Space) && jumpTimer < 0 && Grounded())
            {
                jumpTimer = 0.2f;
                y = jumpForce;
            }

            
            
            x *= moveSpeed;
            z *= moveSpeed;

            var moveDir = transform.right * x + transform.forward * z + Vector3.up * y;

            rigidbody.AddForce(moveDir * Time.deltaTime);
           

           

            
        }

        private bool Grounded()
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit))
            {
                var groundDist = 1.01f;

                if (hit.distance < groundDist)
                {
                    return true;
                }
            }
            return false;
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

        
    }
}