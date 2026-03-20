using UnityEngine;

public class RotateObject : MonoBehaviour
{
    [SerializeField] private float speed = 1f;

    void Update()
    {
        transform.Rotate(Vector3.back * Time.deltaTime * speed);
    }
}