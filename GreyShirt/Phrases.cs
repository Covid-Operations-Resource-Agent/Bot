﻿using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Collections.Generic;

namespace Greyshirt
{
    public static class Phrases
    {
        public static class NewUser
        {
            public static Activity RegistrationComplete = MessageFactory.Text("That's all the information I need - you're now ready to take on a mission!");
        }

        public static class Register
        {
            public static Activity GetIsRegistered = MessageFactory.Text($"Are you already registered as a Greyshirt? {Shared.Phrases.EnterNumber}");
            public static Activity HowToRegister = MessageFactory.Text($"No problem, let's get you registered as a Greyshirt." +
                $" Go to TeamRubiconUSA.org/engage. Select Join Up and then follow the onscreen instructions.");
            public static Activity GetNumberNew = MessageFactory.Text($"Once you finish registering, reply to me by sending your new Greyshirt number");
            public static Activity GetNumberNewRepeat = MessageFactory.Text($"What is your Greyshirt number?");
            public static Activity GetNumberExisting = MessageFactory.Text($"That's what I like to hear! What is your Greyshirt number?");
            public static Activity GetNumberConfirm = MessageFactory.Text($"Great! We're almost ready to get started!");
        }

        public static class Options
        {
            public static string NewMission = "I need a mission";
            public static string WhatIsMission = "What's a mission?";
            public static string MoreOptions = "More options";

            public static List<string> List = new List<string> { NewMission, WhatIsMission, MoreOptions };

            public static Activity GetOptions = MessageFactory.Text($"Let me know what you'd like to do. {Shared.Phrases.EnterNumber}");
            public static Activity MissionExplaination = MessageFactory.Text($"Missions are super quick (under an hour), high-impact" +
                $" helpouts. They are opportunities for Greyshirts like you to support people in your community with urgent needs.");
        }

        public static class Match
        {
            public static Activity None = MessageFactory.Text("Unfortunately I don't have any missions near you at this time." +
                " Check back in again soon, and I will also let you know if any new missions pop up near you!");

            public static Activity NoMore = MessageFactory.Text("That's all the missions I have near you at this time." +
                " Check back in again soon, and I will also let you know if any new missions pop up near you!");

            public static string AcceptMission = "I'm on it!";
            public static string DeclineMission = "I'll pass on this one";
            public static List<string> MatchOptions = new List<string> { AcceptMission, DeclineMission };

            public static Activity NumMissions(int num)
            {
                return MessageFactory.Text($"I have {num} {(num == 1 ? "mission" : "missions")} available near you!");
            }

            public static Activity OfferMission(string instructions, string location)
            {
                return MessageFactory.Text($"Here's a mission in {location} - \"{instructions}\". Would you like to accept this mission? {Shared.Phrases.EnterNumber}");
            }

            public static Activity Accepted(string phoneNumber)
            {
                return MessageFactory.Text("That's what I was hoping to hear! This mission is now assigned to you." +
                    $" You can coordinate by calling or texting them at {phoneNumber}." +
                    $" I'll check back in with you periodically to see how it is coming along!");
            }

            public static Activity Another(int remaining)
            {
                return MessageFactory.Text($"Would you like me to send you another mission near you? I have {remaining} more available. {Shared.Phrases.EnterNumber}");
            }
        }

        public static class Need
        {
            public static string Message(string location, string instructions)
            {
                return $"Hey there Greyshirt, a new mission has been received in {location} - \"{instructions}\"." +
                    $" If you would like to accept this mission, reply \"ok\" and then select \"{Options.NewMission}\".";
            }
        }
    }
}
