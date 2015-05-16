using System;
using Server;
using Server.Items;

namespace Server.Mobiles
{
	public class Samurai : BaseCreature
	{
        public override bool CanTeach { get { return false;  } }
		public override bool ClickTitle{ get{ return false; } }

		[Constructable]
		public Samurai() : base( AIType.AI_Melee, FightMode.Aggressor, 10, 1, 0.2, 0.4 )
		{
			Title = "the samurai";

			InitStats( 100, 100, 25 );

			SetSkill( SkillName.ArmsLore, 64.0, 80.0 );
			SetSkill( SkillName.Parry, 64.0, 80.0 );
			SetSkill( SkillName.Swords, 64.0, 85.0 );

			SpeechHue = Utility.RandomDyedHue();

			Hue = Utility.RandomSkinHue();

			if ( Female = Utility.RandomBool() )
			{
				Body = 0x191;
				Name = NameList.RandomName( "female" );
			}
			else
			{
				Body = 0x190;
				Name = NameList.RandomName( "male" );
			}

			switch ( Utility.Random( 3 ) )
			{
				case 0:	AddItem( NotCorpseCont(( new Lajatang()))) ;	break;
				case 1:	AddItem( NotCorpseCont(( new Wakizashi()))) ;	break;
				case 2:	AddItem( NotCorpseCont(( new NoDachi() )));	break;
			}

			switch ( Utility.Random( 3 ) )
			{
				case 0:	AddItem( NotCorpseCont(( new LeatherSuneate()) ));	break;
				case 1:	AddItem( NotCorpseCont(( new PlateSuneate() )));		break;
                case 2: AddItem(NotCorpseCont((new StuddedHaidate()))); break;
			}

			switch ( Utility.Random( 4 ) )
			{
				case 0:	AddItem( NotCorpseCont(( new LeatherJingasa() )));		break;
				case 1:	AddItem( NotCorpseCont(( new ChainHatsuburi() )));		break;
				case 2:	AddItem( NotCorpseCont(( new HeavyPlateJingasa()) ));		break;
				case 3:	AddItem( NotCorpseCont(( new DecorativePlateKabuto()) ));	break;
			}

			AddItem( NotCorpseCont(( new LeatherDo()) ));
			AddItem( NotCorpseCont(( new LeatherHiroSode()) ));
			AddItem( NotCorpseCont(( new SamuraiTabi( Utility.RandomNondyedHue()) ) )); // TODO: Hue

			int hairHue = Utility.RandomNondyedHue();


			Utility.AssignRandomHair( this, hairHue );

			if( Utility.Random( 7 ) != 0 )
				Utility.AssignRandomFacialHair( this, hairHue );

			PackGold( 250, 300 );
		}

		public Samurai( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.WriteEncodedInt( 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadEncodedInt();
		}
	}
}