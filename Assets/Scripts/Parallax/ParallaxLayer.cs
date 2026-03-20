using UnityEngine;

// Attach to each background layer. Set parallaxAmount in the Inspector.
// Must be a child of the ParallaxController GameObject.
public class ParallaxLayer : MonoBehaviour
{
    [Range(-1f, 1f)]
    [Tooltip("1 = moves with camera (close), -1 = barely moves (far away)")]
    public float parallaxAmount = 0.5f;

    [Tooltip("Scales the overall speed of all layers. Match this across all layers.")]
    [SerializeField] private float moveScale = 40f;

    public void MoveLayer(float deltaX, float deltaY)
    {
        Vector3 newPosition = transform.localPosition;
        newPosition.x -= deltaX * (-parallaxAmount * moveScale);
        newPosition.y -= deltaY * (-parallaxAmount * moveScale);
        transform.localPosition = newPosition;
    }
}