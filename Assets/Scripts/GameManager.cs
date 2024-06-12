using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public bool gameOver = false;
    public int bulletCount = 0;

    public float timer = 60f;
    private Text timerText;
    private GameObject gameOverScreen;

    private PlayerController[] players;

    public GameObject normalBullet;
    public List<GameObject> bulletPool;

    public GameObject[] PopulateCylinder()
    {
        var bullets = new GameObject[] // 6 normal to start
        {
            normalBullet, normalBullet, normalBullet, normalBullet, normalBullet, normalBullet
        };

        // randomly change 3 bullets to random special bullet
        var alreadyTouched = new List<int>();
        while (alreadyTouched.Count < 3)
        {
            int index = Random.Range(0, 5);
            if (alreadyTouched.Contains(index))
            {
                continue;
            }

            int specialIndex = Random.Range(0, bulletPool.Count);
            bullets[index] = bulletPool[specialIndex];
            bulletPool.RemoveAt(specialIndex);

            alreadyTouched.Add(index);
        }

        return bullets;
    }

    public void GameOver(bool isTimer)
    {
        gameOver = true;
        var winner = players[0];

        foreach (var player in players)
        {
            player.isFrozen = true;
            if (player.health > winner.health)
                winner = player;
        }

        string reason = "";
        if (isTimer)
        {
            reason = "having the highest remaining health";
        }
        else
        {
            reason = "killing their opponent";
        }

        gameOverScreen.SetActive(true);
        var descriptionText = gameOverScreen.transform.Find("Description").GetComponent<Text>();
        descriptionText.text = descriptionText.text
            .Replace("playerName", winner.playerName)
            .Replace("reason", reason)
            .Replace("bulletCount", bulletCount.ToString());
    }

    void Start()
    {
        timerText = GameObject.Find("Timer").GetComponent<Text>();
        gameOverScreen = GameObject.Find("GameOverScreen");
        gameOverScreen.SetActive(false);

        players = GameObject.FindGameObjectsWithTag("Character").Select(x => x.GetComponent<PlayerController>()).ToArray();
        foreach (var player in players)
        {
            player.isFrozen = false;
        }
    }

    void Update()
    {
        if (gameOver && Input.GetKey(KeyCode.Return))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (gameOver)
        {
            return;
        }

        timer -= Time.deltaTime;

        if (timer <= 0.0f)
        {
            timer = 0.0f;
            GameOver(true);
        }

        timerText.text = System.Math.Round(timer, 2).ToString("0.00");
    }
}