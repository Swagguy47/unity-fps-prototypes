using UnityEngine;
using UnityEngine.Events;

public class Hitbox : MonoBehaviour
{
    [SerializeField] private UnityEvent Damaged; //to be used to call any event in another script
    [HideInInspector] public float DamageTaken; //ammount of hp lost to last impact

    public void Hit(float Damage)
    {
        DamageTaken = Damage;
        Damaged.Invoke();
    }
}
