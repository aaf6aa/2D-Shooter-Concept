using UnityEngine;

public class FireEffect : MonoBehaviour
{
    public float duration = 5.0f;
    public float damage = 20.0f;


    // Update is called once per frame
    void Update()
    {
        gameObject.GetComponent<PlayerController>().DealDamage(damage * Time.deltaTime, true);

        duration -= Time.deltaTime;
        if (duration <= 0.0f)
        {
            // destroy effects
            Destroy(this);
        }
    }
}
