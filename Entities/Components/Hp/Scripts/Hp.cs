using Godot;

namespace Test.Entities.Components;

public partial class Hp : Node {
    [Export] public float HitPoints = 100;

    public void TakeDamage(float damage) {
        HitPoints -= damage;
        GD.Print("Took {0:F} damage, current hp: {0:N1}", damage, HitPoints);
    }
}
