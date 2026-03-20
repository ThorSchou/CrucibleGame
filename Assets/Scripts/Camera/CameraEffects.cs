using UnityEngine;
using Unity.Cinemachine;

public class CameraEffects : MonoBehaviour
{
    public Vector3 cameraWorldSize;
    public float screenYDefault;
    public float screenYTalking;

    private CinemachineCamera virtualCamera;
    private CinemachineBasicMultiChannelPerlin multiChannelPerlin;

    [Range(0, 10)]
    [System.NonSerialized] public float shakeLength = 10;

    void Start()
    {
        virtualCamera = GetComponent<CinemachineCamera>();
        multiChannelPerlin = GetComponent<CinemachineBasicMultiChannelPerlin>();

        NewPlayer.Instance.cameraEffects = this;
    }

    void Update()
    {
        if (multiChannelPerlin != null)
        {
            multiChannelPerlin.FrequencyGain +=
                (0 - multiChannelPerlin.FrequencyGain) * Time.deltaTime * (10 - shakeLength);

            if (multiChannelPerlin.FrequencyGain < 0.1f)
                multiChannelPerlin.FrequencyGain = 0f;
        }
    }

    public void Shake(float shake, float length)
    {
        shakeLength = length;
        if (multiChannelPerlin != null)
        {
            multiChannelPerlin.AmplitudeGain = 0.5f;
            multiChannelPerlin.FrequencyGain = shake;
        }
    }
}