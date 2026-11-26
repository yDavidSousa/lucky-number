using UnityEngine;

public static class DiceSpawnSystem
{
    public static Vector3 GetLandingPosition(Vector3 landingCenter, Vector3 landingRangeX, Vector3 landingRangeZ)
    {
        return landingCenter + new Vector3(
            Random.Range(landingRangeX.x, landingRangeX.y),
            0f,
            Random.Range(landingRangeZ.x, landingRangeZ.y)
        );
    }

    public static Vector3 GetSeparatedLandingPosition(Vector3 landingCenter, Vector3 landingRangeX, Vector3 landingRangeZ, float minDiceDistance, Vector3 firstPosition)
    {
        for (int i = 0; i < 30; i++)
        {
            Vector3 position = GetLandingPosition(landingCenter,  landingRangeX, landingRangeZ);

            Vector3 delta = position - firstPosition;
            delta.y = 0f;

            if (delta.sqrMagnitude >= minDiceDistance * minDiceDistance)
            {
                return position;
            }
        }

        return firstPosition + Vector3.right * minDiceDistance;
    }
}