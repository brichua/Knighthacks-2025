using UnityEngine;

public class Anomaly7 : Anomaly
{
    //Handles the major change to room appearance
    public Sprite primaryRoom;
    public Sprite alternateRoom;

    public Anomaly7(Sprite primaryRoom, Sprite alternateRoom)
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
        //Do nothing lmao.
    }

    public void changeGameObjectSprite(SpriteRenderer obj)
    {
        obj.sprite = alternateRoom;
    }

    public void resetLobby(SpriteRenderer obj)
    {
        obj.sprite = primaryRoom;
    }
}
