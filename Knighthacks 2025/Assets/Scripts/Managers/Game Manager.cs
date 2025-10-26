using System.Collections;
using System.Timers;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;

public class GameManager : MonoBehaviour
{
    //This script will mainly be focused on making sure all of the other scripts can work
    //together 
    public AnomalyManager anomalyManager;
    public CustomerManager customerManager;
    public TaskManager taskManager;

    public GameObject startButton;
    public GameObject canvas;
    public GameObject taskButtons;
    public GameObject dayTextObject;
    public GameObject tvObject;
    public GameObject endButton;

    public AudioSource bgMusic;
    public Volume mainVolume;
    public Volume scareVolume;
    private Vignette vignette;

    public SpriteRenderer scareSpriteRenderer;
    public Sprite[] scareSprites;
    public CanvasGroup fadeCanvasGroup;

    public float vignetteMaxIntensity = 0.6f;
    public float vignetteGrowTime = 1.2f;
    public float scareDuration = 0.6f;

    public CanvasGroup blackImage;
    public SpriteRenderer background;
    public SpriteRenderer tv;
    public Sprite night;
    public Sprite tvOff;
    public Sprite tvOn;
    public Sprite tvStatus1;
    public Sprite tvStatus2;
    public Sprite tvStatus3;
    public Sprite tvError;
    

    static Timer dayTimer;

    public int health;
    public int day;
    public bool dayEnd = false;

    // Prevent overlapping error sequences
    private bool isHandlingTvError = false;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (dayEnd) 
        {
            customerManager.stopSpawning = true;
            if (customerManager.customers.Count == 0) 
            { 
                EndDaySequence();
            }
        }
        if (taskManager.snackChosen && taskManager.teaChosen && taskManager.flowerChosen) 
        {
            if (!taskManager.completeOrder(customerManager))
            {
                // If an order failed, run the TV error sequence which handles the health decrement
                if (!isHandlingTvError)
                {
                    StartCoroutine(HandleOrderFailureSequence());
                }
            }
            else 
            {
                taskManager.resetTasks();
            }
        }
    }

    public int getHealth()
    {
        return health;
    }

    public void startGame()
    {
        startButton.SetActive(false);
        health = 3;
        day = 1;
        canvas.SetActive(true);
        startDay(day);
        bgMusic.Play();
        customerManager.StartCustomerSpawnTimer();
    }

    public void startDay(int day)
    {
        // Start the sequence asynchronously so fades and flicker happen over time.
        StartCoroutine(StartDaySequence(day));
    }

    IEnumerator StartDaySequence(int dayNumber)
    {

        // Safety checks
        if (blackImage == null || dayTextObject == null || tv == null)
        {
            Debug.LogWarning("GameManager: missing references required for StartDaySequence.");
            yield break;
        }

        const float fadeDuration = 0.8f;
        const float holdAfterFadeToBlack = 0.25f;
        const float tvToggleDelay = 0.18f;
        const int tvFlickerCycles = 3;

        // Ensure black overlay visible and clear to start
        blackImage.gameObject.SetActive(true);
        blackImage.alpha = 0f;

        // Fade to black
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            blackImage.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }
        blackImage.alpha = 1f;

        if (tvOff != null) background.sprite = tvOff;
        taskButtons.SetActive(true);

        // Activate day text and set its content
        dayTextObject.SetActive(true);
        // Set timer
        dayTimer = new Timer(300000); // 5 minutes per day
        dayTimer.Elapsed += (s, e) => { dayEnd = true; };
        dayTimer.Start();
        // Try TMP first, then legacy Text
        TMP_Text tmp = dayTextObject.GetComponentInChildren<TMP_Text>();
        if (tmp != null)
        {
            tmp.text = "Day " + dayNumber;
        }
        else
        {
            Text legacy = dayTextObject.GetComponentInChildren<Text>();
            if (legacy != null) legacy.text = "Day " + dayNumber;
        }

        yield return new WaitForSeconds(holdAfterFadeToBlack);

        // Fade from black
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            blackImage.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }
        blackImage.alpha = 0f;
        blackImage.gameObject.SetActive(false);

        // TV flicker: toggle between tvOff and tvOn tvFlickerCycles times
        for (int i = 0; i < tvFlickerCycles; i++)
        {
            if (tvOff != null) background.sprite = tvOff;
            yield return new WaitForSeconds(tvToggleDelay);
            if (tvOn != null) background.sprite = tvOn;
            yield return new WaitForSeconds(tvToggleDelay);
        }

        // Finalize: hide day text and set TV to stable status sprite
        dayTextObject.SetActive(false);
        tvObject.SetActive(true);
        if (tvStatus1 != null) tv.sprite = tvStatus1;

        customerManager.SpawnCustomer();
    }

    // Handles the sequence when an order fails:
    // - change TV renderer to tvError
    // - animate background flicker between tvOff and tvOn
    // - decrement health
    // - set background to tvOn and set tv sprite to status 2 (health==2) or status 3 (health==1)
    IEnumerator HandleOrderFailureSequence()
    {
        if (isHandlingTvError) yield break;
        isHandlingTvError = true;

        // Set TV to error sprite immediately (if available)
        if (tv != null && tvError != null)
        {
            tv.sprite = tvError;
        }

        // Flicker background
        const int flickerCycles = 4;
        const float flickerDelay = 0.15f;
        for (int i = 0; i < flickerCycles; i++)
        {
            if (tvOff != null) background.sprite = tvOff;
            yield return new WaitForSeconds(flickerDelay);
            if (tvOn != null) background.sprite = tvOn;
            yield return new WaitForSeconds(flickerDelay);
        }

        // Now apply the health penalty
        health--;

        // Reset tasks after applying the health change (preserves original behavior)
        taskManager.resetTasks();

        // Ensure background returns to tvOn and update tv status sprite based on remaining health
        if (tvOn != null) background.sprite = tvOn;

        if (tv != null)
        {
            if (health == 2 && tvStatus2 != null)
            {
                tv.sprite = tvStatus2;
            }
            else if (health == 1 && tvStatus3 != null)
            {
                tv.sprite = tvStatus3;
            }else if(health <= 0)
            {
                StartCoroutine(ScareAndResetSequence());
            }
            else if (tvStatus1 != null)
            {
                // default when health is 3 or other
                tv.sprite = tvStatus1;
            }
        }

        isHandlingTvError = false;
    }

    public void EndDaySequence()
    {
        dayEnd = false;
        endButton.SetActive(true);
        day++;
        background.sprite = night;
    }

    public IEnumerator ScareAndResetSequence()
    {
        Debug.Log("Starting scare sequence...");

        // --- PREPARE ---
        if (mainVolume == null || scareVolume == null)
        {
            Debug.LogWarning("Missing volume references!");
            yield break;
        }

        if (scareVolume.profile.TryGet(out Vignette vignetteEffect))
        {
            vignette = vignetteEffect;
        }
        else if (mainVolume.profile.TryGet(out Vignette fallbackVignette))
        {
            vignette = fallbackVignette;
        }
        else
        {
            Debug.LogWarning("No vignette found in volumes!");
            yield break;
        }

        // Initial setup
        scareVolume.weight = 0f;
        vignette.intensity.Override(0f);
        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.gameObject.SetActive(true);
        canvas.SetActive(true);

        // --- 1. GROW VIGNETTE + BLEND IN SECOND VOLUME ---
        float elapsed = 0f;
        while (elapsed < vignetteGrowTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / vignetteGrowTime;

            vignette.intensity.Override(Mathf.Lerp(0f, vignetteMaxIntensity, t));
            scareVolume.weight = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }
        vignette.intensity.Override(vignetteMaxIntensity);
        scareVolume.weight = 1f;

        // --- 2. SPAWN RANDOM SCARE SPRITE ---
        if (scareSprites != null && scareSprites.Length > 0 && scareSpriteRenderer != null)
        {
            scareSpriteRenderer.sprite = scareSprites[Random.Range(0, scareSprites.Length)];
            scareSpriteRenderer.gameObject.SetActive(true);
            scareSpriteRenderer.transform.localScale = Vector3.one * 0.8f;

            // Bounce effect
            float bounceTime = 0.1f;
            Vector3 originalScale = scareSpriteRenderer.transform.localScale;
            Vector3 targetScale = originalScale * 1.3f;

            // Scale up fast
            float t = 0f;
            while (t < bounceTime)
            {
                t += Time.deltaTime;
                scareSpriteRenderer.transform.localScale = Vector3.Lerp(originalScale, targetScale, t / bounceTime);
                yield return null;
            }

            // Scale back down quickly
            t = 0f;
            while (t < bounceTime)
            {
                t += Time.deltaTime;
                scareSpriteRenderer.transform.localScale = Vector3.Lerp(targetScale, originalScale, t / bounceTime);
                yield return null;
            }

            yield return new WaitForSeconds(scareDuration);
            scareSpriteRenderer.gameObject.SetActive(false);
        }

        // --- 3. FADE TO BLACK WITH FLICKER ---
        const float fadeDuration = 1.2f;
        elapsed = 0f;

        // Flicker parameters
        float flickerChance = 0.25f; // 25% chance per frame to toggle
        float flickerCooldown = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);

            // Handle random flicker
            flickerCooldown -= Time.deltaTime;
            if (flickerCooldown <= 0f && Random.value < flickerChance)
            {
                canvas.SetActive(!canvas.activeSelf);
                flickerCooldown = Random.Range(0.05f, 0.15f); // small delay before next flicker check
            }

            yield return null;
        }

        // Ensure full black and canvas is back on
        fadeCanvasGroup.alpha = 1f;
        canvas.SetActive(true);

        // --- 4. RESET GAME STATE ---
        Debug.Log("Resetting game...");
        health = 3;
        day = 1;
        taskManager.resetTasks();
        customerManager.customers.Clear();
        customerManager.stopSpawning = true;
        canvas.SetActive(false);
        scareVolume.weight = 0f;
        vignette.intensity.Override(0f);

        yield return new WaitForSeconds(0.5f);

        // --- 5. FADE FROM BLACK ---
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }
        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.gameObject.SetActive(false);

        // --- 6. ENABLE START BUTTON AGAIN ---
        startButton.SetActive(true);

        Debug.Log("Scare and reset complete.");
    }


}