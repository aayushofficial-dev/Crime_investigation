using UnityEngine;
using UnityEngine.SceneManagement;

public class AccidentCutsceneDirector : MonoBehaviour
{
    private enum CutsceneStep
    {
        CityView,
        AccidentView,
        CrashScreen,
        InvestigationReady
    }

    private static AccidentCutsceneDirector instance;

    private GameObject car;
    private GameObject victim;
    private Camera cutsceneCamera;
    private AudioSource audioSource;
    private CutsceneStep step = CutsceneStep.CityView;
    private float timer;
    private bool skidSoundPlayed;
    private bool crashSoundPlayed;
    private bool tireMarksCreated;
    private string message = "City View";

    private readonly Vector3 carStart = new Vector3(146.2f, 4.2f, -168f);
    private readonly Vector3 carEnd = new Vector3(146.2f, 4.2f, -148.6f);
    private readonly Vector3 victimStart = new Vector3(141.8f, 4.22f, -143.2f);
    private readonly Vector3 victimHit = new Vector3(147.4f, 4.22f, -140.2f);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoStart()
    {
        if (!SceneManager.GetActiveScene().name.ToLower().Contains("cityandcar"))
            return;

        if (instance != null)
            return;

        instance = new GameObject("Accident Cutscene Director").AddComponent<AccidentCutsceneDirector>();
    }

    private void Start()
    {
        car = GameObject.Find("Car");
        victim = GameObject.Find("Formal_With Mustache_Base");

        if (car == null || victim == null)
        {
            Debug.LogWarning("Accident cutscene needs objects named Car and Formal_With Mustache_Base.");
            return;
        }

        PrepareSceneObjects();
        CreateCamera();
        CreateAudio();
    }

    private void Update()
    {
        if (car == null || victim == null || cutsceneCamera == null)
            return;

        timer += Time.deltaTime;

        if (step == CutsceneStep.CityView)
            PlayCityView();
        else if (step == CutsceneStep.AccidentView)
            PlayAccidentView();
        else if (step == CutsceneStep.CrashScreen)
            PlayCrashScreen();
        else
            ShowInvestigationReady();
    }

    private void PrepareSceneObjects()
    {
        DisablePhysics(car);
        DisablePhysics(victim);

        CarAutoDrive carAutoDrive = car.GetComponent<CarAutoDrive>();
        if (carAutoDrive != null)
            carAutoDrive.enabled = false;

        Animator victimAnimator = victim.GetComponentInChildren<Animator>();
        if (victimAnimator != null)
            victimAnimator.enabled = false;

        car.transform.position = carStart;
        car.transform.rotation = Quaternion.identity;

        victim.transform.position = victimStart;
        victim.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
    }

    private void DisablePhysics(GameObject target)
    {
        Rigidbody rb = target.GetComponent<Rigidbody>();
        if (rb == null)
            return;

        rb.isKinematic = true;
        rb.useGravity = false;
    }

    private void CreateCamera()
    {
        Camera oldCamera = Camera.main;
        if (oldCamera != null)
            oldCamera.gameObject.SetActive(false);

        GameObject cameraObject = new GameObject("Accident Cutscene Camera");
        cutsceneCamera = cameraObject.AddComponent<Camera>();
        cameraObject.tag = "MainCamera";
    }

    private void CreateAudio()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.volume = 0.8f;
    }

    private void PlayCityView()
    {
        Vector3 widePosition = new Vector3(100f, 70f, -220f);
        Vector3 closePosition = new Vector3(132f, 18f, -162f);
        Vector3 wideLookAt = new Vector3(135f, 4f, -165f);
        Vector3 closeLookAt = new Vector3(146f, 4.5f, -144f);

        float progress = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(timer / 6f));
        SetCamera(
            Vector3.Lerp(widePosition, closePosition, progress),
            Vector3.Lerp(wideLookAt, closeLookAt, progress),
            Mathf.Lerp(58f, 40f, progress)
        );

        message = "Wide city view slowly focusing on the accident area";

        if (timer > 6f)
            NextStep(CutsceneStep.AccidentView);
    }

    private void PlayAccidentView()
    {
        SetCamera(new Vector3(134f, 12f, -156f), new Vector3(146.5f, 4.5f, -144f), 42f);
        message = "Accident scene view";

        float progress = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(timer / 3.2f));
        car.transform.position = Vector3.Lerp(carStart, carEnd, progress);
        victim.transform.position = Vector3.Lerp(victimStart, victimHit, progress);

        if (!skidSoundPlayed && timer > 1.8f)
        {
            skidSoundPlayed = true;
            PlayGeneratedSound(540f, 0.5f);
        }

        if (!tireMarksCreated && timer > 2f)
        {
            tireMarksCreated = true;
            CreateTireMarks();
        }

        if (timer > 3.2f)
        {
            victim.transform.position = victimHit;
            victim.transform.rotation = Quaternion.Euler(90f, 0f, 90f);
            NextStep(CutsceneStep.CrashScreen);
        }
    }

    private void PlayCrashScreen()
    {
        if (!crashSoundPlayed)
        {
            crashSoundPlayed = true;
            PlayCrashSound();
        }

        message = "Crash";

        if (timer > 3.5f)
            NextStep(CutsceneStep.InvestigationReady);
    }

    private void ShowInvestigationReady()
    {
        SetCamera(new Vector3(137.8f, 10.5f, -137.8f), new Vector3(146.5f, 4.8f, -143.4f), 48f);
        message = "Investigation Started";
    }

    private void NextStep(CutsceneStep nextStep)
    {
        step = nextStep;
        timer = 0f;
    }

    private void SetCamera(Vector3 position, Vector3 lookAt, float fieldOfView)
    {
        cutsceneCamera.transform.position = position;
        cutsceneCamera.transform.rotation = Quaternion.LookRotation(lookAt - position);
        cutsceneCamera.fieldOfView = fieldOfView;
    }

    private void CreateTireMarks()
    {
        CreateMark(new Vector3(145.6f, 4.25f, -153f));
        CreateMark(new Vector3(146.8f, 4.25f, -153f));
    }

    private void CreateMark(Vector3 position)
    {
        GameObject mark = GameObject.CreatePrimitive(PrimitiveType.Cube);
        mark.name = "Tire Mark";
        mark.transform.position = position;
        mark.transform.localScale = new Vector3(0.25f, 0.03f, 5f);
        mark.GetComponent<Renderer>().material.color = Color.black;
    }

    private void PlayGeneratedSound(float frequency, float duration)
    {
        if (audioSource == null)
            return;

        AudioClip clip = AudioClip.Create("Skid Sound", 11025, 1, 11025, false);
        float[] samples = new float[11025];

        for (int i = 0; i < samples.Length; i++)
        {
            float t = i / 11025f;
            float fade = 1f - Mathf.Clamp01(t / duration);
            samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * fade * 0.4f;
        }

        clip.SetData(samples, 0);
        audioSource.PlayOneShot(clip);
    }

    private void PlayCrashSound()
    {
        if (audioSource == null)
            return;

        AudioClip clip = AudioClip.Create("Crash Sound", 11025, 1, 11025, false);
        float[] samples = new float[11025];

        for (int i = 0; i < samples.Length; i++)
        {
            float t = i / 11025f;
            float fade = Mathf.Exp(-t * 8f);
            samples[i] = Random.Range(-1f, 1f) * fade;
        }

        clip.SetData(samples, 0);
        audioSource.PlayOneShot(clip);
    }

    private void OnGUI()
    {
        GUI.depth = -1000;

        if (step == CutsceneStep.CrashScreen)
        {
            DrawCrashScreen();
            return;
        }

        GUI.Box(new Rect(20f, 20f, 420f, 70f), message);
    }

    private void DrawCrashScreen()
    {
        GUI.color = new Color(0f, 0f, 0f, 1f);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);

        GUI.color = Color.white;
        DrawCrack(Screen.width * 0.5f, Screen.height * 0.5f, 260f, 3f);
        DrawCrack(Screen.width * 0.52f, Screen.height * 0.5f, 190f, -35f);
        DrawCrack(Screen.width * 0.48f, Screen.height * 0.48f, 200f, 42f);
        DrawCrack(Screen.width * 0.5f, Screen.height * 0.52f, 150f, 80f);

        GUI.Label(new Rect(Screen.width / 2f - 80f, Screen.height / 2f + 105f, 200f, 30f), "CRASH");
        GUI.color = Color.white;
    }

    private void DrawCrack(float x, float y, float length, float angle)
    {
        Matrix4x4 oldMatrix = GUI.matrix;
        GUIUtility.RotateAroundPivot(angle, new Vector2(x, y));
        GUI.DrawTexture(new Rect(x, y, length, 5f), Texture2D.whiteTexture);
        GUI.matrix = oldMatrix;
    }
}
