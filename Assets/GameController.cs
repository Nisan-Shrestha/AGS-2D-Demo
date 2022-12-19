using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class GameController : MonoBehaviour
{

    public TextMeshProUGUI score;
    public TextMeshProUGUI gameOverText;
    public GameObject spawners;
    int scoreIndex = 0;
    public bool gameOver;
    public int activeEnemyCount = 0;
    public int maxActiveEnemy = 6;
    public GameObject enemyPrefab;
    Transform[] spawnPoints;
    // Start is called before the first frame update
    void Start()
    {
        gameOver = false;
        gameOverText.enabled = false;
        spawnPoints = spawners.GetComponentsInChildren<Transform>();

    }

    // Update is called once per frame
    void Update()
    {
        if (activeEnemyCount < maxActiveEnemy)
        {
            Vector3 pos = spawnPoints[Random.Range(0, 6)].position;
            pos.x = pos.x + Random.Range(-1.0f, +1.0f);
            Object.Instantiate(enemyPrefab, pos ,Quaternion.identity);
            activeEnemyCount++;
        }
    }

    public void addScore()
    {
        scoreIndex += 1;
        score.text = scoreIndex.ToString();
        return;
    }
}
