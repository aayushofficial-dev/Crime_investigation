using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NPCInteraction : MonoBehaviour
{
    public float interactRange = 10f;
    public GameObject dialogueUI;
    public TMP_Text dialogueText;

    private Camera cam;

    void Start()
    {
        cam = Camera.main;
        dialogueUI.SetActive(false);
    }

    void Update()
    {
        RaycastHit hit;
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);

        Debug.DrawRay(cam.transform.position, cam.transform.forward * interactRange, Color.red);

        if (Physics.Raycast(ray, out hit, interactRange))
        {
            Debug.Log("Hitting: " + hit.collider.gameObject.name + " | Tag: " + hit.collider.tag);

            if (hit.collider.CompareTag("NPC"))
            {
                NPCDialogue npc = hit.collider.GetComponent<NPCDialogue>();

                if (npc == null)
                    npc = hit.collider.GetComponentInParent<NPCDialogue>();

                if (npc != null)
                {
                    dialogueUI.SetActive(true);
                    dialogueText.text = npc.dialogueText;
                    return;
                }
            }
        }

        dialogueUI.SetActive(false);
    }
}