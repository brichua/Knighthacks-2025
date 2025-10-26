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
        int index = customer.spriteIndex / 2;
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
