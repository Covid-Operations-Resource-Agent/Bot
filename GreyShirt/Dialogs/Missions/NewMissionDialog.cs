﻿using Greyshirt.State;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Extensions.Configuration;
using Shared;
using Shared.ApiInterface;
using System.Threading;
using System.Threading.Tasks;

namespace Greyshirt.Dialogs.Missions
{
    public class NewMissionDialog : DialogBase
    {
        public static string Name = typeof(NewMissionDialog).FullName;

        public NewMissionDialog(StateAccessors state, DialogSet dialogs, IApiInterface api, IConfiguration configuration, MultiLanguageLG lgGenerator)
            : base(state, dialogs, api, configuration, lgGenerator) { }

        public override Task<WaterfallDialog> GetWaterfallDialog(ITurnContext turnContext, CancellationToken cancellation)
        {
            return Task.Run(() =>
            {
                return new WaterfallDialog(Name, new WaterfallStep[]
                {
                    async (dialogContext, cancellationToken) =>
                    {
                        var greyshirt = await this.api.GetGreyshirtFromContext(dialogContext.Context);
                        var userContext = await this.state.GetUserContext(turnContext, cancellationToken);

                        // TODO: this could be configurable.
                        double requestMeters = Units.Miles.ToMeters(50);

                        // Get all users within distance.
                        var usersWithinDistance = await this.api.GetUsersWithinDistance(greyshirt.LocationCoordinates, requestMeters);
                        if (usersWithinDistance.Count > 0)
                        {
                            // Get any missions 
                            foreach (var userWithinDistance in usersWithinDistance)
                            {
                                var missions = await this.api.GetMissionsCreatedByUser(userWithinDistance, isAssigned: false);

                                foreach (var mission in missions)
                                {
                                    userContext.Matches.Add(new Match { PhoneNumber = userWithinDistance.PhoneNumber, MissionId = mission.Id });
                                }
                            }

                            // If there were matches, present them to the user.
                            if (userContext.Matches.Count > 0)
                            {
                                await Messages.SendAsync(Phrases.Match.NumMissions(userContext.Matches.Count), turnContext, cancellationToken);
                                return await BeginDialogAsync(dialogContext, MatchDialog.Name, null, cancellationToken);
                            }

                            await Messages.SendAsync(Phrases.Match.None, turnContext, cancellationToken);
                            return await dialogContext.NextAsync(null, cancellationToken);
                        }

                        return await dialogContext.NextAsync(null, cancellationToken);
                    },
                    async (dialogContext, cancellationToken) =>
                    {
                        return await dialogContext.EndDialogAsync(null, cancellationToken);
                    }
                });
            });
        }
    }
}
