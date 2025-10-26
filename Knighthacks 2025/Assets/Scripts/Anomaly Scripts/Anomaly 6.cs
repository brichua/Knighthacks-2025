using UnityEngine;

public class Anomaly6 : Anomaly
{
    public Sprite primaryRoom;
    public Sprite alternateRoom;
    public Anomaly6(Sprite primaryRoom, Sprite alternateRoom)
    {
        this.primaryRoom = primaryRoom;
        this.alternateRoom = alternateRoom;
    }
    public override bool CanSpawn()
    {
        return true;
    }
    public override void ApplyToCustomer(Customer customer)
    {
        throw new System.NotImplementedException();
    }

    // Set temp sprite so that we can swap sprites
    public void changeGameObjectSprite(SpriteRenderer obj)
    {
        obj.sprite = alternateRoom;
    }

    public void resetLobby(SpriteRenderer obj)
    {
        obj.sprite = primaryRoom;
    }
}
