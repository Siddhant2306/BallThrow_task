using UnityEngine;

[System.Serializable]
public class LevelDifficultyTuning
{
    [Header("Ramp")]
    [Tooltip("How many levels it takes to reach max difficulty (roughly).")]
    public int rampLevels = 10;

    [Header("Static Obstacles")]
    public int staticObstacleMin = 0;
    public int staticObstacleMax = 6;

    [Header("Moving Obstacles")]
    [Tooltip("First level that can spawn moving obstacles.")]
    public int movingObstacleStartLevel = 3;
    public int movingObstacleMax = 4;
    public float movingSpeedMin = 0.9f;
    public float movingSpeedMax = 2.8f;
    public float movingAmplitudeMin = 0.7f;
    public float movingAmplitudeMax = 2.0f;

    [Header("Goal")]
    public float goalScaleLevel1 = 1f;
    public float goalScaleMin = 0.55f;

    [Header("Fairness")]
    [Tooltip("A 'reserved safe corridor' around the spawn->goal line. Higher is easier.")]
    public float safeCorridorWidthEasy = 3.6f;
    public float safeCorridorWidthHard = 1.6f;

    [Tooltip("Minimum spacing between spawned obstacles. Higher is easier.")]
    public float obstacleSpacingEasy = 3.0f;
    public float obstacleSpacingHard = 1.7f;
}

public struct LevelDifficultySettings
{
    public int level;
    public float t01;
    public int staticObstacleCount;
    public int movingObstacleCount;
    public float movingSpeed;
    public float movingAmplitude;
    public float goalScale;
    public float safeCorridorWidth;
    public float obstacleSpacing;
}

public static class LevelDifficulty
{
    public static LevelDifficultySettings Evaluate(int level, LevelDifficultyTuning tuning)
    {
        if (tuning == null)
            tuning = new LevelDifficultyTuning();

        int safeLevel = Mathf.Max(1, level);
        int ramp = Mathf.Max(1, tuning.rampLevels);
        float t = Mathf.Clamp01((safeLevel - 1) / (float)ramp);

        int staticCount = Mathf.RoundToInt(Mathf.Lerp(tuning.staticObstacleMin, tuning.staticObstacleMax, t));

        int movingCount = 0;
        if (safeLevel >= Mathf.Max(1, tuning.movingObstacleStartLevel))
        {
            float mt = Mathf.Clamp01((safeLevel - tuning.movingObstacleStartLevel) / (float)Mathf.Max(1, ramp));
            movingCount = Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(1, tuning.movingObstacleMax, mt)), 0, tuning.movingObstacleMax);
        }

        float movingSpeed = Mathf.Lerp(tuning.movingSpeedMin, tuning.movingSpeedMax, t);
        float movingAmp = Mathf.Lerp(tuning.movingAmplitudeMin, tuning.movingAmplitudeMax, t);
        float goalScale = Mathf.Lerp(tuning.goalScaleLevel1, tuning.goalScaleMin, t);

        float corridor = Mathf.Lerp(tuning.safeCorridorWidthEasy, tuning.safeCorridorWidthHard, t);
        float spacing = Mathf.Lerp(tuning.obstacleSpacingEasy, tuning.obstacleSpacingHard, t);

        return new LevelDifficultySettings
        {
            level = safeLevel,
            t01 = t,
            staticObstacleCount = Mathf.Max(0, staticCount),
            movingObstacleCount = Mathf.Max(0, movingCount),
            movingSpeed = Mathf.Max(0.01f, movingSpeed),
            movingAmplitude = Mathf.Max(0f, movingAmp),
            goalScale = Mathf.Max(0.05f, goalScale),
            safeCorridorWidth = Mathf.Max(0f, corridor),
            obstacleSpacing = Mathf.Max(0.01f, spacing),
        };
    }
}

