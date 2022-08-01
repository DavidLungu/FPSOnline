using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BulletImpact : MonoBehaviour
{   
    private ParticleSystem particleSystem;

    private PhotonView pv;

    private void Start() {
        pv = GetComponent<PhotonView>();    
        particleSystem = GetComponentInChildren<ParticleSystem>();
    }

    private void Update() {
        if (!pv.IsMine) return;
        StartCoroutine(DestroyBulletHole(this.gameObject, particleSystem.main.startLifetime.constant));
    }

    public IEnumerator DestroyBulletHole(GameObject _bulletHole, float delay) {
        yield return new WaitForSeconds(delay);
        PhotonNetwork.Destroy(_bulletHole);
    }
}
