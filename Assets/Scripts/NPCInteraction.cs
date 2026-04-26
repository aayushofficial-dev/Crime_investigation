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
        cam = Camera.main;
        dialogueUI.SetActive(false);
        if (promptText) promptText.gameObject.SetActive(false);
    }

    void Update()
    {
        RaycastHit hit;
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);

        if (Physics.Raycast(ray, out hit, interactRange))
        {
            if (hit.collider.CompareTag("NPC"))
            {
                NPCDialogue npc = hit.collider.GetComponent<NPCDialogue>();
                if (npc == null)
                    npc = hit.collider.GetComponentInParent<NPCDialogue>();

                if (npc != null)
                {
                    if (promptText) promptText.gameObject.SetActive(true);

                    // NEW INPUT SYSTEM version of pressing E
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

        if (promptText) promptText.gameObject.SetActive(false);
        dialogueUI.SetActive(false);
        talking = false;
    }
}