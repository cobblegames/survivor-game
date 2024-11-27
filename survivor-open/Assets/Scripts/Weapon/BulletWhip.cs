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
        Debug.Log("Enemy Is damaged");
        float halfWidth = squareHitbox.x / 2;
        float halfHeight = squareHitbox.y / 2;

        // Calculate bounds of the rectangle
        float leftBound = squareHitbox.x - halfWidth;
        float rightBound = squareHitbox.x + halfWidth;
        float bottomBound = squareHitbox.y - halfHeight;
        float topBound = squareHitbox.y + halfHeight;

        // Check if the position is within the bounds
        return _enemy.transform.position.x >= leftBound && _enemy.transform.position.x <= rightBound &&
        _enemy.transform.position.y >= bottomBound && _enemy.transform.position.y <= topBound;
    }
}