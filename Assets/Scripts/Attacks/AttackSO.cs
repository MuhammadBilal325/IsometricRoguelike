using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu()]
public class AttackSO : ScriptableObject {
    public Transform attackPrefab;
    public Transform attackSpawnVFX;
    public Sprite attackSprite;
    public string attackName;
    public float attackDuration;
    public float attackCooldown;
    public float attackDelay;
    public int attackDamage;
}
