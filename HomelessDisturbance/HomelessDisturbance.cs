using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using FivePD.API;
using static CitizenFX.Core.BaseScript;
using static CitizenFX.Core.Debug;
using static CitizenFX.Core.UI.Screen;
using static CitizenFX.Core.Native.API;
using static CitizenFX.Core.World;
using Utilities = HomelessDisturbance.Utils.Utilities;

namespace HomelessDisturbance
{
    [CalloutProperties("Homeless Disturbance", "Dennis Smink", "1.0.0")]
    
    public class HomelessDisturbance : Callout
    {
        // The array for the homeless people
        Ped[] homelessPeople;
        Ped homelessLeader;

        // The total of homelesss people
        private int numberOfHomelessPeople;
        
        // Some skins I have personally selected
        PedHash[] homelessPeopleSkins =
        {
            PedHash.Trucker01SMM,
            PedHash.OldMan1a,
            PedHash.OldMan2,
            PedHash.Rurmeth01AFY,
            PedHash.Rurmeth01AMM,
            PedHash.Soucent02AMO,
            PedHash.Soucent03AMO,
            PedHash.Tramp01,
            PedHash.Tramp01AMM,
            PedHash.Tramp01AMO,
            PedHash.TrampBeac01AFM,
            PedHash.TrampBeac01AMM,
            PedHash.Salton01AMY,
        };
        
        Random random = new Random();

        public HomelessDisturbance()
        {
            float offsetX = random.Next(100, 200);
            float offsetY = random.Next(100, 200);
            
            InitInfo(World.GetNextPositionOnStreet(
                Game.PlayerPed.GetOffsetPosition(new Vector3(offsetX, offsetY, 0))
            ));

            ShortName = "Homeless Disturbance";
            CalloutDescription = "This is a callout for homeless disturbance";
            ResponseCode = 1;
            StartDistance = 120f;
        }

        public override async Task OnAccept()
        {
            InitBlip();

            // Create a random number of homeless people
            numberOfHomelessPeople = random.Next(3, 6);
            // Tell the variable how much we got
            homelessPeople = new Ped[numberOfHomelessPeople];

            // Throw out a few notifications
            ShowNotification($"We've received a call for around {numberOfHomelessPeople} homeless people disturbing the piece.");
            ShowNotification("Head over there, try to identify the leaders & bring them in.");
            
            // Create the homeless leader
            homelessLeader = await SpawnPed(homelessPeopleSkins[random.Next(0, homelessPeopleSkins.Length)], Location);
            homelessLeader.AlwaysKeepTask = true;
            homelessLeader.BlockPermanentEvents = true;  
            
            // Create the peds
            for (int i = 0; i < numberOfHomelessPeople; i++)
            {
                var homelessLocation = Location;

                if (Utilities.RandomBool)
                {
                    homelessLocation -= Utilities.RANDOM.Next(1,10);
                }
                
                homelessPeople[i] = await SpawnPed(homelessPeopleSkins[random.Next(0, homelessPeopleSkins.Length)], homelessLocation);
                homelessPeople[i].AlwaysKeepTask = true;
                homelessPeople[i].BlockPermanentEvents = true;
            }
        }

        public override void OnStart(Ped player)
        {
            base.OnStart(player);
            
            homelessLeader.Task.ReactAndFlee(homelessLeader);
            homelessLeader.AttachBlip();
                        
            ShowSubtitle("Shit, the cops! I'm out of here!", 7500);

            // Loop through the homelessPeople, randomize their outcomes
            foreach (Ped homeboy in homelessPeople)
            {
                int chance = random.Next(1, 5);

                switch(chance) 
                {
                    // Case 1: wander around with a flare
                    case 1:
                        homeboy.Task.WanderAround();
                        homeboy.Task.PlayAnimation("move_drunk_m", "walk");

                        // If true, then give a flare :)
                        if (Utilities.RandomBool)
                        {
                            homeboy.Weapons.Give(WeaponHash.Flare, 1, true, true);
                        }

                        break;

                    // Case 3: Make the ped duck (Does not work for some reason)
                    case 3:
                        
                        homeboy.Task.PlayAnimation("move_duck_for_cover", "loop");
                        //SetPedDucking(homeboy.Handle, true);
                        
                        break;
                    
                    // Case 4: Make the ped have their hands up
                    case 4:
                        homeboy.Task.HandsUp(-1);

                        break;
                    
                    // Default revert to wandering around with nothing special
                    default:
                        homeboy.Task.WanderAround();
                        break;
                }
            }
        }
    }
}