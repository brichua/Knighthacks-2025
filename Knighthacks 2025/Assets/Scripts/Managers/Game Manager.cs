using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    //This script will mainly be focused on making sure all of the other scripts can work
    //together 
    //public MonoBehaviour anomalyManager;
    //public MonoBehaviour customerManager;

    public GameObject startButton;
    public GameObject canvas;
    public GameObject taskButtons;
    public GameObject dayTextObject;
    public GameObject tvObject;

    public CanvasGroup blackImage;
    public SpriteRenderer background;
    public SpriteRenderer tv;
    public Sprite night;
    public Sprite tvOff;
    public Sprite tvOn;
    public Sprite tvStatus1;
    public Sprite tvStatus2;
    public Sprite tvStatus3;

    public int health;
    public int day;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

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
}