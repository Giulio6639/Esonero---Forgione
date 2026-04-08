using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ParallaxBackground : MonoBehaviour
{
    private float length, startpos;
    public Transform cam;

    [Tooltip("0 = Fermo rispetto alla camera, 1 = Si muove con la camera (Sfondo lontanissimo)")]
    public float parallaxFactor;

    void Start()
    {
        startpos = transform.position.x;
        length = GetComponent<SpriteRenderer>().bounds.size.x;

        if (cam == null) cam = Camera.main.transform;
    }

    void LateUpdate()
    {
        float temp = (cam.transform.position.x * (1 - parallaxFactor));

        float dist = (cam.transform.position.x * parallaxFactor);

        transform.position = new Vector3(startpos + dist, transform.position.y, transform.position.z);

        if (temp > startpos + length)
        {
            startpos += length;
        }
        else if (temp < startpos - length)
        {
            startpos -= length;
        }
    }
}