using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/NadeCommandScriptableObject", order = 1)]
public class NadeCommandScriptableObject : ScriptableObject{
    [SerializeField] private GameObject nadePrefab;
    [SerializeField] private float throwForce;
    [SerializeField] private float throwDelay;
    [SerializeField] private int maxStack;
    [SerializeField] private int cost;

    public GameObject NadePrefab { get => nadePrefab; }
    public float ThrowForce { get => throwForce; }
    public float ThrowDelay { get => throwDelay; }
    public int MaxStack { get => maxStack; }
    public int Cost { get => cost; }
}
