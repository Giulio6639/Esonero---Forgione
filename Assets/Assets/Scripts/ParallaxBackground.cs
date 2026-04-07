using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ParallaxBackground : MonoBehaviour
{
    private float length, startpos;
    public Transform cam; // La Main Camera

    [Tooltip("0 = Fermo rispetto alla camera, 1 = Si muove con la camera (Sfondo lontanissimo)")]
    public float parallaxFactor;

    void Start()
    {
        startpos = transform.position.x;
        // Ottiene automaticamente la larghezza del tuo sprite
        length = GetComponent<SpriteRenderer>().bounds.size.x;

        // Se non hai assegnato la camera nell'inspector, la trova da solo
        if (cam == null) cam = Camera.main.transform;
    }

    void LateUpdate()
    {
        // 'temp' calcola quanto ci siamo allontanati dal punto di origine rispetto al layer
        float temp = (cam.transform.position.x * (1 - parallaxFactor));

        // 'dist' calcola di quanto deve spostarsi il layer
        float dist = (cam.transform.position.x * parallaxFactor);

        // Applica il movimento al layer sull'asse X
        transform.position = new Vector3(startpos + dist, transform.position.y, transform.position.z);

        // --- LA MAGIA DELL'INFINITO ---
        // Se la camera supera il bordo destro dell'immagine, sposta l'origine in avanti
        if (temp > startpos + length)
        {
            startpos += length;
        }
        // Se la camera supera il bordo sinistro, sposta l'origine indietro
        else if (temp < startpos - length)
        {
            startpos -= length;
        }
    }
}