using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfiniteWaves : MonoBehaviour {

    static float TRACKVOLUME = .6f;

    public GameObject wave;
    public GameObject[] flyingEnemies;
    public GameObject[] ballEnemies;
    public GameObject[] bossEnemies;
    public int numberOfSimultaneousPaths = 1;
    public int numberOfEnemiesOnPath = 3;
    public DifficultyLevel infiniteDifficulty;
    public int randomEnemy;
    public int randomPath;
    public PathType[] flyingPathTypes = new PathType[] { PathType.SplineJ, PathType.SplineU, PathType.SplineFigureEight, PathType.LinearHorizontal, PathType.LinearVertical, PathType.LinearDiagonal, PathType.LinearSeven, PathType.LinearZ };
    public AudioSource[] gameplayTracks;
    public int nextTrack = 0;
    public GameObject warningLineMinivan;

    GameObject[] currentWave;
    RandomWaveGenerator waveGen;
    int waveNumber = 1;
    int deadPaths;
    EnemyType enemyType;
    Text text;
    Text waveCounterText;
    float displayedDifficultyNumber = 1.0f;

    public enum PathType
    {
        SplineJ,
        SplineU,
        SplineFigureEight,
        LinearW,
        LinearHorizontal,
        LinearVertical,
        LinearDiagonal,
        LinearSeven,
        LinearZ,
        BouncingBalls
    }

    public enum EnemyType
    {
        flying,
        ball,
        boss
    }

    public enum BossType
    {
        bossW,
        bossWStopAtPoints,
        bossLinear
    }

    // Use this for initialization
    void Start () {
        enemyType = EnemyType.flying;
        numberOfSimultaneousPaths = 1;
        infiniteDifficulty.difficulty = 0;
        text = GameObject.Find("CenterBillboard").GetComponent<Text>();
        waveCounterText = GameObject.Find("WaveCounterText").GetComponent<Text>();
        StartInfiniteWave();
        gameplayTracks[nextTrack].volume = TRACKVOLUME;
	}
	
    void StartInfiniteWave()
    {
        currentWave = new GameObject[numberOfSimultaneousPaths];
        waveCounterText.text = "Wave\n" + (waveNumber).ToString();

        if (enemyType == EnemyType.ball)
        {
            currentWave[0] = Instantiate(wave);
            waveGen = currentWave[0].GetComponent<RandomWaveGenerator>();

            //initialize enemy array
            waveGen.easyNumberOfEnemies = numberOfEnemiesOnPath;
            waveGen.enemy = new GameObject[numberOfEnemiesOnPath + infiniteDifficulty.difficulty];

            for (int x = 0; x < waveGen.enemy.Length; x++)
            {
                waveGen.enemy[x] = ballEnemies[Random.Range(0, ballEnemies.Length)];
            }

            //initialize path
            waveGen.pathType = RandomWaveGenerator.PathType.BouncingBalls;
            waveGen.pointWhereItsGoing = true;
        }
        else
        {
            for (int j = 0; j < numberOfSimultaneousPaths; j++)
            {
                currentWave[j] = Instantiate(wave);
                waveGen = currentWave[j].GetComponent<RandomWaveGenerator>();

                //initialize enemy array
                waveGen.easyNumberOfEnemies = numberOfEnemiesOnPath;
                waveGen.enemy = new GameObject[numberOfEnemiesOnPath + infiniteDifficulty.difficulty];

                if (enemyType == EnemyType.flying)
                {
                    for (int x = 0; x < waveGen.enemy.Length; x++)
                    {
                        waveGen.enemy[x] = flyingEnemies[Random.Range(0, flyingEnemies.Length)];
                    }

                    //initialize path
                    waveGen.pathType = (RandomWaveGenerator.PathType)flyingPathTypes[Random.Range(0, flyingPathTypes.Length)];
                    waveGen.pointWhereItsGoing = true;
                }



                if (enemyType == EnemyType.boss)
                {
                    waveGen.singleBossEnemy = true;

                    int x = Random.Range(0, 7);
                    //int x = 4; //for testing pie boss
                    if (x < 4)//Alien,Carrot,Car,Bottle bosses
                    {
                        waveGen.enemy[0] = bossEnemies[x];
                        waveGen.stopAtPoints = true;
                        waveGen.fireOnStop = true;
                        waveGen.onScreenLoop = true;
                        waveGen.pathType = RandomWaveGenerator.PathType.LinearW;
                        if (x == 2 || x == 3)//muffler or bottle
                        {
                            waveGen.stopDuration = 3;
                        }
                    }
                    else if (x < 5)//Piechart boss
                    {
                        waveGen.enemy[0] = bossEnemies[4];
                        waveGen.onScreenLoop = true;
                        waveGen.pathType = RandomWaveGenerator.PathType.LinearW;
                    }
                    else if (x < 6) //miniVan boss
                    {
                        waveGen.enemy[0] = bossEnemies[5];
                        waveGen.createMirror = true;
                        waveGen.numberOfWaves = 99;
                        waveGen.warningLine = warningLineMinivan;
                        waveGen.pathType = RandomWaveGenerator.PathType.LinearHorizontal;
                    }
                    else //missile boss
                    {
                        waveGen.enemy[0] = bossEnemies[6];
                        waveGen.numberOfWaves = 99;
                        waveGen.pathType = RandomWaveGenerator.PathType.LinearVertical;
                    }
                }
            }
        }
        waveNumber++;

    }

	// Update is called once per frame
	void Update () {
        for (int k = 0; k < numberOfSimultaneousPaths; k++)
        {
            if (currentWave[k])
            {
                if (!currentWave[k].activeInHierarchy)
                {
                    Destroy(currentWave[k]);
                    deadPaths++;
                }
            }
        }

        if(deadPaths == numberOfSimultaneousPaths || (enemyType == EnemyType.ball && deadPaths == 1))
        {
            deadPaths = 0;
            if(waveNumber % 10 == 1 && waveNumber != 1)//up difficulty after each boss wave
            {
                StartCoroutine(UpDifficulty());
            }
            if (waveNumber % 10 == 0)
            {
                enemyType = EnemyType.boss;
            }
            else if(waveNumber % 5 == 0)
            {
                enemyType = EnemyType.ball;
            }
            else
            {
                enemyType = EnemyType.flying;
            }
            StartInfiniteWave();
        }
	}

    IEnumerator UpDifficulty()
    {
        if (infiniteDifficulty.difficulty < 3)
        {
            infiniteDifficulty.difficulty++;
        }
        else if(infiniteDifficulty.difficulty == 3)
        {
            numberOfSimultaneousPaths++;
            infiniteDifficulty.difficulty = 1;
        }

        nextTrack++;
        if (nextTrack < 8)
        {
            gameplayTracks[nextTrack].volume = TRACKVOLUME;
        }
        displayedDifficultyNumber++;
        text.text = "Difficulty " + displayedDifficultyNumber + ".0";
        yield return new WaitForSeconds(2);
        text.text = "";
    }
}
