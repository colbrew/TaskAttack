using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomWaveGenerator : MonoBehaviour {

    [Header("Set in Inspector")]
    public GameObject[] enemy;
    public int easyNumberOfEnemies = 3;
    public bool singleBossEnemy = false;
    public float speedMultiplier = 1;
    public PathType pathType;
    public float delayBetweenObjects = .35f;
    public float speed;
    public DifficultyLevel difficultyLevel;
    public bool createMirror = false;
    public bool onMirrorRotate180y = false;
    bool bSide = true;
    public int numberOfWaves = 1;
    bool nextWave = true;
    public float delayBetweenWaves = 1f;
    public AudioData startSound;
    public float persistentBossHealth;

    [Header("Flip Paths")]
    public bool uSideways = false;
    public bool sevenUpsideDown = false;
    public bool zSideways = false;

    [Header("Set for Linear Paths")]
    public bool pointWhereItsGoing = false;
    public bool onScreenLoop = false;
    public bool stopAtPoints = false;
    public float stopDuration = 1;
    public bool fireOnStop = false;
    public GameObject warningLine;
    
    [Header("Set Dynamically")]
    public Transform[] controlPointsList;
    public GameObject[] movingObjects;
    // Are we making a line or a loop?
    Vector3 pos;
    Vector3 lastPos;
    // variables to help move from control point set to control point set
    public float[] birthTime;
    int[] currentPoint;
    int[] nextPoint;
    int numberOfMovingObjects = 1;
    public float timeSinceLastObject;
    public float objectBirthTime;
    int nextObject = 1;
    float[] x = new float[8];
    float[] y = new float[8];
    public bool pause = false;
    public bool pauseStarted = false;
    string linearOrSplineOrBalls;
    bool startingWave = false;
    int currentWave = 1;
    // for increasing number or angle of boss shots
    public float startHealth;
    bool secondShot = false;
    bool thirdShot = false;
    float currentStopDuration;
    ParticleSystem.ShapeModule smoke;
    protected int enemyNumber;
    float lineXPos;

    public int numberOfPoints;

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

    // Use this for initialization
    void Start()
    {
        lineXPos = Camera.main.ViewportToWorldPoint(new Vector3(.03f, 0, 0)).x;
        enemyNumber = 0;
        onMirrorRotate180y = false;

        // variables for Bosses
        startHealth = enemy[0].GetComponent<EnemyBase>().health;// used to calculate boss injury stages
        persistentBossHealth = startHealth;// used to have persistent health across instantiations

        // set type of path for Move() and gizmos
        if (pathType == PathType.LinearHorizontal || pathType == PathType.LinearVertical || pathType == PathType.LinearSeven || pathType == PathType.LinearW || pathType == PathType.LinearZ || pathType == PathType.LinearDiagonal)
        {
            linearOrSplineOrBalls = "Linear";
        }
        else if(pathType == PathType.BouncingBalls)
        {
            linearOrSplineOrBalls = "Balls";
        }
        else
        {
            linearOrSplineOrBalls = "Spline";
        }

        // Setup the lerp points
        if (pathType == PathType.LinearHorizontal || pathType == PathType.LinearSeven || pathType == PathType.LinearDiagonal)
        {
            numberOfPoints = 3;
        }
        if (pathType == PathType.SplineJ || pathType == PathType.SplineU || pathType == PathType.LinearZ || pathType == PathType.LinearVertical)
        {
            numberOfPoints = 4;
        }
        if (pathType == PathType.LinearW)
        {
            numberOfPoints = 5;
        }
        if (pathType == PathType.SplineFigureEight)
        {
            numberOfPoints = 8;
        }
        if(pathType == PathType.BouncingBalls)
        {
            numberOfPoints = easyNumberOfEnemies + difficultyLevel.difficulty;
        }

        InitializePoints(numberOfPoints);
        RandomizePoints();
        SetPoints(numberOfPoints);
        if (pathType == PathType.LinearHorizontal)
        {
            Instantiate(warningLine, new Vector3(lineXPos, controlPointsList[0].position.y, 0), Quaternion.identity);
            if (createMirror)
            {
                lineXPos = -lineXPos;
            }
            Invoke("StartWave", delayBetweenWaves);
            startingWave = true;
        }
        else
        {
            StartWave();
        }
    }

    void StartWave()
    {
        numberOfMovingObjects = 1;
        nextObject = 1;
        if (startSound != null)
        {
            startSound.Play(transform);
        }

        // get the wave started
        SetDifficulty();
        if (linearOrSplineOrBalls != "Balls")
        {
            movingObjects[0] = Instantiate(enemy[enemyNumber]);
            if(onMirrorRotate180y)
            {
                movingObjects[0].transform.rotation = new Quaternion(0, 180, 0, 0);
            }

            movingObjects[0].transform.parent = transform;
            movingObjects[0].transform.position = controlPointsList[0].position;
            currentPoint = new int[movingObjects.Length];
            currentPoint[0] = 0;
            nextPoint = new int[movingObjects.Length];
            nextPoint[0] = 1;
            birthTime = new float[movingObjects.Length];
            birthTime[0] = Time.time;
            objectBirthTime = Time.time;
            startingWave = false;
        }
        else if(linearOrSplineOrBalls == "Balls") // if balls, we just have to instantiate them
        {
            for (int i = 0; i < numberOfPoints; i++)
            {
                if(enemyNumber > enemy.Length - 1)
                {
                    enemyNumber = 0;
                }
                movingObjects[i] = Instantiate(enemy[enemyNumber]);
                movingObjects[i].transform.parent = transform;
                enemyNumber++;
                // check if the upcoming ball instantiation point overlaps the player position. If so, create a new instantiation point.
                if (Player.S != null)
                {
                    while (Mathf.Abs(controlPointsList[i].position.y - Player.S.transform.position.y) <= .8f || Mathf.Abs(controlPointsList[i].position.x - Player.S.transform.position.x) <= .8f)
                    {
                        RandomizePoints();
                        SetPoints(numberOfPoints);
                    }
                }
                movingObjects[i].transform.position = controlPointsList[i].position;
            }          
        }
    }

    // create and name a list of Transforms
    void InitializePoints(int j)
    {
        controlPointsList = new Transform[j];
        for (int i = 0; i < j; i++)
        {
            controlPointsList[i] = new GameObject().transform;
            controlPointsList[i].name = "p" + i;
            controlPointsList[i].SetParent(transform);
        }
    }

    // randomize the Transform positions according to the rules for the type of path
    void RandomizePoints()
    {
        if (pathType == PathType.SplineJ)
        {
            x[0] = Random.Range(.1f, .5f);
            y[0] = 1.1f;
            x[1] = Random.Range(.1f, .5f);
            y[1] = Random.Range(.5f, .9f);
            x[2] = Random.Range(.5f, .9f);
            y[2] = Random.Range(.1f, .5f);
            y[3] = y[2];
            x[3] = 1.1f;
        }

        if (pathType == PathType.SplineU)
        {
            if (uSideways)
            {
                x[0] = x[3] = -.1f;
                y[0] = Random.Range(.75f, .85f);
                y[1] = Random.Range(.6f, .7f);
                y[2] = 1 - y[1];
                y[3] = 1 - y[0];
                x[1] = x[2] = Random.Range(.6f, .8f);
            }
            else
            {
                x[0] = Random.Range(.1f, .4f);
                y[0] = y[3] = 1.1f;
                x[1] = Random.Range(.15f, .4f);
                y[1] = Random.Range(.4f, .6f);
                x[2] = 1 - x[1];
                y[2] = y[1];
                x[3] = 1 - x[0];
            }
        }

        if (pathType == PathType.SplineFigureEight)
        {
            x[0] = x[4] = .5f;
            y[0] = 1.1f;
            x[1] = x[3] = x[6] = Random.Range(.7f, .9f);
            x[2] = x[5] = x[7] = 1 - x[1];
            y[1] = y[7] = Random.Range(.75f, .9f);
            y[2] = y[6] = Random.Range(.55f, .7f);
            y[3] = y[5] = Random.Range(.35f, .5f);
            y[4] = Random.Range(.1f, .3f);
        }

        if (pathType == PathType.LinearW)
        {
            x[0] = -.1f;
            y[0] = y[1] = y[3] = Random.Range(.7f, .8f);
            x[1] = Random.Range(.2f, .4f);
            x[2] = Random.Range(.6f, .8f);
            y[2] = y[4] = Random.Range(.2f, .6f);
            x[3] = 1 - x[1];
            x[4] = 1 - x[2];
        }

        if (pathType == PathType.LinearHorizontal)
        {
            if (singleBossEnemy)
            {
                x[0] = -.5f;
                x[2] = 1.5f;
            }
            else
            {
                x[0] = -.1f;
                x[2] = 1.1f;
            }
            x[1] = .5f;
            y[0] = y[1] = y[2] = Random.Range(.1f, .9f);
            onMirrorRotate180y = false;
        }

        if (pathType == PathType.LinearDiagonal)
        {
            x[0] = -.1f;
            x[1] = .5f;
            x[2] = 1.1f;
            y[0] = Random.Range(.85f, .95f);
            y[1] = .5f;
            y[2] = 1 - y[0];
        }

        if (pathType == PathType.LinearVertical)
        {
            if(singleBossEnemy)
            {
                y[0] = 1.25f;
                y[1] = .75f;
                y[2] = .25f;
                y[3] = -.25f;
            }
            else
            {
                y[0] = 1.1f;
                y[1] = .7f;
                y[2] = .3f;
                y[3] = -.1f;
            }
            x[0] = x[1] = x[2] = x[3] = Random.Range(.1f, .9f);
        }

        if(pathType == PathType.LinearSeven)
        {
            if (sevenUpsideDown)
            {
                x[0] = Random.Range(.8f, 1);
                y[0] = 1.1f;
                x[1] = Random.Range(.2f, .3f);
                y[1] = y[2] = Random.Range(.2f, .5f);
                x[2] = 1.1f;
            }
            else
            {
                x[0] = x[2] = -.1f;
                y[0] = y[1] = Random.Range(.7f, .8f);
                x[1] = Random.Range(.8f, .95f);
                y[2] = Random.Range(0, .2f);
            }
        }

        if (pathType == PathType.LinearZ)
        {
            if (zSideways)
            {
                x[0] = x[1] = Random.Range(.1f, .2f);
                y[0] = 1.1f;
                y[1] = Random.Range(.05f, .15f);
                y[2] = Random.Range(.7f, .8f);
                x[2] = x[3] = 1 - x[0];
                y[3] = -.1f;
            }
            else
            {
                x[0] = -.1f;
                y[0] = y[1] = Random.Range(.7f, .8f);
                x[1] = Random.Range(.8f, .95f);
                x[2] = 1 - x[1];
                y[2] = y[3] = 1 - y[1] - .05f;
                x[3] = 1.1f;
            }
        }

        if(pathType == PathType.BouncingBalls)
        {
            for (int i = 0; i < numberOfPoints; i++)
            {
                x[i] = Random.Range(.1f, .9f);
                y[i] = Random.Range(.3f, .9f);
            }
        }
    }

    void SetPoints(int j)
    {
        for (int i = 0; i < j; i++)
        {
            controlPointsList[i].position = Camera.main.ViewportToWorldPoint(new Vector3(x[i], y[i], 10));
        }
    }

    public virtual void SetDifficulty()
    {
        speed = (enemy[enemyNumber].GetComponent<EnemyBase>().speed + (.3f * difficultyLevel.difficulty)) * speedMultiplier;
        delayBetweenObjects = delayBetweenObjects - (.02f * difficultyLevel.difficulty);
        movingObjects = new GameObject[easyNumberOfEnemies + difficultyLevel.difficulty];
        if (stopAtPoints || singleBossEnemy)
        {
            movingObjects = new GameObject[1];
        }
    }

    private void Update()
    {
        if(persistentBossHealth <= 0) // kill boss if persistent health hits 0
        {
            if (movingObjects[0] != null)
            {
                movingObjects[0].GetComponent<EnemyBase>().health = 0;
            }
        }

        if (linearOrSplineOrBalls != "Balls")
        {
            // move each enemy that has been instantiated
            if (persistentBossHealth > 0) // check if it is a boss enemy who has died and should be frozen for explosion
            {
                for (int i = 0; i < numberOfMovingObjects; i++)
                {
                    if (linearOrSplineOrBalls == "Spline")
                    {
                        if (movingObjects[i] != null && movingObjects[i].activeInHierarchy)
                        {
                            Move(i);
                        }
                    }

                    if (linearOrSplineOrBalls == "Linear" && !startingWave)
                    {
                        if (movingObjects[i] != null && movingObjects[i].activeInHierarchy)
                        {
                            if (pause && !pauseStarted)
                            {
                                currentStopDuration = stopDuration;

                                if (movingObjects[i].GetComponent<EnemyBase>().health <= (startHealth * EnemyBase.BOSSSTAGE3MULTIPLIER))//fires a third time if health is less than 1/5
                                {
                                     thirdShot = secondShot = true;
                                }
                                else if (movingObjects[i].GetComponent<EnemyBase>().health <= (startHealth * EnemyBase.BOSSSTAGE2MULTIPLIER)) //fires a second time if health is less than 1/2
                                {
                                    secondShot = true;
                                }
                               
                                birthTime[i] = Time.time + currentStopDuration;
                                pauseStarted = true;
                                if (fireOnStop && difficultyLevel.difficulty > 0)
                                {
                                    movingObjects[i].GetComponent<EnemyBase>().fireDelegate();
                                    // for enemymuffler boss, increase angle of smoke as boss gets weaker
                                    if (movingObjects[0].GetComponentInChildren<Weapon>().Type == WeaponType.enemylaser)
                                    {
                                        smoke = movingObjects[0].GetComponentInChildren<Weapon>().currentLaserProjectile.GetComponent<ParticleSystem>().shape;

                                        if (movingObjects[i].GetComponent<EnemyBase>().health <= (startHealth * EnemyBase.BOSSSTAGE3MULTIPLIER))
                                        {
                                            smoke.angle *= 9;
                                        }
                                        else if (movingObjects[i].GetComponent<EnemyBase>().health <= (startHealth * EnemyBase.BOSSSTAGE2MULTIPLIER))
                                        {
                                            smoke.angle *= 4;
                                        }
                                    }
                                }
                            }
                            else if (pause)
                            {

                                if ((birthTime[i] - Time.time) < (currentStopDuration / 2) && thirdShot && secondShot && difficultyLevel.difficulty > 0)
                                {
                                    movingObjects[i].GetComponent<EnemyBase>().fireDelegate();
                                    secondShot = false;
                                }

                                if (Time.time >= birthTime[i])
                                {
                                    if (thirdShot && difficultyLevel.difficulty > 0)
                                    {
                                        movingObjects[i].GetComponent<EnemyBase>().fireDelegate();
                                        thirdShot = false;
                                    }
                                    else if (secondShot && difficultyLevel.difficulty > 0)
                                    {
                                        movingObjects[i].GetComponent<EnemyBase>().fireDelegate();
                                        secondShot = false;
                                    }
                                    pause = false;
                                    pauseStarted = false;
                                    movingObjects[i].GetComponentInChildren<Weapon>().StopLaser();
                                }
                            }
                            else
                            {
                                Move(i);
                            }
                        }
                    }
                }
            }

            timeSinceLastObject = Time.time - objectBirthTime;

            // Instantiate new movingobjects with a set delay until numberOfMovingObjects have been instantiated
            if (numberOfMovingObjects < movingObjects.Length && timeSinceLastObject > delayBetweenObjects)
            {
                if (enemyNumber == enemy.Length - 1)
                {
                    enemyNumber = 0;
                }
                else
                {
                    enemyNumber++;
                }
                movingObjects[nextObject] = Instantiate(enemy[enemyNumber]);
                movingObjects[nextObject].transform.parent = transform;
                movingObjects[nextObject].transform.position = controlPointsList[0].position;
                currentPoint[nextObject] = 0;
                nextPoint[nextObject] = 1;
                birthTime[nextObject] = Time.time;
                nextObject++;
                numberOfMovingObjects++;
                objectBirthTime = Time.time;
            }

            // if all enemies of the wave have been spawned, check if they have all been destroyed. if so end wave
            if (numberOfMovingObjects == movingObjects.Length && !startingWave)
            {
                for (int i = 0; i < numberOfMovingObjects; i++)
                {
                    if (movingObjects[i] != null)
                    {
                        if (movingObjects[i].activeInHierarchy)
                        {
                            break;
                        }
                        if (i == numberOfMovingObjects - 1)
                        {
                            if(persistentBossHealth <= 0 ) // if this is a persistent boss who has died, we don't want to spawn mirrors or next waves
                            {
                                gameObject.SetActive(false);
                            }
                            else if (createMirror && bSide)
                            {
                                if (pathType == PathType.SplineJ)
                                {
                                    x[0] = 1 - x[0];
                                    x[1] = 1 - x[1];
                                    x[2] = 1 - x[2];
                                    x[3] = -.1f;
                                }
                                if (pathType == PathType.SplineU)
                                {
                                    if (uSideways)
                                    {
                                        x[0] = x[3] = 1.1f;
                                        x[1] = x[2] = 1 - x[1];
                                    }
                                    else
                                    {
                                        x[0] = 1 - x[0];
                                        x[1] = 1 - x[1];
                                        x[2] = 1 - x[2];
                                        x[3] = 1 - x[3];
                                    }
                                }
                                if (pathType == PathType.SplineFigureEight)
                                {
                                    x[1] = x[7];
                                    x[2] = x[3];
                                    x[3] = x[6] = x[1];
                                    x[5] = x[7] = x[2];
                                }
                                if (pathType == PathType.LinearHorizontal)
                                {
                                    if (singleBossEnemy)
                                    {
                                        x[0] = 1.5f;
                                        x[2] = -.5f;
                                    }
                                    else
                                    {
                                        x[0] = 1.1f;
                                        x[2] = -.1f;
                                    }
                                    y[0] = y[1] = y[2] = Random.Range(.1f, .9f);
                                    onMirrorRotate180y = true;
                                }
                                if (pathType == PathType.LinearDiagonal)
                                {
                                    x[0] = 1.1f;
                                    x[2] = -.1f;
                                }
                                if (pathType == PathType.LinearVertical)
                                {
                                    if (singleBossEnemy)
                                    {
                                        y[0] = -.5f;
                                        y[3] = 1.5f;

                                    }
                                    else
                                    {
                                        y[0] = -.1f;
                                        y[3] = 1.1f;
                                    }
                                }
                                if (pathType == PathType.LinearSeven)
                                {
                                    if (sevenUpsideDown)
                                    {
                                        x[0] = 1 - x[0];
                                        x[1] = 1 - x[1];
                                        x[2] = -.1f;
                                    }
                                    else
                                    {
                                        x[0] = x[2] = 1.1f;
                                        x[1] = 1 - x[1];
                                    }
                                }
                                if (pathType == PathType.LinearZ)
                                {
                                    if (zSideways)
                                    {
                                        x[0] = x[1] = 1 - x[0];
                                        x[2] = x[3] = 1 - x[0];
                                    }
                                    else
                                    {
                                        x[0] = 1.1f;
                                        x[1] = 1 - x[1];
                                        x[2] = 1 - x[2];
                                        x[3] = -.1f;
                                    }
                                }
                                bSide = false;
                                SetPoints(numberOfPoints);
                                if (pathType == PathType.LinearHorizontal)
                                {
                                    Instantiate(warningLine, new Vector3(lineXPos, controlPointsList[0].position.y, 0), Quaternion.identity);
                                    lineXPos = -lineXPos;
                                }
                                Invoke("StartWave", delayBetweenWaves);
                                startingWave = true;
                            }
                            else if (currentWave >= numberOfWaves)
                            {
                                gameObject.SetActive(false);
                            }
                            else
                            {
                                currentWave++;
                                bSide = true;
                                RandomizePoints();
                                SetPoints(numberOfPoints);
                                if (pathType == PathType.LinearHorizontal)
                                {
                                    Instantiate(warningLine, new Vector3(lineXPos, controlPointsList[0].position.y, 0), Quaternion.identity);
                                    if (createMirror)
                                    {
                                        lineXPos = -lineXPos;
                                    }
                                }
                                Invoke("StartWave", delayBetweenWaves);
                                startingWave = true;
                            }
                        }
                    }
                }
            }
        }
        else if (linearOrSplineOrBalls == "Balls")
        {
            for (int i = 0; i < numberOfPoints; i++)
            {
                if (movingObjects[i] != null)
                {
                    if (movingObjects[i].activeInHierarchy)
                    {
                        break;
                    }
                    else if (i == numberOfPoints - 1)
                    {
                        this.gameObject.SetActive(false);
                    }
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;

        if (linearOrSplineOrBalls == "Spline")
        {
            //Draw the Catmull-Rom spline between the points
            for (int i = 0; i < controlPointsList.Length; i++)
            {
                // Can't draw between the endpoints
                // Neither do we need to draw from the second to the last endpoint
                // if we are not making a looping line
                /* if ((i == controlPointsList.Length -1) && !isLooping)
                {
                    continue;
                } */

                DisplayCatmullRomSpline(i);
            }
        }

        if(linearOrSplineOrBalls == "Linear")
        {
            for (int i = 0; i < (controlPointsList.Length - 1); i++)
            {
                Gizmos.DrawLine(controlPointsList[i].position, controlPointsList[i + 1].position);
            }

            if (controlPointsList.Length > 0 && onScreenLoop)
                Gizmos.DrawLine(controlPointsList[controlPointsList.Length - 1].position, controlPointsList[1].position);
        }
    }

    // Display a spline between 2 points derived with the Catmull-Rom spline algorithm
    void DisplayCatmullRomSpline(int position)
    {
        // The four points we need to form a spline between p1 and p2
        Vector3 p0 = controlPointsList[ClampListPos(position - 1)].position;
        Vector3 p1 = controlPointsList[position].position;
        Vector3 p2 = controlPointsList[ClampListPos(position + 1)].position;
        Vector3 p3 = controlPointsList[ClampListPos(position + 2)].position;

        // The start position of the line
        lastPos = p1;

        // The spline's resolution
        // Make sure it's adding up to 1, so 0.3 will give a gap, but 0.2 will work
        float resolution = 0.2f;

        // How many times should we loop?
        int loops = Mathf.FloorToInt(1f / resolution);

        for (int i = 1; i <= loops; i++)
        {
            // Which t position are we at?
            float t = i * resolution;

            // Find the coordinates between the end points with a Catmull-Rom spline
            Vector3 newPos = GetCatmullRomPosition(t, p0, p1, p2, p3);

            // Draw this line segment
            Gizmos.DrawLine(lastPos, newPos);

            // Save this pos so we can draw the next line segment
            lastPos = newPos;
        }
    }

    // Clamp the list positions to allow looping
    int ClampListPos(int pos)
    {
        if (pos < 0)
        {
            pos = controlPointsList.Length - 1;
        }

        if (pos > controlPointsList.Length)
        {
            pos = 1;
        }
        else if (pos > controlPointsList.Length - 1)
        {
            pos = 0;
        }

        return pos;
    }

    // Returns a position between 4 Vector3 with Catmull-Rom spline algorithm
    // http://wwww.iquilezles.org/www/articles/minispline/minispline.htm
    Vector3 GetCatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        // The coefficients of the cubic polynomial(except the 0.5f * which I added later for performance)
        Vector3 a = 2f * p1;
        Vector3 b = p2 - p0;
        Vector3 c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
        Vector3 d = -p0 + 3f * p1 - 3f * p2 + p3;

        // The cubic polynomial: a + b * t + c * t^2 + d * t^3
        Vector3 position = 0.5f * (a + (b * t) + (c * t * t) + (d * t * t * t));

        return position;
    }


    void Move(int x)
    {
        if (linearOrSplineOrBalls == "Spline")
        {
            pos = movingObjects[x].transform.position;
            Vector3 oldPosition = pos;
            Quaternion oldRotation = movingObjects[x].transform.rotation;

            float t = (Time.time - birthTime[x]) * speed;

            if (t >= 1)
            {
                currentPoint[x]++;
                birthTime[x] = Time.time;
                return;
            }

            if (currentPoint[x] >= controlPointsList.Length)
            {
                currentPoint[x] = 0;
                if (!nextWave)
                    movingObjects[x].SetActive(false);
                return;
            }

            // The four points we need to form a spline between p1 and p2
            Vector3 p0 = controlPointsList[ClampListPos(currentPoint[x] - 1)].position;
            Vector3 p1 = controlPointsList[currentPoint[x]].position;
            Vector3 p2 = controlPointsList[ClampListPos(currentPoint[x] + 1)].position;
            Vector3 p3 = controlPointsList[ClampListPos(currentPoint[x] + 2)].position;

            // The coefficients of the cubic polynomial(except the 0.5f * which I added later for performance)
            Vector3 a = 2f * p1;
            Vector3 b = p2 - p0;
            Vector3 c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
            Vector3 d = -p0 + 3f * p1 - 3f * p2 + p3;

            // The cubic polynomial: a + b * t + c * t^2 + d * t^3
            pos = 0.5f * (a + (b * t) + (c * t * t) + (d * t * t * t));
            Vector3 newPosition = movingObjects[x].transform.position = pos;

            // rotate enemy to face the direction it is moving
            if ((newPosition - oldPosition) != Vector3.zero)
            {
                Quaternion newRotation = Quaternion.LookRotation(newPosition - oldPosition, Vector3.forward);
                // 2d can only rotate around z axis, so set x & y rotations to 0
                newRotation.x = 0.0f;
                newRotation.y = 0.0f;
                movingObjects[x].transform.rotation = newRotation;
            }
        }

        if (linearOrSplineOrBalls == "Linear")
        {
            float u;

            if (pathType == PathType.LinearW && currentPoint[0] == 0)// slow down linearW boss entry onto screen for dramatic effect
            {
                u = (Time.time - birthTime[x]) * speed / 3;
            }
            else
            {
                u = (Time.time - birthTime[x]) * speed;
            }

            if (u >= 1)
            {
                currentPoint[x]++;
                if (currentPoint[x] == controlPointsList.Length - 1)
                {
                    nextPoint[x] = 1;


                    if(!onScreenLoop)
                    {
                        currentPoint[x] = 0;
                        movingObjects[x].SetActive(false);
                        if(x == 0)
                        {
                            persistentBossHealth = movingObjects[0].GetComponent<EnemyBase>().health;
                        }
                        return;
                    }
                }
                else
                {
                    if (currentPoint[x] == controlPointsList.Length)
                    {
                        currentPoint[x] = 1;
                    }
                    nextPoint[x] = currentPoint[x] + 1;
                }
                
                // only allow delays at points if no more than one movingobject (prevent multiple objects from running into each other)
                if (stopAtPoints && movingObjects.Length == 1)
                {
                    pause = true;
                }
                else
                {
                    birthTime[x] = Time.time;
                }
                return;
            }
            
            Vector3 a = controlPointsList[currentPoint[x]].position;
            Vector3 b = controlPointsList[nextPoint[x]].position;
            Vector3 pos = Vector3.Lerp(a, b, u);
            movingObjects[x].transform.position = pos;
            
            if(pointWhereItsGoing)
            {
                // rotate enemy to face the direction it is moving
                Quaternion newRotation = Quaternion.LookRotation(controlPointsList[nextPoint[x]].position - controlPointsList[currentPoint[x]].position, Vector3.forward);
                    // 2d can only rotate around z axis, so set x & y rotations to 0
                    newRotation.x = 0.0f;
                    newRotation.y = 0.0f;
                    movingObjects[x].transform.rotation = newRotation;     
            }
        }
    }
}
