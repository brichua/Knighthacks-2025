using UnityEngine;

public class Anomaly4 : Anomaly
{
    public Sprite[] characterChangeSprites;

    public Anomaly4(Sprite[] sprites)
    {
        characterChangeSprites = sprites;
    }
    public override bool CanSpawn()
    {
        // Add custom spawn logic here
        return true;
    }

    public override void ApplyToCustomer(Customer customer)
    {
        int index = 0;
        if(customer.spriteIndex == 4)
        {
            index = 2;
        }else if(customer.spriteIndex == 0)
        {
            index = 0;
        }
        else if (customer.spriteIndex == 2)
        {
            index = 1;
        }
        Debug.Log("Anomaly4 applying to customer with sprite index: " + customer.spriteIndex);
        if (characterChangeSprites != null && characterChangeSprites.Length > index)
        {
            customer.SpriteRenderer.sprite = characterChangeSprites[index];
            Debug.Log("Anomaly4 applied to customer with sprite index: " + customer.spriteIndex);
        }
        else
        {
            Debug.LogWarning("Anomaly4 sprites not set correctly!");
        }
    }
}
