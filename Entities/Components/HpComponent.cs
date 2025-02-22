using Godot;

namespace Test.Scripts.Components;

public partial class HpComponent : Node3D
{
	[Export] public float Hp = 100;

	public void TakeDamage(float damage)
	{
		Hp -= damage;
		GD.Print("Took {0:F} damage, current hp: {0:N1}", damage, Hp);
	}
}
