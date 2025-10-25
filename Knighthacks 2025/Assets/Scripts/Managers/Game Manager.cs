using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    //This script will mainly be focused on making sure all of the other scripts can work
    //together 
    public MonoBehaviour anomalyManager;
    public MonoBehaviour customerManager;

    public GameObject startButton;
    public GameObject taskButtons;

    public CanvasGroup blackImage;
    public Sprite night;
    public Sprite tvOff;
    public Sprite tvOn;
    public Sprite tvStatus1;
    public Sprite tvStatus2;
    public Sprite tvStatus3;

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
        startDay(2);
        taskButtons.SetActive(true);
    }

    public void startDay(int day)
    {

    }
}
