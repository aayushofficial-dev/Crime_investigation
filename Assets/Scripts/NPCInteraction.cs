// NPCInteraction.cs — attach to your FPS camera or player
using UnityEngine;
using UnityEngine.UI;
using TMPro; // use this if you're using TextMeshPro

public class NPCInteraction : MonoBehaviour
{
    public float interactRange = 3f;       // how close the player must be
    public GameObject dialogueUI;          // drag your UI Panel here
    public TMP_Text dialogueText;          // drag your Text element here

    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        RaycastHit hit;

        // shoot ray from centre of camera forward
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, interactRange))
        {
            if (hit.collider.CompareTag("NPC"))
            {
                NPCDialogue npc = hit.collider.GetComponent<NPCDialogue>();
                if (npc != null)
                {
                    dialogueUI.SetActive(true);
                    dialogueText.text = npc.dialogueText;
                    return;
                }
            }
        }

        // nothing valid hit — hide UI
        dialogueUI.SetActive(false);
    }
}