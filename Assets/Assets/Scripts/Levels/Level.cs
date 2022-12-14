using UnityEngine;

public class Level : MonoBehaviour
{
    //Basic class for setting up levels with inheritance
    protected string levelName;
    protected Vector3 startPos;
    protected bool isUnlocked = true;

    public Level(string nameOfLevel)
    {
        levelName = nameOfLevel;
    }

    public virtual void LevelStart()
    {
    }

    public virtual void LevelUpdate()
    {
    }

    public virtual void LevelEnd()
    {
    }

    public virtual void ResetLevel()
    {
    }

    public string GetLevelString()
    {
        return levelName;
    }

    public void SetUnlocked(bool unlockBool)
    {
        isUnlocked = unlockBool;
    }

    public bool IsUnlocked()
    {
        return isUnlocked;
    }
}
