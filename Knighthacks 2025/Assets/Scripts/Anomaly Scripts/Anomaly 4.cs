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
        int index = customer.spriteIndex / 2;
        if(index == 4)
        {
            index = 3; // Cap index to avoid out of bounds
        }
        if (characterChangeSprites != null && characterChangeSprites.Length > index)
        {
            customer.SpriteRenderer.sprite = characterChangeSprites[index];
        }
        else
        {
            Debug.LogWarning("Anomaly4 sprites not set correctly!");
        }
    }
}
