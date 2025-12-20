using System;
using System.Collections.Generic;
using System.Linq;
using Bannerlord.GameMaster.Characters;
using Bannerlord.GameMaster.Information;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace Bannerlord.GameMaster.Heroes
{
	public class HeroGenerator
	{
		#region Character

		/// <summary>
		/// Create a Character randomly choosing between any template with optional face and body randomization<br/>
		/// If randomFactor is greater than 0 (0-1), the template will be randomized based on its min and max values of the template<br/>
		/// </summary>
		/// <param name="randomFactor">0 to 1 how far character properties can be randomized from template (constrained by the template min and max properties. (0 skips randomization)</param>
		/// <param name="clan">Defaults to null. If null, a random clan is selected (Ignored for wanderers)</param>
		/// <param name="occupation">Defaults to Lord. Can be used to create Lords, Wanderers, or others</param>
		public Hero CreateSingleHeroFromRandomTemplates(string name, CultureFlags cultureFlags = CultureFlags.AllMainCultures, GenderFlags genderFlags = GenderFlags.Either, float randomFactor = 0.5f, Clan clan = null, Occupation occupation = Occupation.Lord)
		{
			Hero hero = CreateHeroesFromRandomTemplates(1, cultureFlags, genderFlags, randomFactor, clan)[0];		
			hero.SetStringName(name);

			return hero;
		}

		public Hero CreateRandomWandererAtSettlement(Settlement settlement, CultureFlags cultureFlags = CultureFlags.AllMainCultures, GenderFlags genderFlags = GenderFlags.Either)
		{
			Hero hero = CreateHeroesFromRandomTemplates(1, cultureFlags, genderFlags, 0.5f, occupation: Occupation.Wanderer)[0];
			
			hero.Clan = null;
			hero.IsMinorFactionHero = false;
			EnterSettlementAction.ApplyForCharacterOnly(hero, settlement);
			hero.UpdateLastKnownClosestSettlement(settlement);	

			return hero;
		}

		/// <summary>
		/// Create Characters randomly choosing between any template with optional face and body randomization<br/>
		/// If randomFactor is greater than 0 (0-1), the template will be randomized based on its min and max values of the template<br/>
		/// </summary>
		/// <param name="randomFactor">0 to 1 how far character properties can be randomized from template (constrained by the template min and max properties. (0 skips randomization)</param>
		/// <param name="clan">Defaults to null. If null, a random clan is selected (Ignored for wanderers)</param>
		/// <param name="occupation">Defaults to Lord. Can be used to create Lords, Wanderers, or others</param>
		public List<Hero> CreateHeroesFromRandomTemplates(int Count, CultureFlags cultureFlags = CultureFlags.AllMainCultures, GenderFlags genderFlags = GenderFlags.Either, float randomFactor = 0.5f, Clan clan = null, Occupation occupation = Occupation.Lord)
		{			
			CharacterTemplatePooler templatePooler = new();
			List<CharacterObject> templatePool = templatePooler.GetTemplatesFromFlags(cultureFlags, genderFlags);
					
			Clan[] clans = new Clan[0]; //Used if clan is null so clan IEnumerable isnt interated through each time using AtElement()
			if (clan == null)
				clans = Clan.NonBanditFactions.ToArray();

			List<Hero> heroes = new();
			for (int i = 0; i < Count; i++)
			{
				//Create character from random template
				CharacterObject character;
				int randomTemplateIndex = RandomNumberGen.Instance.NextRandomInt(templatePool.Count);
				character = CharacterObject.CreateFrom(templatePool[randomTemplateIndex]);

				// Randomize face, hair, beard, tattoos, and body if randomFactor greater than 0
				if (randomFactor > 0)
					character = RandomizeCharacterObject(character, randomFactor);

				Clan currentClan = clan; // Makes if hero a different possible clan if clan is null
				currentClan ??= clans[RandomNumberGen.Instance.NextRandomInt(clans.Length)]; // Get random clan if null
				
				TextObject randomNameObj = CultureLookup.GetRandomName(character.Culture, character.IsFemale); // Get random name based on gender and culture
				Hero hero = CreateHero(character, randomNameObj, occupation, currentClan);
				
				heroes.Add(hero);
			}

			return heroes;
		}

		#endregion
		#region Randomize

		public CharacterObject RandomizeCharacterObject(CharacterObject template, float randomFactor, bool useFaceConstraints = true, bool useBuildConstraints = true, bool useHairConstraints = true)
		{
			int _seed = RandomNumberGen.Instance.NextRandomInt();

			// Static = Keys: Face, skin, hair, eye color, face porpotions, base body frame
			// Dynamic = Floats (0f - 1f): Age, Weight, Build
			//BodyProperties currentBodyProperties = template.GetBodyProperties(template.Equipment, seed: _seed);
			BodyProperties minBodyProperties = template.GetBodyPropertiesMin();
			BodyProperties maxBodyProperties = template.GetBodyPropertiesMax();

			int hairCoverType = HairCoveringType.None;
			string hairTags = HairTags.All;
			string beardTags = BeardTags.All;
			string tatooTags = TattooTags.None;

			BodyProperties randomProperties = TaleWorlds.Core.FaceGen.GetRandomBodyProperties(template.Race, template.IsFemale,
					minBodyProperties, maxBodyProperties, hairCoverType, _seed, hairTags, beardTags, tatooTags, randomFactor);


			return template;
		}

		#endregion
		#region Hero

		/// <summary>
		/// Generates a hero from a character (Use CreateCharacter())
		/// <param name="clan">Defaults to null. If null, a random clan is selected</param>
		/// </summary>
		private Hero CreateHero(CharacterObject template, TextObject nameObj, Occupation occupation, Clan clan = null)
		{
			string stringId = ObjectManager.Instance.GetUniqueStringId(nameObj, typeof(Hero));
			int randomAge = RandomNumberGen.Instance.NextRandomInt(20, 31);

			Hero hero = HeroCreator.CreateSpecialHero(template, age: randomAge);
			hero.StringId = stringId;
			hero.SetName(nameObj, nameObj);

			hero.PreferredUpgradeFormation = FormationClass.Cavalry;
			
			hero.Gold = 10000;
			hero.Level = 10;

			// Random clan if null
			if (clan == null)
			{
				int randomIndex = RandomNumberGen.Instance.NextRandomInt(Clan.NonBanditFactions.Count());
				clan = Clan.NonBanditFactions.ElementAt(randomIndex);
			}

			hero.Clan = clan;
			hero.IsMinorFactionHero = false; // Ensures Lord always show as lord and not minor faction (Player is a minor faction until kingdom) // Should also be fine if hero is in an actual minor faction.
			
			// Override for wanderers
			if (occupation == Occupation.Wanderer)
				hero.Clan = null;			

			hero.SetNewOccupation(occupation); // Controls if Lord, wanderer, notable, etc
			hero.UpdateHomeSettlement(); // Has to happen after clan
			Settlement heroSettlement = hero.GetHomeOrAlternativeSettlement(); // Used incase home settlement is null		

			// Lords
			if(hero.Occupation == Occupation.Lord)
			{
				hero.EquipLordBasedOnCulture();

				// Dont auto create party if clan already has 6 or more parties
				if (hero.Clan.WarPartyComponents.Count < 6)
					hero.CreateParty(heroSettlement);
			}

			// Wanderers and other
			else
			{
				hero.EquipHeroBasedOnCulture();
				EnterSettlementAction.ApplyForCharacterOnly(hero, heroSettlement);
			}			

			hero.UpdateLastKnownClosestSettlement(heroSettlement);						
			hero.UpdatePowerModifier();
			
			return hero;
		}

		#endregion
	}
}