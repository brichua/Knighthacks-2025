using System;
using UnityEngine;

public class Accusation : MonoBehaviour
{
    public bool accuse(string decorationFlower, Customer customer) 
    {
        // Both conditions for a correct accusation, both have same outcome
        if ((customer.isAnomaly && decorationFlower == "black") || (!customer.isAnomaly && decorationFlower == "white")) 
        {
            return true;
        }
        else 
        {
            // Failure leads to same outcome, have difference perhaps in actual real code and not a glorified function that could have just been put as a function in GameManager
            return false;
        }
    }
}
