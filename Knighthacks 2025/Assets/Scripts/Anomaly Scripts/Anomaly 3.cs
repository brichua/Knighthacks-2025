using UnityEngine;

public class Anomaly3 : Anomaly
{
    public Sprite[] characterChangeSprites;

    public Anomaly3(Sprite[] sprites)
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
        if (customer.spriteIndex == 4)
        {
            index = 2;
        }
        else if (customer.spriteIndex == 0)
        {
            index = 0;
        }
        else if (customer.spriteIndex == 2)
        {
            index = 1;
        }
        if (characterChangeSprites != null && characterChangeSprites.Length > index)
        {
            customer.SpriteRenderer.sprite = characterChangeSprites[index];
        }
        else
        {
            Debug.LogWarning("Anomaly3 sprites not set correctly!");
        }
    }
}
