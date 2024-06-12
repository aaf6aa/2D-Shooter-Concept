using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public string playerName = "Player";
    public bool isFrozen = false;

    public GameManager gameManager;
    public SoundManager soundManager;

    public GameObject[] bullets;
    int bulletIndex = 0;
    private string[] bulletTexts = new string[]
    {
        "Unknown", "Unknown", "Unknown", "Unknown", "Unknown", "Unknown"
    };

    public float health = 600.0f;

    public bool isAi = false;
    public float speed;
    public float jumpForce;
    public RectTransform healthBar;
    public GameObject damageText;

    public GameObject revolver;
    private GameObject revolverRingOverlay;
    private GameObject revolverCylinder;
    private Text revolverText;

    private GameObject canvas;
    private SpriteRenderer sprite;
    private RectTransform rectTransform;
    private Rigidbody2D rigidbody2d;
    private Vector2 objectSize;
    private Vector2 screenBounds;
    private GameObject otherPlayer;

    void Move(Vector2 movementVector)
    {
        movementVector = movementVector * speed * Time.deltaTime;

        // horizontal movement not physics based
        var newX = rectTransform.position.x + movementVector.x;
        rectTransform.position = new Vector3(
            newX,
            rectTransform.position.y,
            0f);

        // vertical movement (jumping) will be smooth and affected by gravity
        if (movementVector.y > 0 && rectTransform.position.y <= -1.75)
        {
            soundManager.PlayEffect("jump");
            rigidbody2d.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
        }

        // clamp the position to the screen bounds
        rectTransform.position = new Vector3(
            Mathf.Clamp(rectTransform.position.x, -screenBounds.x + objectSize.x, screenBounds.x - objectSize.x),
            rectTransform.position.y,
            0f);

        // flip sprite to face the enemy
        if (otherPlayer != null)
        {
            sprite.flipX = (rectTransform.position.x - otherPlayer.transform.position.x > 0);
        }
    }

    float bulletInterval = 1.25f;
    float bulletDelta = 0.5f;
    float bulletForce = 0.075f;

    bool firstCylinder = true;
    void Shoot()
    {
        soundManager.PlayEffect("shoot");

        var pos = rectTransform.position;
        Vector2 force;
        if (!sprite.flipX)
        {
            pos.x += 1.0f;
            force = new Vector2(bulletForce, 0.0f);
        }
        else
        {
            pos.x -= 1.0f;
            force = new Vector2(-bulletForce, 0.0f);
        }

        var shot = Instantiate(bullets[bulletIndex], pos, Quaternion.identity);
        shot.GetComponent<Rigidbody2D>().AddForce(force);
        shot.GetComponent<SpriteRenderer>().flipX = sprite.flipX;
        shot.GetComponent<BulletScript>().player = this;

        if (firstCylinder)
        {
            var bulletType = bullets[bulletIndex].GetComponent<BulletScript>().type;
            var typeText = bulletType.ToString().Replace("_", " ");
            bulletTexts[bulletIndex] = typeText;

            // replace sprite in cylinder
            var cylinderBullet = revolverCylinder.transform.Find($"Bullet{bulletIndex}").gameObject;
            if (bulletType == BulletScript.BulletType.Big)
            {
                cylinderBullet.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
            }
            else if (bulletType == BulletScript.BulletType.Explosive)
            {
                cylinderBullet.transform.localScale = new Vector3(0.3f, 0.3f, 1f);
            }

            var cylinderBulletSprite = cylinderBullet.GetComponent<SpriteRenderer>();
            var currentBulletSprite = bullets[bulletIndex].GetComponent<SpriteRenderer>();
            cylinderBulletSprite.color = currentBulletSprite.color;
            cylinderBulletSprite.sprite = currentBulletSprite.sprite;
        }

        bulletIndex += 1;
        if (bulletIndex >= bullets.Length)
        {
            bulletIndex = 0;
            firstCylinder = false;
        }

        revolverText.text = bulletTexts[bulletIndex];

        gameManager.bulletCount += 1;

        revolverCylinder.transform.Rotate(0.0f, 0.0f, -60f);
        revolverRingOverlay.SetActive(true);
    }

    public void DealDamage(float damage, bool fire = false)
    {
        health -= damage;

        if (!fire)
        {
            var text = Instantiate(damageText, transform.position, Quaternion.identity, canvas.transform);
            text.GetComponent<Text>().text = $"-{damage}!";
            Destroy(text, 0.66f);
        }
        
    }

    void Start()
    {
        soundManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        bullets = gameManager.PopulateCylinder();

        sprite = this.GetComponent<SpriteRenderer>();
        rectTransform = this.GetComponent<RectTransform>();
        rigidbody2d = this.GetComponent<Rigidbody2D>();
        canvas = GameObject.Find("Canvas");

        revolverCylinder = revolver.transform.Find("Cylinder").gameObject;
        revolverRingOverlay = revolver.transform.Find("ring").Find("ring_overlay").gameObject;
        revolverRingOverlay.SetActive(false);

        var revolverTextObj = revolver.transform.Find("BulletType");
        if (revolver.transform.localScale.x == -1)
        {
            revolverTextObj.transform.localScale = new Vector3(-1f, 1f, 1f);
        }
        revolverText = revolverTextObj.GetComponent<Text>();

        objectSize = (rectTransform.rect.size * rectTransform.localScale) / 2;
        screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.position.z));
    }

    private Vector2 aiMovementTarget;
    private float aiMovementDelta = 0.0f;
    public float aiMovementInterval = 5.0f;

    void Update()
    {
        if (otherPlayer == null)
        {
            var found = GameObject.FindGameObjectsWithTag("Character");
            foreach (var character in found)
            {
                if (character.name != this.name)
                {
                    otherPlayer = character;
                }
            }
        }

        if (health <= 0.0f)
        {
            gameManager.GameOver(false);
            health = 0.0f;
        }

        healthBar.sizeDelta = new Vector2(health, healthBar.sizeDelta.y);
    }

    void FixedUpdate()
    {
        if (isFrozen)
        {
            return;
        }    

        Vector2 movement;
        if (isAi)
        {
            // emulate movement by picking a random spot on the screen and moving towards it for a certain duration, then picking something else
            aiMovementDelta -= Time.deltaTime;

            if (aiMovementDelta <= 0.0f || (int)rectTransform.position.x == (int)aiMovementTarget.x) // get a new position if time has passed or if arrived at the desired position
            {
                aiMovementTarget = new Vector2(Random.Range(-screenBounds.x + objectSize.x, screenBounds.x - objectSize.x), 0.0f);
                aiMovementDelta = aiMovementInterval;
            }

            //movement = Vector2.Lerp(rectTransform.position, aiMovementTarget, Time.deltaTime);
            movement = new Vector2(
                Mathf.Clamp(aiMovementTarget.x - rectTransform.position.x, -1.0f, 1.0f),
                Mathf.Clamp(aiMovementTarget.y - rectTransform.position.y, -1.0f, 1.0f));

            if (Random.Range(0.0f, 1.0f) < 0.02f) // jump every now and then
            {
                movement.y = 1.0f;
            }
            else
            {
                movement.y = 0.0f;
            }
        }
        else
        {
            movement = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        }
        Move(movement);


        float prevBulletDelta = bulletDelta;
        bulletDelta -= Time.deltaTime;

        if (bulletDelta <= 0.0f && prevBulletDelta > 0.0f) 
        {
            soundManager.PlayEffect("cycle");

            revolverRingOverlay.SetActive(false);
        }

        if (((isAi && Random.Range(0.0f, 1.0f) < 0.01f) || (!isAi && Input.GetKey(KeyCode.Space))) && bulletDelta <= 0.0f)
        {
            Shoot();
            bulletDelta = bulletInterval;
        }
    }
}
