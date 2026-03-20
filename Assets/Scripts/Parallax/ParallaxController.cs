using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

public class ParallaxController : MonoBehaviour
{
    private Camera cam;
    private Vector2 previousCameraPosition;
    private List<ParallaxLayer> layers = new List<ParallaxLayer>();

    void Start()
    {
        cam = Camera.main;
        previousCameraPosition = cam.transform.position;
        FindLayers();
        CinemachineCore.CameraUpdatedEvent.AddListener(OnCinemachineUpdated);
    }

    void OnDestroy()
    {
        CinemachineCore.CameraUpdatedEvent.RemoveListener(OnCinemachineUpdated);
    }

    private void OnCinemachineUpdated(CinemachineBrain brain)
    {
        Vector2 currentPosition = cam.transform.position;
        Vector2 delta = currentPosition - previousCameraPosition;

        if (delta.sqrMagnitude > 0f)
        {
            foreach (ParallaxLayer layer in layers)
                layer.MoveLayer(delta.x, delta.y);
        }

        previousCameraPosition = currentPosition;
    }

    public void FindLayers()
    {
        layers.Clear();
        foreach (Transform child in transform)
        {
            ParallaxLayer layer = child.GetComponent<ParallaxLayer>();
            if (layer != null) layers.Add(layer);
        }
    }
}