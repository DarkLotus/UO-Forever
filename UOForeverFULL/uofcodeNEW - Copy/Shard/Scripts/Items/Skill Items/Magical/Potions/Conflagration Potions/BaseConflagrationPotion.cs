#region References
using System;
using System.Collections.Generic;

using Server.Spells;
using Server.Targeting;
#endregion

namespace Server.Items
{
	public abstract class BaseConflagrationPotion : BasePotion
	{
		public abstract int MinDamage { get; }
		public abstract int MaxDamage { get; }

		public override bool RequireFreeHand { get { return false; } }

		public BaseConflagrationPotion(PotionEffect effect)
			: base(0xF06, effect)
		{
			Hue = 0x489;
		}

		public BaseConflagrationPotion(Serial serial)
			: base(serial)
		{ }

		public override bool Drink(Mobile from)
		{
			if (EraAOS && (from.Paralyzed || from.Frozen || (from.Spell != null && from.Spell.IsCasting)))
			{
				from.SendLocalizedMessage(1062725); // You can not use that potion while paralyzed.
				return false;
			}

			int delay = GetDelay(from);

			if (delay > 0)
			{
				from.SendLocalizedMessage(1072529, String.Format("{0}\t{1}", delay, delay > 1 ? "seconds." : "second."));
					// You cannot use that for another ~1_NUM~ ~2_TIMEUNITS~
				return false;
			}

			var targ = from.Target as ThrowTarget;

			if (targ != null && targ.Potion == this)
			{
				return false;
			}

			from.RevealingAction();

			if (!m_Users.Contains(from))
			{
				m_Users.Add(from);
			}

			from.Target = new ThrowTarget(this);
			return true;
		}

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

		private readonly List<Mobile> m_Users = new List<Mobile>();

		public void Explode_Callback(IEntity[] states)
		{
			Explode((Mobile)states[0], states[1].Location, states[1].Map);
		}

		public virtual void Explode(Mobile from, Point3D loc, Map map)
		{
			if (Deleted || map == null)
			{
				return;
			}

			Consume();

			// Check if any other players are using this potion
			for (int i = 0; i < m_Users.Count; i ++)
			{
				var targ = m_Users[i].Target as ThrowTarget;

				if (targ != null && targ.Potion == this)
				{
					Target.Cancel(from);
				}
			}

			// Effects
			Effects.PlaySound(loc, map, 0x20C);

			for (int i = -2; i <= 2; i ++)
			{
				for (int j = -2; j <= 2; j ++)
				{
					var p = new Point3D(loc.X + i, loc.Y + j, loc.Z);

					if (map.CanFit(p, 12, true, false) && from.InLOS(p))
					{
						new InternalItem(from, p, map, MinDamage, MaxDamage);
					}
				}
			}
		}

		#region Delay
		private static readonly Dictionary<Mobile, Timer> m_Delay = new Dictionary<Mobile, Timer>();

		public static void AddDelay(Mobile m)
		{
			Timer timer;
			m_Delay.TryGetValue(m, out timer);

			if (timer != null)
			{
				timer.Stop();
			}

			m_Delay[m] = Timer.DelayCall(TimeSpan.FromSeconds(30), EndDelay, m);
		}

		public static int GetDelay(Mobile m)
		{
			Timer timer;
			m_Delay.TryGetValue(m, out timer);

			if (timer != null && timer.Next > DateTime.UtcNow)
			{
				return (int)(timer.Next - DateTime.UtcNow).TotalSeconds;
			}

			return 0;
		}

		public static void EndDelay(Mobile m)
		{
			Timer timer;
			m_Delay.TryGetValue(m, out timer);

			if (timer != null)
			{
				timer.Stop();
				m_Delay.Remove(m);
			}
		}
		#endregion

		private class ThrowTarget : Target
		{
			private readonly BaseConflagrationPotion m_Potion;

			public BaseConflagrationPotion Potion { get { return m_Potion; } }

			public ThrowTarget(BaseConflagrationPotion potion)
				: base(12, true, TargetFlags.None)
			{
				m_Potion = potion;
			}

			protected override void OnTarget(Mobile from, object targeted)
			{
				if (m_Potion.Deleted || m_Potion.Map == Map.Internal)
				{
					return;
				}

				var p = targeted as IPoint3D;

				if (p == null || from.Map == null)
				{
					return;
				}

				// Add delay
				AddDelay(from);

				SpellHelper.GetSurfaceTop(ref p);

				from.RevealingAction();

				IEntity to = new Entity(Serial.Zero, new Point3D(p), from.Map);

				Effects.SendMovingEffect(
					from, p is Mobile ? (Mobile)p : to, 0xF0D, 7, 0, false, false, Math.Min(m_Potion.Hue - 1, 0), 0);
				Timer.DelayCall(TimeSpan.FromSeconds(1.5), m_Potion.Explode_Callback, new[] {from, to});
			}
		}

		public class InternalItem : Item
		{
			private Mobile m_From;
			private int m_MinDamage;
			private int m_MaxDamage;
			private DateTime m_End;
			private Timer m_Timer;

			public Mobile From { get { return m_From; } }

			public override bool BlocksFit { get { return true; } }

			public InternalItem(Mobile from, Point3D loc, Map map, int min, int max)
				: base(0x398C)
			{
				Movable = false;
				Light = LightType.Circle300;

				MoveToWorld(loc, map);

				m_From = from;
				m_End = DateTime.UtcNow + TimeSpan.FromSeconds(10);

				SetDamage(min, max);

				m_Timer = new InternalTimer(this, m_End);
				m_Timer.Start();
			}

			public override void OnAfterDelete()
			{
				base.OnAfterDelete();

				if (m_Timer != null)
				{
					m_Timer.Stop();
				}
			}

			public InternalItem(Serial serial)
				: base(serial)
			{ }

			public int GetDamage()
			{
				return Utility.RandomMinMax(m_MinDamage, m_MaxDamage);
			}

			private void SetDamage(int min, int max)
			{
				/* 	new way to apply alchemy bonus according to Stratics' calculator.
					this gives a mean to values 25, 50, 75 and 100. Stratics' calculator is outdated.
					Those goals will give 2 to alchemy bonus. It's not really OSI-like but it's an approximation. */

				m_MinDamage = min;
				m_MaxDamage = max;

				if (m_From == null)
				{
					return;
				}

				int alchemySkill = m_From.Skills.Alchemy.Fixed;
				int alchemyBonus = alchemySkill / 125 + alchemySkill / 250;

				m_MinDamage = Scale(m_From, m_MinDamage + alchemyBonus);
				m_MaxDamage = Scale(m_From, m_MaxDamage + alchemyBonus);
			}

			public override void Serialize(GenericWriter writer)
			{
				base.Serialize(writer);

				writer.Write(0); // version

				writer.Write(m_From);
				writer.Write(m_End);
				writer.Write(m_MinDamage);
				writer.Write(m_MaxDamage);
			}

			public override void Deserialize(GenericReader reader)
			{
				base.Deserialize(reader);

				int version = reader.ReadInt();

				m_From = reader.ReadMobile();
				m_End = reader.ReadDateTime();
				m_MinDamage = reader.ReadInt();
				m_MaxDamage = reader.ReadInt();

				m_Timer = new InternalTimer(this, m_End);
				m_Timer.Start();
			}

			public override bool OnMoveOver(Mobile m)
			{
				if (Visible && m_From != null && (!EraAOS || m != m_From) && SpellHelper.ValidIndirectTarget(m_From, m) &&
					m_From.CanBeHarmful(m, false))
				{
					m_From.DoHarmful(m);

					m.Damage(GetDamage(), m_From);
					m.PlaySound(0x208);
				}

				return true;
			}

			private class InternalTimer : Timer
			{
				private readonly InternalItem m_Item;
				private readonly DateTime m_End;

				public InternalTimer(InternalItem item, DateTime end)
					: base(TimeSpan.Zero, TimeSpan.FromSeconds(1.0))
				{
					m_Item = item;
					m_End = end;

					Priority = TimerPriority.FiftyMS;
				}

				protected override void OnTick()
				{
					if (m_Item.Deleted)
					{
						return;
					}

					if (DateTime.UtcNow > m_End)
					{
						m_Item.Delete();
						Stop();
						return;
					}

					Mobile from = m_Item.From;

					if (m_Item.Map == null || from == null)
					{
						return;
					}

					var mobiles = new List<Mobile>();

					foreach (Mobile mobile in m_Item.GetMobilesInRange(0))
					{
						mobiles.Add(mobile);
					}

					for (int i = 0; i < mobiles.Count; i++)
					{
						Mobile m = mobiles[i];

						if ((m.Z + 16) > m_Item.Z && (m_Item.Z + 12) > m.Z && (!m_Item.EraAOS || m != from) &&
							SpellHelper.ValidIndirectTarget(from, m) && from.CanBeHarmful(m, false))
						{
							from.DoHarmful(m);

							m.Damage(m_Item.GetDamage(), from);
							m.PlaySound(0x208);
						}
					}
				}
			}
		}
	}
}