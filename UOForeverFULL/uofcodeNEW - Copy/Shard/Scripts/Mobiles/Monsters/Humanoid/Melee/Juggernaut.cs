#region References
using System;

using Server.Items;
using Server.Network;
#endregion

namespace Server.Mobiles
{
	[CorpseName("a juggernaut corpse")]
	public class Juggernaut : BaseCreature
	{
		private bool m_Stunning;
		public override string DefaultName { get { return "a blackthorn juggernaut"; } }

		[Constructable]
		public Juggernaut()
			: base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.3, 0.6)
		{
			Body = 768;

			SetStr(301, 400);
			SetDex(51, 70);
			SetInt(51, 100);

			SetHits(181, 240);

			SetDamage(12, 19);

			SetSkill(SkillName.Anatomy, 90.1, 100.0);
			SetSkill(SkillName.MagicResist, 140.1, 150.0);
			SetSkill(SkillName.Tactics, 90.1, 100.0);
			SetSkill(SkillName.Wrestling, 90.1, 100.0);

			Fame = 12000;
			Karma = -12000;

			VirtualArmor = 70;

			if (0.1 > Utility.RandomDouble())
			{
				PackItem(new PowerCrystal());
			}

			if (0.4 > Utility.RandomDouble())
			{
				PackItem(new ClockworkAssembly());
			}
		}

		public override void OnDeath(Container c)
		{
			base.OnDeath(c);

			if (0.05 > Utility.RandomDouble())
			{
				if (!IsParagon)
				{
					if (0.75 > Utility.RandomDouble())
					{
						c.DropItem(DawnsMusicGear.RandomCommon);
					}
					else
					{
						c.DropItem(DawnsMusicGear.RandomUncommon);
					}
				}
				else
				{
					c.DropItem(DawnsMusicGear.RandomRare);
				}
			}
		}

		public override void GenerateLoot()
		{
			AddLoot(LootPack.Rich);
			AddLoot(LootPack.Gems, 1);
		}

		public override int GetDeathSound()
		{
			return 0x423;
		}

		public override int GetAttackSound()
		{
			return 0x23B;
		}

		public override int GetHurtSound()
		{
			return 0x140;
		}

		public override bool AlwaysMurderer { get { return true; } }
		public override bool BardImmune { get { return !EraAOS; } }
		public override bool BleedImmune { get { return true; } }
		public override Poison PoisonImmune { get { return Poison.Lethal; } }
		public override int Meat { get { return 1; } }
		public override int TreasureMapLevel { get { return 5; } }

		public override void OnGaveMeleeAttack(Mobile defender)
		{
			base.OnGaveMeleeAttack(defender);

			if (!m_Stunning && 0.3 > Utility.RandomDouble())
			{
				m_Stunning = true;

				defender.Animate(21, 6, 1, true, false, 0);
				PlaySound(0xEE);
				defender.LocalOverheadMessage(MessageType.Regular, 0x3B2, false, "You have been stunned by a colossal blow!");

				var weapon = Weapon as BaseWeapon;
				if (weapon != null)
				{
					weapon.OnHit(this, defender);
				}

				if (defender.Alive)
				{
					defender.Frozen = true;
					Timer.DelayCall(TimeSpan.FromSeconds(5.0), Recover_Callback, defender);
				}
			}
		}

		private void Recover_Callback(Mobile defender)
		{
			defender.Frozen = false;
			defender.Combatant = null;
			defender.LocalOverheadMessage(MessageType.Regular, 0x3B2, false, "You recover your senses.");

			m_Stunning = false;
		}

		public Juggernaut(Serial serial)
			: base(serial)
		{ }

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}
}