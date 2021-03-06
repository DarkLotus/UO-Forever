#region References

using Server.Items;

#endregion

namespace Server.Mobiles
{
    [CorpseName("a gargoyle corpse")]
    public class GargoyleEnforcer : BaseCreature
    {
        public override WeaponAbility GetWeaponAbility()
        {
            return WeaponAbility.WhirlwindAttack;
        }

        public override string DefaultName { get { return "a gargoyle enforcer"; } }

        [Constructable]
        public GargoyleEnforcer() : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Body = 0x2F2;
            BaseSoundID = 0x174;

            Alignment = Alignment.Inhuman;

            SetStr(760, 850);
            SetDex(102, 150);
            SetInt(152, 200);

            SetHits(482, 485);

            SetDamage(7, 14);


            SetSkill(SkillName.MagicResist, 120.1, 130.0);
            SetSkill(SkillName.Tactics, 70.1, 80.0);
            SetSkill(SkillName.Wrestling, 80.1, 90.0);
            SetSkill(SkillName.Swords, 80.1, 90.0);
            SetSkill(SkillName.Anatomy, 70.1, 80.0);
            SetSkill(SkillName.Magery, 80.1, 90.0);
            SetSkill(SkillName.EvalInt, 70.3, 100.0);
            SetSkill(SkillName.Meditation, 70.3, 100.0);

            Fame = 5000;
            Karma = -5000;

            VirtualArmor = 50;

            if (0.05 > Utility.RandomDouble())
            {
                PackItem(new GargoylesPickaxe());
            }
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Rich);
            AddLoot(LootPack.MedScrolls);
        }

        public override int Meat { get { return 1; } }

        public GargoyleEnforcer(Serial serial) : base(serial)
        {}

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