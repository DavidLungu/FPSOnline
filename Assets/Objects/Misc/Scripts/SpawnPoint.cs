using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SpawnPoint : MonoBehaviour
{
    public bool isActive;

    [SerializeField] private float detectRadius, detectAngle;
    [SerializeField] LayerMask targetMask, obstructionMask;

    public Transform tempSkin;

    private void Start() {
        tempSkin.gameObject.SetActive(false);
        isActive = true;
    }

    private void Update() {
        Collider[] rangeScans = Physics.OverlapSphere(transform.position, detectRadius, targetMask);
        
        if (rangeScans.Length != 0) {
            Debug.Log(rangeScans[0].name);

            Transform target = rangeScans[0].transform;
            Vector3 directionToTarget = (target.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, directionToTarget) < detectAngle / 2) {
                float distanceToTarget = Vector3.Distance(transform.position, target.position);
                
                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstructionMask)) {
                    isActive = false;
                } 
                else {
                    isActive = true;
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("Player")) {
            isActive = false;
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.gameObject.CompareTag("Player")) {
            isActive = true;
        }
    }
}
