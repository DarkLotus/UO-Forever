using System;
using Server.Targeting;
using Server.Network;
using Server.Mobiles;
using Server.Items;
using Server.Engines.XmlSpawner2;

namespace Server.SkillHandlers
{
	public class Provocation
	{
		public static void Initialize()
		{
			SkillInfo.Table[(int)SkillName.Provocation].Callback = new SkillUseCallback( OnUse );
		}

		public static TimeSpan OnUse( Mobile m )
		{
			m.RevealingAction();

			BaseInstrument.PickInstrument( m, new InstrumentPickedCallback( OnPickedInstrument ) );

			return TimeSpan.FromSeconds( 1.2 ); // Cannot use another skill for 1 second
		}

		public static void OnPickedInstrument( Mobile from, BaseInstrument instrument )
		{
			from.RevealingAction();
			from.SendLocalizedMessage( 501587 ); // Whom do you wish to incite?
			from.Target = new InternalFirstTarget( from, instrument );
		}

		private class InternalFirstTarget : Target
		{
			private BaseInstrument m_Instrument;

			public InternalFirstTarget( Mobile from, BaseInstrument instrument ) : base( BaseInstrument.GetBardRange( from, SkillName.Provocation ), false, TargetFlags.None )
			{
				m_Instrument = instrument;
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				from.RevealingAction();

                IEntity entity = targeted as IEntity; if (XmlScript.HasTrigger(entity, TriggerName.onTargeted) && UberScriptTriggers.Trigger(entity, from, TriggerName.onTargeted, null, null, null, 0, null, SkillName.Provocation, from.Skills[SkillName.Provocation].Value))
                {
                    return;
                }

				if ( targeted is BaseCreature && from.CanBeHarmful( (Mobile)targeted, true ) )
				{
					BaseCreature creature = (BaseCreature)targeted;

					if ( !m_Instrument.IsChildOf( from.Backpack ) )
					{
						from.SendLocalizedMessage( 1062488 ); // The instrument you are trying to play is no longer in your backpack!
					}
					else if ( creature.Controlled )
					{
						from.SendLocalizedMessage( 501590 ); // They are too loyal to their master to be provoked.
					}
					else if ( creature.IsParagon && BaseInstrument.GetBaseDifficulty( creature ) >= 160.0 )
					{
						from.SendLocalizedMessage( 1049446 ); // You have no chance of provoking those creatures.
					}
					else if ( creature.Unprovokable )
					{
						from.SendLocalizedMessage( 1049446 ); // You have no chance of provoking those creatures.
					}
					else
					{
						from.RevealingAction();
						m_Instrument.PlayInstrumentWell( from );
						from.SendLocalizedMessage( 1008085 ); // You play your music and your target becomes angered.  Whom do you wish them to attack?
						from.Target = new InternalSecondTarget( from, m_Instrument, creature );
					}
				}
				else
				{
					from.SendLocalizedMessage( 501589 ); // You can't incite that!
				}
			}
		}

		private class InternalSecondTarget : Target
		{
			private BaseCreature m_Creature;
			private BaseInstrument m_Instrument;

			public InternalSecondTarget( Mobile from, BaseInstrument instrument, BaseCreature creature ) : base( BaseInstrument.GetBardRange( from, SkillName.Provocation ), false, TargetFlags.None )
			{
				m_Instrument = instrument;
				m_Creature = creature;
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				from.RevealingAction();

				if ( targeted is BaseCreature )
				{
					BaseCreature creature = (BaseCreature)targeted;

					if ( !m_Instrument.IsChildOf( from.Backpack ) )
					{
						from.SendLocalizedMessage( 1062488 ); // The instrument you are trying to play is no longer in your backpack!
					}
                    else if (creature.UnprovokableTarget)
					{
						from.SendLocalizedMessage( 1049446 ); // You have no chance of provoking those creatures.
					}
					//else if ( m_Creature.Map != creature.Map || !m_Creature.InRange( creature, BaseInstrument.GetBardRange( from, SkillName.Provocation ) ) )
					else if ( m_Creature.Map != creature.Map || !m_Creature.InRange( creature, m_Creature.RangePerception ) )
					{
						from.SendLocalizedMessage( 1049450 ); // The creatures you are trying to provoke are too far away from each other for your music to have an effect.
					}
					else if ( m_Creature != creature )
					{
						from.NextSkillTime = DateTime.UtcNow + TimeSpan.FromSeconds( 7.0 );

						double diff = ((m_Instrument.GetDifficultyFor( m_Creature ) + m_Instrument.GetDifficultyFor( creature )) * 0.35) - 5.0;
						double music = from.Skills[SkillName.Musicianship].Value;

						if ( music > 100.0 )
							diff -= (music - 100.0) * 0.5;

						if ( from.CanBeHarmful( m_Creature, true ) && from.CanBeHarmful( creature, true ) )
						{
							if ( !BaseInstrument.CheckMusicianship( from ) )
							{
								from.NextSkillTime = DateTime.UtcNow + TimeSpan.FromSeconds( 5.05);
								from.SendLocalizedMessage( 500612 ); // You play poorly, and there is no effect.
								m_Instrument.PlayInstrumentBadly( from );
								m_Instrument.ConsumeUse( from );
							}
							else
							{
								from.DoHarmful( m_Creature, true );
								from.DoHarmful( creature, true );

								if ( !from.CheckTargetSkill( SkillName.Provocation, creature, diff-25.0, diff+25.0 ) )
								{
									from.NextSkillTime = DateTime.UtcNow + TimeSpan.FromSeconds( 5.05 );
									from.SendLocalizedMessage( 501599 ); // Your music fails to incite enough anger.
									m_Instrument.PlayInstrumentBadly( from );
									m_Instrument.ConsumeUse( from );
								}
								else
								{
									from.SendLocalizedMessage( 501602 ); // Your music succeeds, as you start a fight.
									m_Instrument.PlayInstrumentWell( from );
									m_Instrument.ConsumeUse( from );
									m_Creature.Provoke( from, creature, true );
								}
							}
						}
					}
					else
					{
						from.SendLocalizedMessage( 501593 ); // You can't tell someone to attack themselves!
					}
				}
				//allow for targetting a player
				else if( targeted is PlayerMobile )
				{
					PlayerMobile player = (PlayerMobile)targeted;

					if ( m_Creature.Unprovokable )
					{
						from.SendLocalizedMessage( 1049446 ); // You have no chance of provoking those creatures.
					}
					else if ( m_Creature.Map != player.Map || 
						!m_Creature.InRange( player, BaseInstrument.GetBardRange( from, SkillName.Provocation ) ) )
					{
						from.SendLocalizedMessage( 1049450 ); // The creatures you are trying to provoke are too far away from each other for your music to have an effect.
						from.Target = new InternalFirstTarget( from, m_Instrument );
					}
					else //valid pair
					{
						from.NextSkillTime = DateTime.UtcNow + TimeSpan.FromSeconds( 10.0 );      //Appropriate skill delay for player provoke
						
						double diff = ((m_Instrument.GetDifficultyFor( m_Creature ) + m_Instrument.GetDifficultyFor( player )) * 0.5) - 5.0;
						double music = from.Skills[SkillName.Musicianship].Value;

						if ( music > 100.0 )
							diff -= (music - 100.0) * 0.5;

						if ( from.CanBeHarmful( m_Creature, true ) && from.CanBeHarmful( player, true ) )
						{
							if ( !BaseInstrument.CheckMusicianship( from ) )
							{
								from.SendLocalizedMessage( 500612 ); // You play poorly, and there is no effect.
								m_Instrument.PlayInstrumentBadly( from );
								m_Instrument.ConsumeUse( from );
							}
							else
							{
								from.DoHarmful( m_Creature );
								from.DoHarmful( player );

								if ( !from.CheckTargetSkill( SkillName.Provocation, player, diff-25.0, diff+25.0 ) )
								{
									from.SendLocalizedMessage( 501599 ); // Your music fails to incite enough anger.
									m_Instrument.PlayInstrumentBadly( from );
									m_Instrument.ConsumeUse( from );
								}
								else
								{
									from.SendLocalizedMessage( 501602 ); // Your music succeeds, as you start a fight.
									m_Instrument.PlayInstrumentWell( from );
									m_Instrument.ConsumeUse( from );
									m_Creature.Provoke( from, player, true );
								}
							}
						}
					}
				}//end of else (target is player)
				
				else
				{
					from.SendLocalizedMessage( 501589 ); // You can't incite that!
				}
			}
		}
	}
}