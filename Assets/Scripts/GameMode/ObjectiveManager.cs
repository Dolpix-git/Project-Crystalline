using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveManager : MonoBehaviour{
    public static ObjectiveManager Instance { get; private set; }

    public event Action OnAttackerWin;
    public event Action OnDefenderWin;
    public event Action OnPlanted;
    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this);
        } else {
            Instance = this;
        }
    }

    public void AttackerWin() {
        OnAttackerWin?.Invoke();
    }
    public void DefenderWin() {
        OnDefenderWin?.Invoke();
    }
    public void Planted() {
        OnPlanted?.Invoke();
    }
}
