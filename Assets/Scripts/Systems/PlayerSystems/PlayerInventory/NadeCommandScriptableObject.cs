using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/NadeCommandScriptableObject", order = 1)]
public class NadeCommandScriptableObject : ScriptableObject{
    /// <summary>
    /// The one that we will fire
    /// </summary>
    [SerializeField] private GameObject nadePrefab;
    /// <summary>
    /// The one that will be picked up
    /// </summary>
    [SerializeField] private GameObject nadeObject;
    [SerializeField] private float throwForce;
    [SerializeField] private float throwDelay;
    [SerializeField] private int maxStack;
    [SerializeField] private int cost;

    public GameObject NadePrefab { get => nadePrefab; }
    public GameObject NadeObject { get => nadeObject; }
    public float ThrowForce { get => throwForce; }
    public float ThrowDelay { get => throwDelay; }
    public int MaxStack { get => maxStack; }
    public int Cost { get => cost; }
    
}
