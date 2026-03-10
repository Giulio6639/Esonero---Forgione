using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [Header("Impostazioni Parallasse")]
    [Tooltip("0 = Fermo (Cielo), 1 = Si muove col giocatore (Primo piano)")]
    public float parallaxEffectMultiplier;

    private Transform cameraTransform;
    private Vector3 lastCameraPosition;

    void Start()
    {
        cameraTransform = Camera.main.transform;
        lastCameraPosition = cameraTransform.position;
    }

    void LateUpdate()
    {
        // Calcola di quanto si è mossa la telecamera dall'ultimo frame
        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;

        // Sposta lo sfondo in base al moltiplicatore
        transform.position += new Vector3(deltaMovement.x * parallaxEffectMultiplier, deltaMovement.y * parallaxEffectMultiplier, 0);

        lastCameraPosition = cameraTransform.position;
    }
}