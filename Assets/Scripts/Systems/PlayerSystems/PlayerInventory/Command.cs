using UnityEngine;

public class Command{
    protected int amount;
    protected int inventoryIndex;
    public virtual int AttemptToAddAmount(int amount) { return amount; }
    public virtual void Activate(Vector3 lookVector) { }
    public virtual void Drop(Vector3 lookVector) { }
}