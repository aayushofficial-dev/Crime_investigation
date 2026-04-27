using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class NPCInteraction : MonoBehaviour
{
    public float interactRange = 10f;
    public GameObject dialogueUI;
    public TMP_Text dialogueText;
    public TMP_Text promptText;

    private Camera cam;
    private bool talking = false;

    void Start()
    {
        // Safe camera finding
        cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("No MainCamera found! Tag your camera as MainCamera.");
            return;
        }

        if (dialogueUI != null)
            dialogueUI.SetActive(false);

        if (promptText != null)
            promptText.gameObject.SetActive(false);
    }

    void Update()
    {
        // Re-find camera if it somehow becomes null (scene change safety)
        if (cam == null)
        {
            cam = Camera.main;
            return;
        }

        // Null check for UI
        if (dialogueUI == null || dialogueText == null)
        {
            Debug.LogError("dialogueUI or dialogueText not assigned in Inspector!");
            return;
        }

        RaycastHit hit;
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);

        if (Physics.Raycast(ray, out hit, interactRange))
        {
            Debug.Log("Hit: " + hit.collider.name + " Tag: " + hit.collider.tag);

            if (hit.collider.CompareTag("NPC"))
            {
                NPCDialogue npc = hit.collider.GetComponent<NPCDialogue>();
                if (npc == null)
                    npc = hit.collider.GetComponentInParent<NPCDialogue>();

                if (npc != null)
                {
                    if (promptText != null)
                        promptText.gameObject.SetActive(true);

                    if (Keyboard.current.eKey.wasPressedThisFrame)
                    {
                        talking = !talking;
                        dialogueUI.SetActive(talking);
                        if (talking)
                            dialogueText.text = npc.dialogueText;
                    }
                    return;
                }
            }
        }

        // Nothing hit or not NPC
        if (promptText != null)
            promptText.gameObject.SetActive(false);

        if (dialogueUI != null)
            dialogueUI.SetActive(false);

        talking = false;
    }
}