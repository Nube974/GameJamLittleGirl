using UnityEngine;
using Unity.Cinemachine;

public class CameraSwitchTrigger : MonoBehaviour
{
    public CinemachineCamera vcamFollow;
    public CinemachineCamera vcamPanorama;

    public int followPriority = 10;
    public int panoramaPriority = 20;
    public bool revertOnExit = true;

    public Behaviour playerInputToDisable; // optionnel

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (playerInputToDisable) playerInputToDisable.enabled = false;

        if (vcamPanorama) vcamPanorama.Priority = panoramaPriority;
        if (vcamFollow) vcamFollow.Priority = followPriority;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || !revertOnExit) return;
        if (playerInputToDisable) playerInputToDisable.enabled = true;

        if (vcamFollow) vcamFollow.Priority = panoramaPriority;
        if (vcamPanorama) vcamPanorama.Priority = followPriority;
    }
}
