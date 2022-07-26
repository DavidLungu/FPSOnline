using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuCamera : MonoBehaviour
{
    [SerializeField] private float speed;

    [SerializeField] private Material[] skyboxMaterials;
    [SerializeField] private Color[] ambientColours;

    private int skyboxTheme;

    private void Awake()
    {
        skyboxTheme = Random.Range(0, skyboxMaterials.Length-1);

        RenderSettings.skybox = skyboxMaterials[skyboxTheme];
        RenderSettings.ambientEquatorColor = ambientColours[skyboxTheme];
    }

    private void Update()
    {
        transform.Rotate(Vector3.up * (speed * Time.deltaTime));
    }
}
