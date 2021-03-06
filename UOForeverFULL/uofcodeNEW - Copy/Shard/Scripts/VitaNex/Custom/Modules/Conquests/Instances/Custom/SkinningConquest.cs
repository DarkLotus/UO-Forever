﻿#region References
using System;

using Server.Items;

using VitaNex;
#endregion

namespace Server.Engines.Conquests
{
	public class SkinningConquest : Conquest
	{
		public override string DefCategory { get { return "Crafting/Skinning"; } }

		public virtual Type DefCreature { get { return null; } }
		public virtual bool DefChildren { get { return true; } }
		public virtual bool DefChangeCreatureReset { get { return false; } }

		[CommandProperty(Conquests.Access)]
		public CreatureTypeSelectProperty Creature { get; set; }

		[CommandProperty(Conquests.Access)]
		public bool Children { get; set; }

		[CommandProperty(Conquests.Access)]
		public bool ChangeCreatureReset { get; set; }

		public SkinningConquest()
		{ }

		public SkinningConquest(GenericReader reader)
			: base(reader)
		{ }

		public override void EnsureDefaults()
		{
			base.EnsureDefaults();

			Creature = DefCreature;
			Children = DefChildren;
			ChangeCreatureReset = DefChangeCreatureReset;
		}

		public override sealed int GetProgress(ConquestState state, object args)
		{
			return GetProgress(state, args as Corpse);
		}

		protected virtual int GetProgress(ConquestState state, Corpse corpse)
		{
			if (corpse == null)
			{
				return 0;
			}

            if (state.User == null)
                return 0;

			if (Creature.IsNotNull && !corpse.Owner.TypeEquals(Creature, Children))
			{
				if (ChangeCreatureReset)
				{
					return -state.Progress;
				}

				return 0;
			}

			return 1;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			int version = writer.SetVersion(0);

			switch (version)
			{
				case 0:
					{
						writer.WriteType(Creature);
						writer.Write(Children);
						writer.Write(ChangeCreatureReset);
					}
					break;
			}
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.GetVersion();

			switch (version)
			{
				case 0:
					{
						Creature = reader.ReadType();
						Children = reader.ReadBool();
						ChangeCreatureReset = reader.ReadBool();
					}
					break;
			}
		}
	}
}