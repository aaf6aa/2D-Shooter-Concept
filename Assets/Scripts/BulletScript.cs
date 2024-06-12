using UnityEngine;

public class BulletScript : MonoBehaviour
{
    public enum BulletType
    {
        Normal,
        Big,
        Explosive,
        Fire,
        Time_Decrease,
        Knockback,
        Forcefield,
        Magnetic
    }

    public GameObject particleEffect;
    public PlayerController player;
    public BulletType type;
    public float damage = 50f; //approx 10 shots to kill
    public float forceStrength = 0.0f;

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.tag == "Bullet")
        {
            player.soundManager.PlayEffect("click");

            Destroy(col.gameObject);
        }
        else if (col.gameObject.tag == "Character")
        {
            var player = col.gameObject.GetComponent<PlayerController>();

            if (player == this.player)
            {
                return;
            }

            this.player.soundManager.PlayEffect("hit");
            if (type == BulletType.Fire)
            {
                this.player.soundManager.PlayEffect("fire");
                var effect = Instantiate(particleEffect, transform.position, Quaternion.identity, player.transform);

                col.gameObject.AddComponent<FireEffect>();
            }
            else if (type == BulletType.Time_Decrease)
            {
                GameObject.Find("GameManager").GetComponent<GameManager>().timer -= 10.00f;
            }
            else
            {
                // all other types do standard damage
                player.DealDamage(damage);
            }

            if (type == BulletType.Knockback)
            {
                if ((transform.position.x - col.collider.transform.position.x) < 0)
                {
                    col.gameObject.GetComponent<Rigidbody2D>().AddForce(Vector2.right * 100, ForceMode2D.Impulse);
                }
                else
                {
                    col.gameObject.GetComponent<Rigidbody2D>().AddForce(Vector2.left * 100, ForceMode2D.Impulse);
                }
            }
        }

        // explodes regardless of what it collided with
        if (type == BulletType.Explosive)
        {
            player.soundManager.PlayEffect("explosion");

            var effect = Instantiate(particleEffect, transform.position, Quaternion.identity);

            // deal AoE
            foreach (var character in GameObject.FindGameObjectsWithTag("Character"))
            {
                if (character.GetComponent<PlayerController>() != player && Vector2.Distance(transform.position, character.transform.position) < 1.5f)
                {
                    character.GetComponent<PlayerController>().DealDamage(damage);
                }
            }
        }

        transform.position = new Vector2(1000.0f, 1000.0f); // in case it doesnt get destroyed

        Destroy(this);
        Destroy(this.gameObject);
    }


    void FixedUpdate()
    {
        if (type == BulletType.Magnetic || type == BulletType.Forcefield)
        {
            foreach (var character in GameObject.FindGameObjectsWithTag("Character"))
            {
                // ignore the bullet's shooter
                if (character.GetComponent<PlayerController>() != player && Vector2.Distance(transform.position, character.transform.position) < 7.50f)
                {
                    var delta = forceStrength * Time.deltaTime;
                    if (type == BulletType.Forcefield)
                    {
                        delta *= -1; // repulsion instead of attraction
                    }
                    character.transform.position = Vector2.MoveTowards(character.transform.position, transform.position, delta);
                }
            }
        }
    }
}
