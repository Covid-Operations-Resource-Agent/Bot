﻿using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Extensions.Configuration;
using Remy.State;
using Shared;
using Shared.ApiInterface;
using Shared.Models;
using Shared.Prompts;
using Shared.Storage;
using Shared.Translation;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Remy.Dialogs.Need
{
    public class NeedDialog : DialogBase
    {
        public static string Name = typeof(NeedDialog).FullName;

        Translator translator;

        public NeedDialog(StateAccessors state, DialogSet dialogs, IApiInterface api, IConfiguration configuration, MultiLanguageLG lgGenerator)
            : base(state, dialogs, api, configuration, lgGenerator)
        {
            this.translator = new Translator(configuration);
        }

        public override Task<WaterfallDialog> GetWaterfallDialog(ITurnContext turnContext, CancellationToken cancellation)
        {
            return Task.Run(() =>
            {
                return new WaterfallDialog(Name, new WaterfallStep[]
                {
                    async (dialogContext, cancellationToken) =>
                    {
                        return await dialogContext.PromptAsync(
                            Prompt.ConfirmPrompt,
                            new PromptOptions { Prompt = Phrases.Need.GetPrivacyConsent },
                            cancellationToken);
                    },
                    async (dialogContext, cancellationToken) =>
                    {
                        if (!(bool)dialogContext.Result)
                        {
                            await Messages.SendAsync(Phrases.Need.NoConsent, turnContext, cancellationToken);
                            return await dialogContext.EndDialogAsync(null, cancellationToken);
                        }

                        return await dialogContext.PromptAsync(
                            Prompt.TextPrompt,
                            new PromptOptions { Prompt = Phrases.Need.GetNeed },
                            cancellationToken);
                    },
                    async (dialogContext, cancellationToken) =>
                    {
                        var user = await this.api.GetUserFromContext(dialogContext.Context);

                        var mission = new Mission
                        {
                            CreatedById = user.Id,
                            Description = (string)dialogContext.Result
                        };

                        await this.api.Create(mission);

                        // Cache any translations to limit API calls.
                        var translationCache = new Dictionary<string, string>();

                        // TODO: this could be configurable.
                        double requestMeters = Units.Miles.ToMeters(50);

                        // Get all greyshirts within the distance from the user.
                        var greyshirtsWithinDistance = await this.api.GetGreyshirtsWithinDistance(user.LocationCoordinates, requestMeters);
                        if (greyshirtsWithinDistance.Count > 0)
                        {
                            // Use the outgoing queue for the Greyshirt bot.
                            var queueHelper = new OutgoingMessageQueueHelpers(this.configuration.GreyshirtAzureWebJobsStorage());
                            var message = Greyshirt.Phrases.Need.Notification(user.Location, mission);

                            // Get any matching resources for the users.
                            foreach (var greyshirt in greyshirtsWithinDistance)
                            {
                                string translatedMessage = message;

                                // Check if the user's language is already cached.
                                if (translationCache.TryGetValue(user.Language, out var translation))
                                {
                                    translatedMessage = translation;
                                }
                                else
                                {
                                    // Translate the message if necessary.
                                    if (translator.IsConfigured && user.Language != Translator.DefaultLanguage)
                                    {
                                        translatedMessage = await translator.TranslateAsync(message, user.Language);
                                    }

                                    // Cache the message.
                                    translationCache.Add(user.Language, translatedMessage);
                                }

                                var data = new OutgoingMessageQueueData
                                {
                                    PhoneNumber = greyshirt.PhoneNumber,
                                    Message = message
                                };

                                await queueHelper.Enqueue(data);
                            }
                        }

                        await Messages.SendAsync(Phrases.Need.Complete, turnContext, cancellationToken);
                        return await dialogContext.EndDialogAsync(null, cancellationToken);
                    },
                });
            });
        }
    }
}
