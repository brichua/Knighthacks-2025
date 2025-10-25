using JetBrains.Annotations;
using UnityEngine;

public class AnomalySpawnManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public MonoBehaviour[] possibleAnomalies;

    void Start()
    {
        //Do nothing lmao
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //This function will roll for if a customer rolls into an anomaly or not
    //If it succeeds, return true. Else, return false
    private bool rollForAnomaly(int x)
    {
        int roll = UnityEngine.Random.Range(1, x);
        if(roll <= 1)
        {
            //Succeeds, generate random Anomaly

            return true;
        }
        //Fails
        return false;
    }
}
