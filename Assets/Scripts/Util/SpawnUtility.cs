using UnityEngine;

/// <summary>
/// Small collection of spawn-related helpers shared across spawner systems.
/// </summary>
public static class SpawnUtility
{
    /// <summary>
    /// Returns a random point guaranteed to be inside the given collider's actual shape
    /// (not just its bounding box). Uses rejection sampling: picks random points inside
    /// the collider's AABB and keeps the first one that OverlapPoint confirms is inside.
    /// Falls back to bounds.center if no valid point is found within maxAttempts
    /// (e.g. for thin/odd-shaped colliders where random AABB points rarely land inside).
    /// </summary>
    public static Vector2 GetRandomPointInCollider(Collider2D collider, int maxAttempts = 30)
    {
        Bounds bounds = collider.bounds;

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector2 candidate = new Vector2(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y)
            );

            if (collider.OverlapPoint(candidate))
                return candidate;
        }

        Debug.LogWarning($"SpawnUtility: couldn't find a point inside '{collider.name}' after " +
                          $"{maxAttempts} attempts, falling back to bounds center.");
        return bounds.center;
    }
}