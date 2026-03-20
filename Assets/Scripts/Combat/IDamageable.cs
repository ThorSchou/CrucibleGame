// Shared contract for anything that can receive damage.
public interface IDamageable
{
    void TakeDamage(int damage, int direction);
}