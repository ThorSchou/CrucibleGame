using System.Collections;
using UnityEngine;

public class ArenaPlatform : MonoBehaviour
{
    [Header("Vertical Bob")]
    [SerializeField] private float bobHeight = 1.0f;
    [SerializeField] private float bobSpeed = 0.5f;

    [Header("Horizontal Drift")]
    [SerializeField] private float driftDistance = 2.0f;
    [SerializeField] private float driftSpeed = 0.3f;

    [Header("Disappear / Reappear")]
    [SerializeField] private bool canDisappear = false;
    [SerializeField] private float visibleTime = 6f;
    [SerializeField] private float hiddenTime = 3f;
    [SerializeField] private float fadeWarning = 1.5f;

    private Vector3 startPos;
    private float vertOffset;
    private float horizOffset;
    private SpriteRenderer sr;
    private Collider2D col;
    private Color baseColor;

    void Start()
    {
        startPos = transform.position;
        vertOffset = Random.Range(0f, Mathf.PI * 2f);
        horizOffset = Random.Range(0f, Mathf.PI * 2f);
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        baseColor = sr.color;

        if (canDisappear)
            StartCoroutine(DisappearCycle());
    }

    void Update()
    {
        float y = startPos.y + Mathf.Sin(Time.time * bobSpeed + vertOffset) * bobHeight;
        float x = startPos.x + Mathf.Sin(Time.time * driftSpeed + horizOffset) * driftDistance;
        transform.position = new Vector3(x, y, startPos.z);
    }

    private IEnumerator DisappearCycle()
    {
        yield return new WaitForSeconds(Random.Range(0f, visibleTime));

        while (true)
        {
            Show(true);
            yield return new WaitForSeconds(visibleTime - fadeWarning);

            float elapsed = 0f;
            bool blink = true;
            while (elapsed < fadeWarning)
            {
                blink = !blink;
                sr.color = new Color(baseColor.r, baseColor.g, baseColor.b, blink ? 1f : 0.25f);
                yield return new WaitForSeconds(0.12f);
                elapsed += 0.12f;
            }

            Show(false);
            yield return new WaitForSeconds(hiddenTime);
        }
    }

    private void Show(bool visible)
    {
        if (sr != null)
            sr.color = new Color(baseColor.r, baseColor.g, baseColor.b, visible ? 1f : 0f);
        if (col != null)
            col.enabled = visible;
    }
}
