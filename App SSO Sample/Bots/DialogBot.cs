// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TeamsAuthSSO.Models;
using TeamsTabSSO.Helpers;

namespace Microsoft.BotBuilderSamples
{
    // This IBot implementation can run any type of Dialog. The use of type parameterization is to allows multiple different bots
    // to be run at different endpoints within the same project. This can be achieved by defining distinct Controller types
    // each with dependency on distinct IBot types, this way ASP Dependency Injection can glue everything together without ambiguity.
    // The ConversationState is used by the Dialog system. The UserState isn't, however, it might have been used in a Dialog implementation,
    // and the requirement is that all BotState objects are saved at the end of a turn.
    public class DialogBot<T> : TeamsActivityHandler
        where T : Dialog
    {
        protected readonly ILogger _logger;
        protected readonly BotState _userState;
        protected readonly BotState _conversationState;
        protected readonly Dialog _dialog;
        private readonly string _connectionName;
        private readonly string _siteUrl;
        private readonly IStatePropertyAccessor<string> _userConfigProperty;

        public DialogBot(ConversationState conversationState, UserState userState, T dialog, ILogger<DialogBot<T>> logger, IConfiguration configuration)
        {
            _connectionName = configuration["ConnectionName"] ?? throw new NullReferenceException("ConnectionName");
            _userState = userState ?? throw new NullReferenceException(nameof(userState));
            _conversationState = conversationState ?? throw new NullReferenceException(nameof(conversationState));
            _logger = logger;
            _dialog = dialog;
            _siteUrl = configuration["SiteUrl"] ?? throw new NullReferenceException("SiteUrl");
            _userConfigProperty = userState.CreateProperty<string>("UserConfiguration");
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            try
            {
                await base.OnTurnAsync(turnContext, cancellationToken);

                // After the turn is complete, persist any UserState changes.
                // Save any state changes that might have occurred during the turn.
                await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
                await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            }
            catch (Exception ex)
            {
                //throw;
                Console.Write(ex);
            }
        }
       
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Running dialog with Message Activity.");
            await _dialog.RunAsync(turnContext, _conversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
        }

        protected async override Task<MessagingExtensionResponse> OnTeamsAppBasedLinkQueryAsync(ITurnContext<IInvokeActivity> turnContext, AppBasedLinkQuery query, CancellationToken cancellationToken)
        {
            var tokenResponse = await GetTokenResponse(turnContext, query.State, cancellationToken);
            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.Token))
            {
                // There is no token, so the user has not signed in yet.
                // Retrieve the OAuth Sign in Link to use in the MessagingExtensionResult Suggested Actions
                var userTokenClient = turnContext.TurnState.Get<UserTokenClient>();
                var resource = await userTokenClient.GetSignInResourceAsync(_connectionName, turnContext.Activity as Activity, null, cancellationToken);
                return new MessagingExtensionResponse
                {
                    ComposeExtension = new MessagingExtensionResult
                    {
                        Type = "auth",
                        SuggestedActions = new MessagingExtensionSuggestedAction
                        {
                            Actions = new List<CardAction>
                                {
                                    new CardAction
                                    {
                                        Type = ActionTypes.OpenUrl,
                                        Value = resource.SignInLink,
                                        Title = "Bot Service OAuth",
                                    },
                                },
                        },
                    },
                };
            }

            var client = new SimpleGraphClient(tokenResponse.Token);
            var profile = await client.GetMyProfile();
            var imagelink = await client.GetPhotoAsync();
            var heroCard = new ThumbnailCard
            {
                Title = "Thumbnail Card",
                Text = $"Hello {profile.DisplayName}",
                Images = new List<CardImage> { new CardImage(imagelink) }
            };
            var attachments = new MessagingExtensionAttachment(HeroCard.ContentType, null, heroCard);
            var result = new MessagingExtensionResult("list", "result", new[] { attachments });
            return new MessagingExtensionResponse(result);
        }

        protected override async Task<MessagingExtensionResponse> OnTeamsMessagingExtensionConfigurationQuerySettingUrlAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionQuery query, CancellationToken cancellationToken)
        {
            // The user has requested the Messaging Extension Configuration page.  
            var escapedSettings = string.Empty;
            var userConfigSettings = await _userConfigProperty.GetAsync(turnContext, () => string.Empty);
            if (!string.IsNullOrEmpty(userConfigSettings))
            {
                escapedSettings = Uri.EscapeDataString(userConfigSettings);
            }
            return new MessagingExtensionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    Type = "config",
                    SuggestedActions = new MessagingExtensionSuggestedAction
                    {
                        Actions = new List<CardAction>
                        {
                            new CardAction
                            {
                                Type = ActionTypes.OpenUrl,
                                Value = $"{_siteUrl}/searchSettings.html?settings={escapedSettings}",
                            },
                        },
                    },
                },
            };
        }

        protected override async Task OnTeamsMessagingExtensionConfigurationSettingAsync(ITurnContext<IInvokeActivity> turnContext, JObject settings, CancellationToken cancellationToken)
        {
            // When the user submits the settings page, this event is fired.
            if (settings["state"] != null)
            {
                var userConfigSettings = settings["state"].ToString();
                await _userConfigProperty.SetAsync(turnContext, userConfigSettings, cancellationToken);
            }
        }

        protected override async Task<MessagingExtensionResponse> OnTeamsMessagingExtensionQueryAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionQuery action, CancellationToken cancellationToken)
        {
            var userConfigSettings = await _userConfigProperty.GetAsync(turnContext, () => string.Empty);
            var attachments = new List<MessagingExtensionAttachment>();

            if (userConfigSettings.ToUpper().Contains("EMAIL"))
            {
                var tokenResponse = await GetTokenResponse(turnContext, action.State, cancellationToken);
                if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.Token))
                {
                    // There is no token, so the user has not signed in yet.
                    // Retrieve the OAuth Sign in Link to use in the MessagingExtensionResult Suggested Actions
                    var userTokenClient = turnContext.TurnState.Get<UserTokenClient>();
                    var resource = await userTokenClient.GetSignInResourceAsync(_connectionName, turnContext.Activity as Activity, null, cancellationToken);
                    return new MessagingExtensionResponse
                    {
                        ComposeExtension = new MessagingExtensionResult
                        {
                            Type = "auth",
                            SuggestedActions = new MessagingExtensionSuggestedAction
                            {
                                Actions = new List<CardAction>
                                {
                                    new CardAction
                                    {
                                        Type = ActionTypes.OpenUrl,
                                        Value = resource.SignInLink,
                                        Title = "Bot Service OAuth",
                                    },
                                },
                            },
                        },
                    };
                }
                var client = new SimpleGraphClient(tokenResponse.Token);
                var me = await client.GetMyProfile();
                var imagelink = await client.GetPhotoAsync();
                var previewcard = new ThumbnailCard
                {
                    Title = me.DisplayName,
                    Images = new List<CardImage> { new CardImage { Url = imagelink } }
                };
                var attachment = new MessagingExtensionAttachment
                {
                    ContentType = ThumbnailCard.ContentType,
                    Content = previewcard,
                    Preview = previewcard.ToAttachment()
                };
                attachments.Add(attachment);
            }
            else
            {

                var state = action.State; // Check the state value
                var tokenResponse = await GetTokenResponse(turnContext, state, cancellationToken);
                if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.Token))
                {
                    // There is no token, so the user has not signed in yet.
                    // Retrieve the OAuth Sign in Link to use in the MessagingExtensionResult Suggested Actions
                    var userTokenClient = turnContext.TurnState.Get<UserTokenClient>();
                    var resource = await userTokenClient.GetSignInResourceAsync(_connectionName, turnContext.Activity as Activity, null, cancellationToken);
                    return new MessagingExtensionResponse
                    {
                        ComposeExtension = new MessagingExtensionResult
                        {
                            Type = "auth",
                            SuggestedActions = new MessagingExtensionSuggestedAction
                            {
                                Actions = new List<CardAction>
                                {
                                    new CardAction
                                    {
                                        Type = ActionTypes.OpenUrl,
                                        Value = resource.SignInLink,
                                        Title = "Bot Service OAuth",
                                    },
                                },
                            },
                        },
                    };
                }

                string accessToken = tokenResponse.Token;
                List<(string, string, string, string)> result = await OneDriveHelper.OneDriveTeamPhotosList(accessToken);

                attachments = result.Select(package =>
                {
                    var previewCard = new ThumbnailCard { Title = package.Item2, Tap = new CardAction { Type = "invoke", Value = package } };
                    if (!string.IsNullOrEmpty(package.Item3))
                    {
                        previewCard.Images = new List<CardImage>() { new CardImage(package.Item3, "Icon") };
                    }

                    var attachment = new MessagingExtensionAttachment
                    {
                        ContentType = ThumbnailCard.ContentType,
                        Content = new ThumbnailCard { Title = package.Item2, Images = new List<CardImage>() { new CardImage(package.Item3, "Icon") } },
                        Preview = previewCard.ToAttachment()
                    };

                    return attachment;
                }).ToList();
            }

            return new MessagingExtensionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    Type = "result",
                    AttachmentLayout = "grid",
                    Attachments = attachments
                }
            };
        }


        protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionSubmitActionAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
            // This method is to handle the 'Close' button on the confirmation Task Module after the user signs out.
            switch (action.CommandId)
            {
                case "UPLOADPHOTO":
                    return await UploadPhotoSubmitAsync(turnContext, action, cancellationToken);
            }
            return await Task.FromResult(new MessagingExtensionActionResponse());
        }

        protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionFetchTaskAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
            if (action.CommandId.ToUpper() == "SHOWPROFILE")
            {
                var state = action.State; // Check the state value
                var tokenResponse = await GetTokenResponse(turnContext, state, cancellationToken);
                if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.Token))
                {
                    // There is no token, so the user has not signed in yet.

                    // Retrieve the OAuth Sign in Link to use in the MessagingExtensionResult Suggested Actions
                    var userTokenClient = turnContext.TurnState.Get<UserTokenClient>();
                    var resource = await userTokenClient.GetSignInResourceAsync(_connectionName, turnContext.Activity as Activity, null, cancellationToken);

                    return new MessagingExtensionActionResponse
                    {
                        ComposeExtension = new MessagingExtensionResult
                        {
                            Type = "auth",
                            SuggestedActions = new MessagingExtensionSuggestedAction
                            {
                                Actions = new List<CardAction>
                                {
                                    new CardAction
                                    {
                                        Type = ActionTypes.OpenUrl,
                                        Value = resource.SignInLink,
                                        Title = "Bot Service OAuth",
                                    },
                                },
                            },
                        },
                    };
                }
                var client = new SimpleGraphClient(tokenResponse.Token);
                var profile = await client.GetMyProfile();
                var imagelink = _siteUrl +  await client.GetPublicURLForProfilePhoto(profile.Id);
                return new MessagingExtensionActionResponse
                {
                    Task = new TaskModuleContinueResponse
                    {
                        Value = new TaskModuleTaskInfo
                        {
                            Card = GetProfileCard(profile, imagelink),
                            Height = 250,
                            Width = 400,
                            Title = "Adaptive Card: Inputs",
                        },
                    },
                };
            }
            else if (action.CommandId.ToUpper() == "UPLOADPHOTO")
            {
                return UploadPhoto(turnContext, action);
            }
            if (action.CommandId.ToUpper() == "SIGNOUTCOMMAND")
            {
                var userTokenClient = turnContext.TurnState.Get<UserTokenClient>();
                await userTokenClient.SignOutUserAsync(turnContext.Activity.From.Id, _connectionName, turnContext.Activity.ChannelId, cancellationToken);

                return new MessagingExtensionActionResponse
                {
                    Task = new TaskModuleContinueResponse
                    {
                        Value = new TaskModuleTaskInfo
                        {
                            Card = new Microsoft.Bot.Schema.Attachment
                            {
                                Content = new AdaptiveCard(new AdaptiveSchemaVersion("1.0"))
                                {
                                    Body = new List<AdaptiveElement>() { new AdaptiveTextBlock() { Text = "You have been signed out." } },
                                    Actions = new List<AdaptiveAction>() { new AdaptiveSubmitAction() { Title = "Close" } },
                                },
                                ContentType = AdaptiveCard.ContentType,
                            },
                            Height = 200,
                            Width = 400,
                            Title = "Adaptive Card: Inputs",
                        },
                    },
                };
            }
            return null;
        }

        protected override async Task<MessagingExtensionResponse> OnTeamsMessagingExtensionSelectItemAsync(ITurnContext<IInvokeActivity> turnContext, JObject query, CancellationToken cancellationToken)
        {
            var (imgId, imgName, imgUrl, imgSignInUrl) = query.ToObject<(string, string, string, string)>();
            var card = new ThumbnailCard
            {
                Title = "OneDrive Image",
                Subtitle = imgName,
                Buttons = new List<CardAction>
                    {
                        new CardAction { Type = ActionTypes.OpenUrl, Title = "Download Image", Value = imgUrl },
                        new CardAction { Type = ActionTypes.OpenUrl, Title = "OneDrive Url", Value = imgSignInUrl },
                    },
            };
            //if (!string.IsNullOrEmpty(iconUrl))
            //{
            card.Images = new List<CardImage>() { new CardImage(imgUrl, "Icon") };
            //}
            var attachment = new MessagingExtensionAttachment
            {
                ContentType = ThumbnailCard.ContentType,
                Content = card,
            };
            return await Task.FromResult(new MessagingExtensionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    Type = "result",
                    AttachmentLayout = "list",
                    Attachments = new List<MessagingExtensionAttachment> { attachment }
                }
            });
        }

        private async Task<TokenResponse> GetTokenResponse(ITurnContext<IInvokeActivity> turnContext, string state, CancellationToken cancellationToken)
        {
            var magicCode = string.Empty;

            if (!string.IsNullOrEmpty(state))
            {
                if (int.TryParse(state, out var parsed))
                {
                    magicCode = parsed.ToString();
                }
            }

            var userTokenClient = turnContext.TurnState.Get<UserTokenClient>();
            var tokenResponse = await userTokenClient.GetUserTokenAsync(turnContext.Activity.From.Id, _connectionName, turnContext.Activity.ChannelId, magicCode, cancellationToken);

            return tokenResponse;
        }

        protected override async Task<InvokeResponse> OnInvokeActivityAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            JObject valueObject = JObject.FromObject(turnContext.Activity.Value);
            if (valueObject["authentication"] != null)
            {
                JObject authenticationObject = JObject.FromObject(valueObject["authentication"]);
                if (authenticationObject["token"] != null)
                {
                    //If the token is NOT exchangeable, then return 412 to require user consent
                    if (await TokenIsExchangeable(turnContext, cancellationToken))
                    {
                        return await base.OnInvokeActivityAsync(turnContext, cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        var response = new InvokeResponse();
                        response.Status = 412;
                        return response;
                    }
                }
            }
            return await base.OnInvokeActivityAsync(turnContext, cancellationToken).ConfigureAwait(false);
        }

        private async Task<bool> TokenIsExchangeable(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            TokenResponse tokenExchangeResponse = null;
            try
            {
                JObject valueObject = JObject.FromObject(turnContext.Activity.Value);
                var tokenExchangeRequest =
                ((JObject)valueObject["authentication"])?.ToObject<TokenExchangeInvokeRequest>();

                var userTokenClient = turnContext.TurnState.Get<UserTokenClient>();

                tokenExchangeResponse = await userTokenClient.ExchangeTokenAsync(turnContext.Activity.From.Id,
                    _connectionName, turnContext.Activity.ChannelId,
                    new TokenExchangeRequest { Token = tokenExchangeRequest.Token },
                    cancellationToken);
            }
#pragma warning disable CA1031 //Do not catch general exception types (ignoring, see comment below)
            catch
#pragma warning restore CA1031 //Do not catch general exception types
            {
                //ignore exceptions
                //if token exchange failed for any reason, tokenExchangeResponse above remains null, and a failure invoke response is sent to the caller.
                //This ensures the caller knows that the invoke has failed.
            }
            if (tokenExchangeResponse == null || string.IsNullOrEmpty(tokenExchangeResponse.Token))
            {
                return false;
            }
            return true;
        }

        private static Microsoft.Bot.Schema.Attachment GetProfileCard(Graph.User profile, string imagelink)
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0));

            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = $"Hello, {profile.DisplayName}",
                Size = AdaptiveTextSize.ExtraLarge
            });

            card.Body.Add(new AdaptiveImage()
            {
                Url = new Uri(imagelink)
            });
            return new Microsoft.Bot.Schema.Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }


        #region Custom action method

        private MessagingExtensionActionResponse UploadPhoto(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action)
        {
            var response = new MessagingExtensionActionResponse()
            {
                Task = new TaskModuleContinueResponse()
                {
                    Type = "continue",
                    Value = new TaskModuleTaskInfo()
                    {
                        Height = 300,
                        Width = 475,
                        Title = "Upload photo to OneDrive",
                        Url = _siteUrl + "/test/UploadPhoto",
                    }
                }
            };
            return response;
        }
        private async Task<MessagingExtensionActionResponse> UploadPhotoSubmitAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
            // The user has chosen to create a card by choosing the 'Web View' context menu command.
            UploadFormResponse cardData = JsonConvert.DeserializeObject<UploadFormResponse>(action.Data.ToString());

            // When the Bot Service Auth flow completes, the action.State will contain a magic code used for verification.
            var state = action.State; // Check the state value
            var tokenResponse = await GetTokenResponse(turnContext, state, cancellationToken);
            Tuple<bool, string> objFolderId = await OneDriveHelper.GetOneDriveFolderIDAsync(tokenResponse.Token);
            //if (objFolderId.Item1) {
            //    // To Do
            //}
            Tuple<bool, JObject> objFileUpload = await OneDriveHelper.UploadOneDrivePhotoAsync(tokenResponse.Token, objFolderId.Item2, cardData.photoFileName, cardData.photoFile);


            var imgUrl = objFileUpload.Item2["@microsoft.graph.downloadUrl"].ToString(); //_siteUrl + "/images/images-002.jpg";

            var card = new ThumbnailCard
            {
                Title = "Name: " + cardData.photoName,
                Subtitle = cardData.photoFileName,
                Text = objFileUpload.Item2["webUrl"].ToString(),
                Images = new List<CardImage> { new CardImage { Url = imgUrl } },
                Buttons = new List<CardAction>
                    {
                        new CardAction { Type = ActionTypes.OpenUrl, Title = "Download Image", Value = imgUrl },
                        new CardAction { Type = ActionTypes.OpenUrl, Title = "OneDrive Url", Value = objFileUpload.Item2["webUrl"] },
                    },
            };

            var attachments = new List<MessagingExtensionAttachment>();
            attachments.Add(new MessagingExtensionAttachment
            {
                Content = card,
                ContentType = ThumbnailCard.ContentType,
                Preview = card.ToAttachment(),
            });

            return new MessagingExtensionActionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    AttachmentLayout = "list",
                    Type = "result",
                    Attachments = attachments,
                },
            };
        }
        #endregion

    }
}

