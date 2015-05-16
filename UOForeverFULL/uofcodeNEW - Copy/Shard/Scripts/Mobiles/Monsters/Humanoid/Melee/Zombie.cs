#region References

using Server.Items;

#endregion

namespace Server.Mobiles
{
    [CorpseName("a rotting corpse")]
    public class Zombie : BaseCreature
    {
        public override string DefaultName { get { return "a zombie"; } }

        [Constructable]
        public Zombie() : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Body = 3;
            BaseSoundID = 471;

            Alignment = Alignment.Undead;

            SetStr(46, 70);
            SetDex(31, 50);
            SetInt(26, 40);

            SetHits(28, 42);

            SetDamage(3, 7);


            SetSkill(SkillName.MagicResist, 15.1, 40.0);
            SetSkill(SkillName.Tactics, 35.1, 50.0);
            SetSkill(SkillName.Wrestling, 35.1, 50.0);

            Fame = 600;
            Karma = -600;

            VirtualArmor = 18;

            switch (Utility.Random(10))
            {
                case 0:
                    PackItem(new LeftArm());
                    break;
                case 1:
                    PackItem(new RightArm());
                    break;
                case 2:
                    PackItem(new Torso());
                    break;
                case 3:
                    PackItem(new Bone());
                    break;
                case 4:
                    PackItem(new RibCage());
                    break;
                case 5:
                    PackItem(new RibCage());
                    break;
                case 6:
                    PackItem(new BonePile());
                    break;
                case 7:
                    PackItem(new BonePile());
                    break;
                case 8:
                    PackItem(new BonePile());
                    break;
                case 9:
                    PackItem(new BonePile());
                    break;
            }
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Meager);
        }

        public override bool BleedImmune { get { return true; } }
        public override Poison PoisonImmune { get { return Poison.Regular; } }
        public override int DefaultBloodHue { get { return 1438; } }

        public Zombie(Serial serial) : base(serial)
        {}

        public override OppositionGroup OppositionGroup { get { return OppositionGroup.FeyAndUndead; } }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int) 0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}