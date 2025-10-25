using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class TaskManager : MonoBehaviour
{
    public DialogueManager dialogueManager;
    public CustomerManager customerManager;
    public string snack;
    public string size;
    public string tea;
    public string flower;
    public bool anomaly;
    public bool tray;

    public bool snackChosen;
    public bool teaChosen;
    public bool flowerChosen;

    public GameObject snackObject;
    public GameObject teaObject;
    public GameObject flowerObject;
    public GameObject decorationObject;

    public SpriteRenderer background;
    public Sprite trayBackground;
    public Sprite noTrayBackground;

    public Sprite cookieSprite;
    public Sprite cakeSprite;
    public Sprite crackerSprite;
    public Sprite anomalyFlower;
    public Sprite regularFlower;

    public Sprite rose;
    public Sprite daisy;
    public Sprite bluebell;
    public Sprite smallGreen;
    public Sprite largeGreen;
    public Sprite smallOolong;
    public Sprite largeOolong;
    public Sprite smallBlack;
    public Sprite largeBlack;

    public Transform kettleStove;
    public Transform kettleTable;
    public Transform kettleCup;
    public Transform strainerSmall;
    public Transform strainerLarge;

    public Sprite roseTea;
    public Sprite daisyTea;
    public Sprite bluebellTea;
    public Sprite smallGreenTea;
    public Sprite largeGreenTea;
    public Sprite smallOolongTea;
    public Sprite largeOolongTea;
    public Sprite smallBlackTea;
    public Sprite largeBlackTea;
    public Sprite greenStrainer;
    public Sprite oolongStrainer;
    public Sprite blackStrainer;

    public Sprite smallCup;
    public Sprite largeCup;

    public GameObject cup;
    public GameObject strainer;
    public GameObject teaFlower;
    public GameObject kettle;

    public int step = 0;
    public bool kettleOnStove = false;
    public bool waterBoiled = false;

    public TextMeshProUGUI reminder;

    public GameObject front;
    public GameObject back;
    public bool lookedBack = false;

    public void takeOrder(GameObject customer1, Customer customer2)
    {
        Debug.Log("Taking order from customer.");
        tray = true;
        background.sprite = trayBackground;
        dialogueManager.PlayOrderSequenceForCustomer(customer2);
        customerManager.moveCustomerToWaitingLine(customer1);
        lookedBack = false;
    }

    public bool completeOrder(CustomerManager customerManager)
    {
        if(snackChosen && teaChosen && flowerChosen)
        {
            tray = false;
            background.sprite = noTrayBackground;
            for (int i = 0; i < customerManager.customers.Count; i++) 
            {
                if (size == customerManager.customers[i].order[0] && tea == customerManager.customers[i].order[1] &&
                    snack == customerManager.customers[i].order[2] && flower == customerManager.customers[i].order[3]) 
                {
                    if (Accusation.accuse(anomaly, customerManager.customers[i])) 
                    {
                        customerManager.customers[i].served = true;
                        //Call destroyCustomer from Customer Manager
                        customerManager.destroyCustomer(i);
                        return true;
                    }
                }
            }
            return false;
        }
        return false;
    }

    public void moveFront()
    {
        front.SetActive(true);
        back.SetActive(false);
        GameObject[] customers = GameObject.FindGameObjectsWithTag("Customer");
        foreach (GameObject customer in customers)
        {
            customer.SetActive(true);
        }
    }

    public void moveBack()
    {
        front.SetActive(false);
        back.SetActive(true);
        GameObject[] customers = GameObject.FindGameObjectsWithTag("Customer");
        foreach (GameObject customer in customers)
        {
            customer.SetActive(false);
        }
        lookedBack = true;
    }

    public void selectCookie()
    {
        if (!tray)
        {
            reminder.text = "You may be forgetting something...";
            return;
        }
        snack = "cookie";
        snackObject.SetActive(true);
        snackObject.GetComponent<SpriteRenderer>().sprite = cookieSprite;
        snackChosen = true;
    }

    public void selectCake()
    {
        if (!tray)
        {
            reminder.text = "You may be forgetting something...";
            return;
        }
        snack = "cake";
        snackObject.SetActive(true);
        snackObject.GetComponent<SpriteRenderer>().sprite = cakeSprite;
        snackChosen = true;
    }

    public void selectCracker()
    {
        if (!tray)
        {
            reminder.text = "You may be forgetting something...";
            return;
        }
        snack = "cracker";
        snackObject.SetActive(true);
        snackObject.GetComponent<SpriteRenderer>().sprite = crackerSprite;
        snackChosen = true;
    }

    public void selectRegularFlower()
    {
        if (!tray)
        {
            reminder.text = "You may be forgetting something...";
            return;
        }
        anomaly = false;
        decorationObject.SetActive(true);
        decorationObject.GetComponent<SpriteRenderer>().sprite = regularFlower;
        flowerChosen = true;
    }

    public void selectAnomalyFlower()
    {
        if (!tray)
        {
            reminder.text = "You may be forgetting something...";
            return;
        }
        anomaly = true;
        decorationObject.SetActive(true);
        decorationObject.GetComponent<SpriteRenderer>().sprite = anomalyFlower;
        flowerChosen = true;
    }

    public void selectSmallCup()
    {
        size = "small";
        cup.GetComponent<SpriteRenderer>().sprite = smallCup;
        cup.SetActive(true);
        step = 1;
    }

    public void selectLargeCup()
    {
        size = "large";
        cup.GetComponent<SpriteRenderer>().sprite = largeCup;
        cup.SetActive(true);
        step = 1;
    }

    public void trash()
    {
        step = 0;
        tea = null;
        cup.SetActive(false);
        strainer.SetActive(false);
        teaFlower.SetActive(false);
    }

    public void selectRose()
    {
        if (step != 1)
        {
            reminder.text = "You may be forgetting something...";
            return;
        }
        flower = "rose";
        step = 2;
        teaFlower.GetComponent<SpriteRenderer>().sprite = roseTea;
        teaFlower.SetActive(true);
    }

    public void selectDaisy()
    {
        if (step != 1)
        {
            reminder.text = "You may be forgetting something...";
            return;
        }
        flower = "daisy";
        step = 2;
        teaFlower.GetComponent<SpriteRenderer>().sprite = daisyTea;
        teaFlower.SetActive(true);
    }

    public void selectBluebell()
    {
        if (step != 1)
        {
            reminder.text = "You may be forgetting something...";
            return;
        }
        flower = "bluebell";
        step = 2;
        teaFlower.GetComponent<SpriteRenderer>().sprite = bluebellTea;
        teaFlower.SetActive(true);
    }

    public void selectGreenTea()
    {
        if (step != 2)
        {
            reminder.text = "You may be forgetting something...";
            return;
        }
        tea = "green";
        step = 3;
        strainer.GetComponent<SpriteRenderer>().sprite = greenStrainer;
        strainer.SetActive(true);
    }

    public void selectOolongTea()
    {
        if (step != 2)
        {
            reminder.text = "You may be forgetting something...";
            return;
        }
        tea = "oolong";
        step = 3;
        strainer.GetComponent<SpriteRenderer>().sprite = oolongStrainer;
        strainer.SetActive(true);
    }

    public void selectBlackTea()
    {
        if (step != 2)
        {
            reminder.text = "You may be forgetting something...";
            return;
        }
        tea = "oolong";
        step = 3;
        strainer.GetComponent<SpriteRenderer>().sprite = blackStrainer;
        strainer.SetActive(true);
    }

    public void boilWater()
    {
        kettle.transform.position = kettleStove.position;
        //wait for some time to simulate boiling
        //finished boiling animation
        waterBoiled = true;
    }

    public void pourTea()
    {
        if (step != 3 || !waterBoiled)
        {
            reminder.text = "You may be forgetting something...";
            return;
        }
        kettle.transform.position = kettleCup.position;
        //pouring animation
        kettle.transform.position = kettleTable.position;
        if (size == "small")
        {
            if (tea == "green")
            {
                teaFlower.GetComponent<SpriteRenderer>().sprite = smallGreenTea;
            }
            else if (tea == "oolong")
            {
                teaFlower.GetComponent<SpriteRenderer>().sprite = smallOolongTea;
            }
            else if (tea == "black")
            {
                teaFlower.GetComponent<SpriteRenderer>().sprite = smallBlackTea;
            }
        }
        else if (size == "large")
        {
            if (tea == "green")
            {
                teaFlower.GetComponent<SpriteRenderer>().sprite = largeGreenTea;
            }
            else if (tea == "oolong")
            {
                teaFlower.GetComponent<SpriteRenderer>().sprite = largeOolongTea;
            }
            else if (tea == "black")
            {
                teaFlower.GetComponent<SpriteRenderer>().sprite = largeBlackTea;
            }
        }
        strainer.SetActive(false);
        step = 4;
        //cup sparkle animation
    }

    public void finishTea()
    {
        if (step != 4)
        {
            reminder.text = "You may be forgetting something...";
            return;
        }
        switch (flower)
        {
            case "rose":
                flowerObject.GetComponent<SpriteRenderer>().sprite = rose;
                break;
            case "daisy":
                flowerObject.GetComponent<SpriteRenderer>().sprite = daisy;
                break;
            case "bluebell":
                flowerObject.GetComponent<SpriteRenderer>().sprite = bluebell;
                break;
        }
        switch (tea)
        {
            case "green":
                teaObject.GetComponent<SpriteRenderer>().sprite = (size == "small") ? smallGreen : largeGreen;
                break;
            case "oolong":
                teaObject.GetComponent<SpriteRenderer>().sprite = (size == "small") ? smallOolong : largeOolong;
                break;
            case "black":
                teaObject.GetComponent<SpriteRenderer>().sprite = (size == "small") ? smallBlack : largeBlack;
                break;
        }
        teaObject.SetActive(true);
        flowerObject.SetActive(true);
        teaChosen = true;
    }

    public void resetTasks()
    {
        snack = null;
        size = null;
        tea = null;
        flower = null;
        anomaly = false;
        tray = false;
        snackChosen = false;
        teaChosen = false;
        flowerChosen = false;
        snackObject.SetActive(false);
        teaObject.SetActive(false);
        flowerObject.SetActive(false);
        decorationObject.SetActive(false);
        step = 0;
        kettleOnStove = false;
        waterBoiled = false;
    }
}
