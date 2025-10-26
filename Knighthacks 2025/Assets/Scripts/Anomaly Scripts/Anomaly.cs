using UnityEngine;

public abstract class Anomaly
{
    public abstract bool CanSpawn();
    public abstract void ApplyToCustomer(Customer customer);
    public virtual void changeGameObjectSprite(GameObject kettle, SpriteRenderer normalKettle, Sprite hotKettle, float xpos, float ypos, float zpos)
    {
        Debug.Log("Override me!");
    }

}
