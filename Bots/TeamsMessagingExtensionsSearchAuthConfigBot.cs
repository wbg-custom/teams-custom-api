// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.BotBuilderSamples.Models;
using Microsoft.Extensions.Configuration;
//using Microsoft.Graph;
using Newtonsoft.Json;
//using Microsoft.Graph;
using Newtonsoft.Json.Linq;
using NuGet.Protocol;
using TeamsMessagingExtensionsSearchAuthConfig.helper;
using TeamsMessagingExtensionsSearchAuthConfig.Helpers;
//using static System.Net.Mime.MediaTypeNames;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class TeamsMessagingExtensionsSearchAuthConfigBot : TeamsActivityHandler
    {
        private readonly string _connectionName;
        private readonly string _siteUrl;
        private readonly UserState _userState;
        private readonly IStatePropertyAccessor<string> _userConfigProperty;

        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TeamsMessagingExtensionsSearchAuthConfigBot(IConfiguration configuration, UserState userState, IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _connectionName = configuration["ConnectionName"] ?? throw new NullReferenceException("ConnectionName");
            _siteUrl = configuration["SiteUrl"] ?? throw new NullReferenceException("SiteUrl");
            _userState = userState ?? throw new NullReferenceException(nameof(userState));
            _userConfigProperty = userState.CreateProperty<string>("UserConfiguration");

            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _httpClientFactory = httpClientFactory;
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // After the turn is complete, persist any UserState changes.
            await _userState.SaveChangesAsync(turnContext);
        }

        protected async override Task<MessagingExtensionResponse> OnTeamsAppBasedLinkQueryAsync(ITurnContext<IInvokeActivity> turnContext, AppBasedLinkQuery query, CancellationToken cancellationToken)
        {

            var state = query.State; // Check the state value
            var tokenResponse = await GetTokenResponse(turnContext, state, cancellationToken);
            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.Token))
            {
                // There is no token, so the user has not signed in yet.

                // Retrieve the OAuth Sign in Link to use in the MessagingExtensionResult Suggested Actions
                var signInLink = await GetSignInLinkAsync(turnContext, cancellationToken).ConfigureAwait(false);

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
                                        Value = signInLink,
                                        Title = "Bot Service OAuth",
                                    },
                                },
                        },
                    },
                };
            }

            var client = new SimpleGraphClient(tokenResponse.Token);
            var profile = await client.GetMyProfile();
            var heroCard = new ThumbnailCard
            {
                Title = "Thumbnail Card",
                Text = $"Hello, {profile.DisplayName}",
                Images = new List<CardImage> { new CardImage("https://raw.githubusercontent.com/microsoft/botframework-sdk/master/icon.png") },
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
            var state = settings["state"];
            if (state != null)
            {
                var userConfigSettings = state.ToString();
                await _userConfigProperty.SetAsync(turnContext, userConfigSettings, cancellationToken);
            }
        }

        protected override async Task<MessagingExtensionResponse> OnTeamsMessagingExtensionQueryAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionQuery action, CancellationToken cancellationToken)
        {

            var text = action?.Parameters?[0]?.Value as string ?? string.Empty;

            var attachments = new List<MessagingExtensionAttachment>();
            var userConfigSettings = await _userConfigProperty.GetAsync(turnContext, () => string.Empty);
            if (userConfigSettings.ToUpper().Contains("EMAIL"))
            {
                // When the Bot Service Auth flow completes, the action.State will contain a magic code used for verification.
                var state = action.State; // Check the state value
                var tokenResponse = await GetTokenResponse(turnContext, state, cancellationToken);
                if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.Token))
                {
                    // There is no token, so the user has not signed in yet.

                    // Retrieve the OAuth Sign in Link to use in the MessagingExtensionResult Suggested Actions
                    var signInLink = await GetSignInLinkAsync(turnContext, cancellationToken).ConfigureAwait(false);

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
                                        Value = signInLink,
                                        Title = "Bot Service OAuth",
                                    },
                                },
                            },
                        },
                    };
                }

                var client = new SimpleGraphClient(tokenResponse.Token);

                var messages = await client.SearchMailInboxAsync("demo");

                // Here we construct a ThumbnailCard for every attachment, and provide a HeroCard which will be
                // displayed if the selects that item.
                attachments = messages.Select(msg => new MessagingExtensionAttachment
                {
                    ContentType = HeroCard.ContentType,
                    Content = new HeroCard
                    {
                        Title = msg.From.EmailAddress.Address,
                        Subtitle = msg.Subject,
                        Text = msg.Body.Content,
                    },
                    Preview = new ThumbnailCard
                    {
                        Title = msg.From.EmailAddress.Address,
                        Text = $"{msg.Subject}<br />{msg.BodyPreview}",
                        Images = new List<CardImage>()
                            {
                                new CardImage("https://raw.githubusercontent.com/microsoft/botbuilder-samples/master/docs/media/OutlookLogo.jpg", "Outlook Logo"),
                            },
                    }.ToAttachment()
                }
                ).ToList();
            }
            else
            {
                //var packages = await FindPackages(text);
                //// We take every row of the results and wrap them in cards wrapped in in MessagingExtensionAttachment objects.
                //// The Preview is optional, if it includes a Tap, that will trigger the OnTeamsMessagingExtensionSelectItemAsync event back on this bot.
                //attachments = packages.Select(package =>
                //        {
                //            var previewCard = new ThumbnailCard { Title = package.Item1, Tap = new CardAction { Type = "invoke", Value = package } };
                //            if (!string.IsNullOrEmpty(package.Item5))
                //            {
                //                previewCard.Images = new List<CardImage>() { new CardImage(package.Item5, "Icon") };
                //            }

                //            var attachment = new MessagingExtensionAttachment
                //            {
                //                ContentType = HeroCard.ContentType,
                //                Content = new HeroCard { Title = package.Item1 },
                //                Preview = previewCard.ToAttachment()
                //            };

                //            return attachment;
                //        }).ToList();

                ////token access
                //string accessToken = await SSOAuthHelper.GetAccessTokenOnBehalfUserAsync(_configuration, _httpClientFactory, _httpContextAccessor);

                // When the Bot Service Auth flow completes, the action.State will contain a magic code used for verification.
                var state = action.State; // Check the state value
                var tokenResponse = await GetTokenResponse(turnContext, state, cancellationToken);

                if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.Token))
                {
                    // There is no token, so the user has not signed in yet.

                    // Retrieve the OAuth Sign in Link to use in the MessagingExtensionResult Suggested Actions
                    var signInLink = await GetSignInLinkAsync(turnContext, cancellationToken).ConfigureAwait(false);

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
                                        Value = signInLink,
                                        Title = "Bot Service OAuth",
                                    },
                                },
                            },
                        },
                    };
                }


                string accessToken = tokenResponse.Token;
                List<(string, string, string, string)> result = await OneDriveTeamPhotosList(accessToken);

                //var previewCard = new ThumbnailCard { Title = "test-title-01", Tap = new CardAction { Type = "invoke", Value = "test-value" } };
                //if (!string.IsNullOrEmpty(package.Item5))
                //{
                //    previewCard.Images = new List<CardImage>() { new CardImage(package.Item5, "Icon") };
                //}
                //var attachment = new MessagingExtensionAttachment
                //{
                //    ContentType = HeroCard.ContentType,
                //    Content = new HeroCard { Title = "test-title=02" },
                //    Preview = previewCard.ToAttachment()
                //};
                //attachments.Add(attachment);

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

            // The list of MessagingExtensionAttachments must we wrapped in a MessagingExtensionResult wrapped in a MessagingExtensionResponse.
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

        protected override Task<MessagingExtensionResponse> OnTeamsMessagingExtensionSelectItemAsync(ITurnContext<IInvokeActivity> turnContext, JObject query, CancellationToken cancellationToken)
        {
            // The Preview card's Tap should have a Value property assigned, this will be returned to the bot in this event. 
            //var (packageId, version, description, projectUrl, iconUrl) = query.ToObject<(string, string, string, string, string)>();
            var (imgId, imgName, imgUrl, imgSignInUrl) = query.ToObject<(string, string, string, string)>();

            // We take every row of the results and wrap them in cards wrapped in in MessagingExtensionAttachment objects.
            // The Preview is optional, if it includes a Tap, that will trigger the OnTeamsMessagingExtensionSelectItemAsync event back on this bot.
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

            return Task.FromResult(new MessagingExtensionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    Type = "result",
                    AttachmentLayout = "list",
                    Attachments = new List<MessagingExtensionAttachment> { attachment }
                }
            });
        }

        protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionSubmitActionAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
            // This method is to handle the 'Close' button on the confirmation Task Module after the user signs out.
            switch(action.CommandId)
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
                    var signInLink = await GetSignInLinkAsync(turnContext, cancellationToken).ConfigureAwait(false);

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
                                        Value = signInLink,
                                        Title = "Bot Service OAuth",
                                    },
                                },
                            },
                        },
                    };
                }

                var client = new SimpleGraphClient(tokenResponse.Token);

                var profile = await client.GetMyProfile();

                return new MessagingExtensionActionResponse
                {
                    Task = new TaskModuleContinueResponse
                    {
                        Value = new TaskModuleTaskInfo
                        {
                            Card = GetProfileCard(profile),
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
                //var state = action.State; // Check the state value
                //var tokenResponse = await GetTokenResponse(turnContext, state, cancellationToken);
                //if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.Token))
                //{
                //    // There is no token, so the user has not signed in yet.

                //    // Retrieve the OAuth Sign in Link to use in the MessagingExtensionResult Suggested Actions
                //    var signInLink = await GetSignInLinkAsync(turnContext, cancellationToken).ConfigureAwait(false);

                //    return new MessagingExtensionActionResponse
                //    {
                //        ComposeExtension = new MessagingExtensionResult
                //        {
                //            Type = "auth",
                //            SuggestedActions = new MessagingExtensionSuggestedAction
                //            {
                //                Actions = new List<CardAction>
                //                {
                //                    new CardAction
                //                    {
                //                        Type = ActionTypes.OpenUrl,
                //                        Value = signInLink,
                //                        Title = "Bot Service OAuth",
                //                    },
                //                },
                //            },
                //        },
                //    };
                //}

                //var client = new SimpleGraphClient(tokenResponse.Token);

                //var profile = await client.GetMyProfile();

                //return new MessagingExtensionActionResponse
                //{
                //    Task = new TaskModuleContinueResponse
                //    {
                //        Value = new TaskModuleTaskInfo
                //        {
                //            Card = GetProfileCard(profile),
                //            Height = 250,
                //            Width = 400,
                //            Title = "Adaptive Card: Inputs",
                //        },
                //    },
                //};
            }

            else if (action.CommandId.ToUpper() == "SIGNOUTCOMMAND")
            {
                var userTokenClient = turnContext.TurnState.Get<UserTokenClient>();
                await userTokenClient.SignOutUserAsync(turnContext.Activity.From.Id, _connectionName, turnContext.Activity.ChannelId, cancellationToken).ConfigureAwait(false);

                return new MessagingExtensionActionResponse
                {
                    Task = new TaskModuleContinueResponse
                    {
                        Value = new TaskModuleTaskInfo
                        {
                            Card = new Attachment
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

        private async Task<string> GetSignInLinkAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var userTokenClient = turnContext.TurnState.Get<UserTokenClient>();
            var resource = await userTokenClient.GetSignInResourceAsync(_connectionName, turnContext.Activity as Activity, null, cancellationToken).ConfigureAwait(false);
            return resource.SignInLink;
        }

        private static Attachment GetProfileCard(Graph.User profile)
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0));

            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = $"Hello, {profile.DisplayName}",
                Size = AdaptiveTextSize.ExtraLarge
            });

            card.Body.Add(new AdaptiveImage()
            {
                Url = new Uri("http://adaptivecards.io/content/cats/1.png")
            });
            return new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }

        // Generate a set of substrings to illustrate the idea of a set of results coming back from a query. 
        private async Task<IEnumerable<(string, string, string, string, string)>> FindPackages(string text)
        {
            var obj = JObject.Parse(await (new HttpClient()).GetStringAsync($"https://azuresearch-usnc.nuget.org/query?q=id:{text}&prerelease=true"));
            return obj["data"].Select(item => (item["id"].ToString(), item["version"].ToString(), item["description"].ToString(), item["projectUrl"]?.ToString(), item["iconUrl"]?.ToString()));
        }

        private async Task<List<(string, string, string, string)>> OneDriveTeamPhotosList(string accessToken)
        {
            Tuple<bool, string> objFolderId = await OneDriveHelper.GetOneDriveFolderIDAsync(accessToken);
            //var obj = JObject.Parse(await (new HttpClient()).GetStringAsync($"https://azuresearch-usnc.nuget.org/query?q=id:{text}&prerelease=true"));
            //return obj["data"].Select(item => (item["id"].ToString(), item["version"].ToString(), item["description"].ToString(), item["projectUrl"]?.ToString(), item["iconUrl"]?.ToString()));

            List<(string, string, string, string)> objList = new List<(string, string, string, string)>();
            if (objFolderId.Item1)
            {
                Tuple<bool, JObject> objPhotoList = await OneDriveHelper.GetOneDrivePhotoListAsync(accessToken, objFolderId.Item2);
                //objList.Add((objResult.Item2, objResult.Item2));
                if (objPhotoList.Item2 != null && objPhotoList.Item2.Count > 0)
                {
                    return objPhotoList.Item2["value"].Select(item => (item["id"].ToString(), item["name"].ToString(), item["@microsoft.graph.downloadUrl"].ToString(), item["webUrl"].ToString())).ToList();
                }
            }
            return objList;
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
            var tokenResponse = await userTokenClient.GetUserTokenAsync(turnContext.Activity.From.Id, _connectionName, turnContext.Activity.ChannelId, magicCode, cancellationToken).ConfigureAwait(false);
            return tokenResponse;
        }

        #region Message Extension Action Response

        //private MessagingExtensionActionResponse EmpDetails(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action)
        //{
        //    var response = new MessagingExtensionActionResponse()
        //    {
        //        Task = new TaskModuleContinueResponse()
        //        {
        //            Type = "continue",
        //            Value = new TaskModuleTaskInfo()
        //            {
        //                Height = 300,
        //                Width = 450,
        //                Title = "Task Module WebView",
        //                Url = _siteUrl + "/Home/CustomForm",
        //            }
        //        }
        //    };
        //    return response;
        //}

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
                        Width = 450,
                        Title = "Task Module WebView",
                        Url = _siteUrl + "/Home/UploadPhoto",
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
