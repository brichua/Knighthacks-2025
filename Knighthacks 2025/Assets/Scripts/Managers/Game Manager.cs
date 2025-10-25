using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Timers;

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
}