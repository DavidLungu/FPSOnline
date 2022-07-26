using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BulletHole : MonoBehaviour
{   
    private PhotonView pv;

    private void Start() {
        pv = GetComponent<PhotonView>();    
    }

    private void Update() {
        if (!pv.IsMine) return;
        StartCoroutine(DestroyBulletHole(this.gameObject, 5f));
    }

    public IEnumerator DestroyBulletHole(GameObject _bulletHole, float delay) {
        yield return new WaitForSeconds(delay);
        PhotonNetwork.Destroy(_bulletHole);
    }
}
