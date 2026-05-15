using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class AccidentSceneDirector : MonoBehaviour
{
    [Header("─── Scene Objects ───")]
    public CarAccidentController    car;
    public CharacterHitSimple       character;
    public CinematicCameraDirector  cinematicDirector;

    [Header("─── UI ───")]
    public CanvasGroup  buttonGroup;        // CanvasGroup on the "Click Here" button panel
    public Button       clickHereButton;    // the actual Button component
    public string       nextSceneName = "InvestigationScene"; // scene to load

    [Header("─── Screen Fade Overlay ───")]
    public Image fadeOverlay;               // full-screen dark Image (alpha starts 0)

    [Header("─── Timing ───")]
    public float introDelay       = 1.5f;  // pause before car starts moving
    public float buttonFadeInTime = 1.2f;  // how long the button takes to appear

    [Header("─── Optional Atmosphere ───")]
    public AudioClip   ambientClip;         // city/wind loop
    public AudioSource ambientSource;
    public Light[]     streetLights;        // flicker on impact

    // ── private ──────────────────────────────────────────────────────────────
    private bool _impactFired = false;

    void Start()
    {
        // Hide button at start
        if (buttonGroup != null)
        {
            buttonGroup.alpha          = 0f;
            buttonGroup.interactable   = false;
            buttonGroup.blocksRaycasts = false;
        }

        // Hide fade overlay
        if (fadeOverlay != null)
        {
            var c = fadeOverlay.color; c.a = 0f; fadeOverlay.color = c;
        }

        // Wire button click
        if (clickHereButton != null)
            clickHereButton.onClick.AddListener(OnClickHere);

        // Ambient sound
        if (ambientSource != null && ambientClip != null)
        {
            ambientSource.clip = ambientClip;
            ambientSource.loop = true;
            ambientSource.Play();
        }

        // Begin
        StartCoroutine(StartScene());
    }

    IEnumerator StartScene()
    {
        yield return new WaitForSeconds(introDelay);
        // Car starts driving on its own (CarAccidentController.Start already called)
        // We just wait — car calls OnCarImpact() when it hits
    }

    // ── Called by CarAccidentController when car hits character ──────────────
    public void OnCarImpact(Vector3 carForward)
    {
        if (_impactFired) return;
        _impactFired = true;

        // Throw character
        character.TriggerHit(carForward);

        // Camera shake immediately
        StartCoroutine(ShakeCamera(0.4f, 0.2f));

        // Flicker street lights
        foreach (var l in streetLights)
            StartCoroutine(FlickerLight(l));

        // Start cinematic sequence
        cinematicDirector.BeginCinematic();
    }

    // ── Called by CinematicCameraDirector when all shots are done ────────────
    public void OnCinematicComplete()
    {
        StartCoroutine(ShowButtonSequence());
    }

    IEnumerator ShowButtonSequence()
    {
        // Slight dark fade over scene
        yield return StartCoroutine(FadeOverlay(0f, 0.45f, 0.8f));

        // Fade in button
        yield return StartCoroutine(FadeCanvasGroup(buttonGroup, 0f, 1f, buttonFadeInTime));

        if (buttonGroup != null)
        {
            buttonGroup.interactable   = true;
            buttonGroup.blocksRaycasts = true;
        }
    }

    void OnClickHere()
    {
        StartCoroutine(LoadNextScene());
    }

    IEnumerator LoadNextScene()
    {
        // Fade to black then load
        yield return StartCoroutine(FadeOverlay(
            fadeOverlay != null ? fadeOverlay.color.a : 0f,
            1f, 0.8f));

        SceneManager.LoadScene(nextSceneName);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    IEnumerator FadeOverlay(float from, float to, float duration)
    {
        if (fadeOverlay == null) yield break;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Lerp(from, to, t / duration);
            var c = fadeOverlay.color; c.a = a; fadeOverlay.color = c;
            yield return null;
        }
    }

    IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
    {
        if (cg == null) yield break;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        cg.alpha = to;
    }

    IEnumerator ShakeCamera(float duration, float magnitude)
    {
        var cam = Camera.main;
        if (cam == null) yield break;
        Vector3 origin = cam.transform.localPosition;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            cam.transform.localPosition = origin + (Vector3)Random.insideUnitCircle * magnitude;
            yield return null;
        }
        cam.transform.localPosition = origin;
    }

    IEnumerator FlickerLight(Light l)
    {
        if (l == null) yield break;
        float original = l.intensity;
        for (int i = 0; i < 5; i++)
        {
            l.intensity = (i % 2 == 0) ? 0f : original;
            yield return new WaitForSecondsRealtime(Random.Range(0.06f, 0.14f));
        }
        l.intensity = 0f; // stays off after crash
    }
}