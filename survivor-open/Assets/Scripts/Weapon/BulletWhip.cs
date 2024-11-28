using UnityEngine;

public class BulletWhip : BaseBullet
{
    [SerializeField] private Vector2 squareHitbox;
    [SerializeField] private Transform hitBoxSize;

    public override void Initialize(IControllable[] _injectedElements)
    {
        base.Initialize(_injectedElements);
        Debug.Log("spawning Bullet Whip");
        hitBoxSize.localScale = squareHitbox;
    }



    protected override bool CheckHitBox(Enemy _enemy)
    {
        Vector2 adjustedSquareHitbox = new Vector2(squareHitbox.x * transform.lossyScale.x, squareHitbox.y * transform.lossyScale.y);
      
        float halfWidth = adjustedSquareHitbox.x / 2f;
        float halfHeight = adjustedSquareHitbox.y / 2f;

        // Calculate bounds of the rectangle
        float leftBound = transform.position.x - halfWidth;
        float rightBound = transform.position.x + halfWidth;
        float bottomBound = transform.position.y - halfHeight;
        float topBound = transform.position.y + halfHeight;


        bool result = _enemy.transform.position.x >= leftBound && _enemy.transform.position.x <= rightBound &&
        _enemy.transform.position.y >= bottomBound && _enemy.transform.position.y <= topBound;

        Debug.Log("Enemy is within bounds " + result);
        // Check if the position is within the bounds
        return result;
    }
}