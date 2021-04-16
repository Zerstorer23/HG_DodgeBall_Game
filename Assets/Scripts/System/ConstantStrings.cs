using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstantStrings : MonoBehaviour
{

    public const string PREFAB_BULLET_1 = "Prefabs/Units/Bullet1";
    public const string PREFAB_BULLET_NAGATO = "Prefabs/Units/Bullet_nagato";
    public const string PREFAB_BULLET_HARUHI = "Prefabs/Units/Bullet_haruhi";
    public const string PREFAB_PLAYER = "Prefabs/Units/Player";
    public const string PREFAB_STARTSCENE_PLAYERNAME = "Prefabs/ConnectedUserName";
    public const string TAG_PROJECTILE = "Projectile";
    public const string TAG_PLAYER = "Player";
    public const string TAG_BOUNDARY = "MapBoundary";
    public const string TAG_BOX_OBSTACLE = "BoxObstacle";


    public const string HASH_MAP_DIFF = "MapDifficulty";
    public const string HASH_PLAYER_LIVES = "PlayerLives";

    public static Color GetColorByHex(string hex)
    {
        Color newCol;
        ColorUtility.TryParseHtmlString(hex, out newCol);
        return newCol;
    }
    public static float GetAngleBetween(Vector3 from, Vector3 to)
    {
        Vector3 diff = to - from;
        diff.Normalize();
        float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;

        return rot_z;// new Vector3(0f, 0f, rot_z);
    }
}
