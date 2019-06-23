using UnityEngine;

public class RandomWaveGeneratorInfinite : RandomWaveGenerator {

    private void Awake()
    {
        delayBetweenObjects = .4f;
    }
    public override void SetDifficulty() // slower difficulty progression for infinite
    {
        speed = (enemy[enemyNumber].GetComponent<EnemyBase>().speed + (.3f * difficultyLevel.difficulty)) * speedMultiplier / 2;
        delayBetweenObjects = delayBetweenObjects - (.05f * difficultyLevel.difficulty) / 2;
        movingObjects = new GameObject[easyNumberOfEnemies + difficultyLevel.difficulty];
        if (stopAtPoints || singleBossEnemy)
        {
            movingObjects = new GameObject[1];
        }
    }
}
