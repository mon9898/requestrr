using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Requestrr.WebApi.RequestrrBot.Locale;
using Requestrr.WebApi.RequestrrBot.TvShows;

namespace Requestrr.WebApi.RequestrrBot.ChatClients.Discord
{
    public class DiscordAnimeUserInterface : ITvShowUserInterface
    {
        private readonly DiscordInteraction _interactionContext;

        public DiscordAnimeUserInterface(DiscordInteraction interactionContext)
        {
            _interactionContext = interactionContext;
        }

        public async Task DisplayMultiSeasonSelectionAsync(TvShowRequest request, TvShow tvShow, TvSeason[] tvSeasons)
        {
            var embed = DiscordTvShowUserInterface.GenerateTvShowDetailsAsync(tvShow);
            var options = tvSeasons.Select(x => new DiscordSelectComponentOption(GetFormattedSeasonName(tvShow, x), $"{request.CategoryId}/{tvShow.TheTvDbId.ToString()}/{x.SeasonNumber.ToString()}")).ToList();
            var seasonSelector = new DiscordSelectComponent($"ASS/{_interactionContext.User.Id}/{request.CategoryId}", LimitStringSize(Language.Current.DiscordCommandTvRequestHelpSeasonsDropdown), options);

            var builder = (await AddPreviousDropdownsAsync(tvShow, new DiscordWebhookBuilder().AddEmbed(embed))).AddComponents(seasonSelector).WithContent(Language.Current.DiscordCommandTvRequestHelpSeasons);
            await _interactionContext.EditOriginalResponseAsync(builder);
        }

        public async Task DisplayNotificationSuccessForSeasonAsync(TvShow tvShow, TvSeason requestedSeason)
        {
            var embed = DiscordTvShowUserInterface.GenerateTvShowDetailsAsync(tvShow);
            var successButton = new DiscordButtonComponent(ButtonStyle.Success, $"0/1/0", Language.Current.DiscordCommandNotifyMeSuccess);

            if (requestedSeason is FutureTvSeasons)
            {
                var builder = (await AddPreviousDropdownsAsync(tvShow, new DiscordWebhookBuilder().AddEmbed(embed))).AddComponents(successButton).WithContent(Language.Current.DiscordCommandTvNotificationSuccessFutureSeasons.ReplaceTokens(tvShow));
                await _interactionContext.EditOriginalResponseAsync(builder);
            }
            else
            {
                var builder = (await AddPreviousDropdownsAsync(tvShow, new DiscordWebhookBuilder().AddEmbed(embed))).AddComponents(successButton).WithContent(Language.Current.DiscordCommandTvNotificationSuccessSeason.ReplaceTokens(tvShow, requestedSeason.SeasonNumber));
                await _interactionContext.EditOriginalResponseAsync(builder);
            }
        }

        public async Task AskForSeasonNotificationRequestAsync(TvShow tvShow, TvSeason selectedSeason)
        {
            var message = Language.Current.DiscordCommandTvNotificationRequestSeason.ReplaceTokens(tvShow, selectedSeason.SeasonNumber);

            if (selectedSeason is FutureTvSeasons)
            {
                if (tvShow.AllSeasonsAvailable())
                    message = Language.Current.DiscordCommandTvNotificationRequestFutureSeasonAvailable;
                else if (tvShow.AllSeasonsFullyRequested())
                    message = Language.Current.DiscordCommandTvNotificationRequestFutureSeasonRequested;
                else
                    message = Language.Current.DiscordCommandTvNotificationRequestFutureSeasonMissing;
            }

            var requestButton = new DiscordButtonComponent(ButtonStyle.Primary, $"ANR/{_interactionContext.User.Id}/{tvShow.TheTvDbId}/{selectedSeason.GetType().Name.First()}/{selectedSeason.SeasonNumber}", Language.Current.DiscordCommandNotifyMe, false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🔔")));

            var embed = DiscordTvShowUserInterface.GenerateTvShowDetailsAsync(tvShow);
            var builder = (await AddPreviousDropdownsAsync(tvShow, new DiscordWebhookBuilder().AddEmbed(embed))).AddComponents(requestButton).WithContent(message);
            await _interactionContext.EditOriginalResponseAsync(builder);
        }

        public async Task DisplayRequestDeniedForSeasonAsync(TvShow tvShow, TvSeason selectedSeason)
        {
            var embed = DiscordTvShowUserInterface.GenerateTvShowDetailsAsync(tvShow);
            var deniedButton = new DiscordButtonComponent(ButtonStyle.Danger, $"0/1/0", Language.Current.DiscordCommandRequestButtonDenied);
            var builder = (await AddPreviousDropdownsAsync(tvShow, new DiscordWebhookBuilder().AddEmbed(embed))).AddComponents(deniedButton).WithContent(Language.Current.DiscordCommandTvRequestDenied);
            await _interactionContext.EditOriginalResponseAsync(builder);
        }

        public async Task DisplayRequestSuccessForSeasonAsync(TvShow tvShow, TvSeason requestedSeason)
        {
            var embed = DiscordTvShowUserInterface.GenerateTvShowDetailsAsync(tvShow);

            var message = requestedSeason is AllTvSeasons
                ? Language.Current.DiscordCommandTvRequestSuccessAllSeasons.ReplaceTokens(tvShow, requestedSeason.SeasonNumber)
                : requestedSeason is FutureTvSeasons
                    ? Language.Current.DiscordCommandTvRequestSuccessFutureSeasons.ReplaceTokens(tvShow, requestedSeason.SeasonNumber)
                    : Language.Current.DiscordCommandTvRequestSuccessSeason.ReplaceTokens(tvShow, requestedSeason.SeasonNumber);

            var successButton = new DiscordButtonComponent(ButtonStyle.Success, $"0/1/0", Language.Current.DiscordCommandRequestButtonSuccess);
            var builder = (await AddPreviousDropdownsAsync(tvShow, new DiscordWebhookBuilder().AddEmbed(embed))).AddComponents(successButton).WithContent(message);
            await _interactionContext.EditOriginalResponseAsync(builder);
        }

        public async Task DisplayTvShowDetailsForSeasonAsync(TvShowRequest request, TvShow tvShow, TvSeason season)
        {
            var message = season is AllTvSeasons
                ? Language.Current.DiscordCommandTvRequestConfirmAllSeasons
                : season is FutureTvSeasons
                    ? Language.Current.DiscordCommandTvRequestConfirmFutureSeasons
                    : Language.Current.DiscordCommandTvRequestConfirmSeason.ReplaceTokens(LanguageTokens.SeasonNumber, season.SeasonNumber.ToString());

            var requestButton = new DiscordButtonComponent(ButtonStyle.Primary, $"ARC/{_interactionContext.User.Id}/{request.CategoryId}/{tvShow.TheTvDbId}/{season.SeasonNumber}", Language.Current.DiscordCommandRequestButton);

            var embed = DiscordTvShowUserInterface.GenerateTvShowDetailsAsync(tvShow);
            var builder = (await AddPreviousDropdownsAsync(tvShow, new DiscordWebhookBuilder().AddEmbed(embed))).AddComponents(requestButton).WithContent(message);
            await _interactionContext.EditOriginalResponseAsync(builder);
        }

        public Task DisplayTvShowIssueDetailsAsync(TvShowRequest request, TvShow tvShow, string issue, int? seasonNumber, int? episodeNumber)
            => Task.CompletedTask;

        public Task DisplayTvShowIssueModalAsync(TvShowRequest request, TvShow tvShow, string issue, int? seasonNumber, int? episodeNumber)
            => Task.CompletedTask;

        public Task CompleteTvShowIssueModalRequestAsync(TvShow tvShow, bool success)
            => Task.CompletedTask;

        public Task ShowTvShowIssueSelection(TvShowRequest request, IReadOnlyList<SearchedTvShow> searchedTvShows)
            => Task.CompletedTask;

        public async Task ShowTvShowSelection(TvShowRequest request, IReadOnlyList<SearchedTvShow> searchedTvShows)
        {
            var options = searchedTvShows.Take(25).Select(x => new DiscordSelectComponentOption(GetFormatedTvShowTitle(x), $"{request.CategoryId}/{x.TheTvDbId.ToString()}")).ToList();
            var select = new DiscordSelectComponent($"ARS/{_interactionContext.User.Id}/{request.CategoryId}", LimitStringSize(Language.Current.DiscordCommandTvRequestHelpSearchDropdown), options);

            await _interactionContext.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddComponents(select).WithContent(Language.Current.DiscordCommandTvRequestHelpSearch));
        }

        public async Task WarnAlreadyNotifiedForSeasonsAsync(TvShow tvShow, TvSeason requestedSeason)
        {
            var messageContent = Language.Current.DiscordCommandTvRequestAlreadyExistNotifiedSeason.ReplaceTokens(tvShow, requestedSeason.SeasonNumber);
            var buttonMessage = Language.Current.DiscordCommandRequestedButton;

            if (requestedSeason is FutureTvSeasons)
            {
                if (tvShow.AllSeasonsAvailable()) { messageContent = Language.Current.DiscordCommandTvRequestAlreadyExistNotifiedFutureSeasonAvailable; buttonMessage = Language.Current.DiscordCommandAvailableButton; }
                else if (tvShow.AllSeasonsFullyRequested()) messageContent = Language.Current.DiscordCommandTvRequestAlreadyExistNotifiedFutureSeasonRequested;
                else messageContent = Language.Current.DiscordCommandTvRequestAlreadyExistNotifiedFutureSeasonFound;
            }

            var embed = DiscordTvShowUserInterface.GenerateTvShowDetailsAsync(tvShow);
            var requestButton = new DiscordButtonComponent(ButtonStyle.Primary, $"ATT/{_interactionContext.User.Id}/{tvShow.TheTvDbId}/999", buttonMessage, true);
            var builder = (await AddPreviousDropdownsAsync(tvShow, new DiscordWebhookBuilder().AddEmbed(embed))).WithContent(messageContent).AddComponents(requestButton);
            await _interactionContext.EditOriginalResponseAsync(builder);
        }

        public async Task WarnAlreadySeasonAlreadyRequestedAsync(TvShow tvShow, TvSeason requestedSeason)
        {
            var messageContent = Language.Current.DiscordCommandTvRequestAlreadyExistSeason.ReplaceTokens(tvShow, requestedSeason.SeasonNumber);
            var buttonMessage = Language.Current.DiscordCommandRequestedButton;

            if (requestedSeason is FutureTvSeasons)
            {
                if (tvShow.AllSeasonsAvailable()) { messageContent = Language.Current.DiscordCommandTvRequestAlreadyExistFutureSeasonAvailable; buttonMessage = Language.Current.DiscordCommandAvailableButton; }
                else if (tvShow.AllSeasonsFullyRequested()) messageContent = Language.Current.DiscordCommandTvRequestAlreadyExistFutureSeasonRequested;
                else messageContent = Language.Current.DiscordCommandTvRequestAlreadyExistFutureSeasonFound;
            }

            var embed = DiscordTvShowUserInterface.GenerateTvShowDetailsAsync(tvShow);
            var requestButton = new DiscordButtonComponent(ButtonStyle.Primary, $"ATT/{_interactionContext.User.Id}/{tvShow.TheTvDbId}/999", buttonMessage, true);
            var builder = (await AddPreviousDropdownsAsync(tvShow, new DiscordWebhookBuilder().AddEmbed(embed))).WithContent(messageContent).AddComponents(requestButton);
            await _interactionContext.EditOriginalResponseAsync(builder);
        }

        public async Task WarnNoTvShowFoundAsync(string tvShowName)
        {
            await _interactionContext.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent(Language.Current.DiscordCommandTvNotFound.ReplaceTokens(LanguageTokens.TvShowTitle, tvShowName)));
        }

        public async Task WarnNoTvShowFoundByTvDbIdAsync(int tvDbId)
        {
            await _interactionContext.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent(Language.Current.DiscordCommandTvNotFoundTVDBID.ReplaceTokens(LanguageTokens.TvShowTVDBID, tvDbId.ToString())));
        }

        public async Task WarnSeasonAlreadyAvailableAsync(TvShow tvShow, TvSeason selectedSeason)
        {
            var embed = DiscordTvShowUserInterface.GenerateTvShowDetailsAsync(tvShow);
            var requestButton = new DiscordButtonComponent(ButtonStyle.Primary, $"ATT/{_interactionContext.User.Id}/{tvShow.TheTvDbId}/999", Language.Current.DiscordCommandAvailableButton, true);
            var builder = (await AddPreviousDropdownsAsync(tvShow, new DiscordWebhookBuilder().AddEmbed(embed))).WithContent(Language.Current.DiscordCommandTvRequestAlreadyAvailableSeason.ReplaceTokens(LanguageTokens.SeasonNumber, selectedSeason.SeasonNumber.ToString())).AddComponents(requestButton);
            await _interactionContext.EditOriginalResponseAsync(builder);
        }

        public async Task WarnShowCannotBeRequestedAsync(TvShow tvShow)
        {
            var embed = DiscordTvShowUserInterface.GenerateTvShowDetailsAsync(tvShow);
            var requestButton = new DiscordButtonComponent(ButtonStyle.Danger, $"ATT/{_interactionContext.User.Id}/{tvShow.TheTvDbId}/999", Language.Current.DiscordCommandRequestButton, true);
            var builder = (await AddPreviousDropdownsAsync(tvShow, new DiscordWebhookBuilder().AddEmbed(embed))).WithContent(Language.Current.DiscordCommandTvRequestUnsupported).AddComponents(requestButton);
            await _interactionContext.EditOriginalResponseAsync(builder);
        }

        public async Task WarnShowHasEndedAsync(TvShow tvShow)
        {
            var embed = DiscordTvShowUserInterface.GenerateTvShowDetailsAsync(tvShow);

            if (tvShow.AllSeasonsAvailable())
            {
                var requestButton = new DiscordButtonComponent(ButtonStyle.Primary, $"ATT/{_interactionContext.User.Id}/{tvShow.TheTvDbId}/999", Language.Current.DiscordCommandAvailableButton, true);
                var builder = (await AddPreviousDropdownsAsync(tvShow, new DiscordWebhookBuilder().AddEmbed(embed))).WithContent(Language.Current.DiscordCommandTvRequestAlreadyAvailableSeries).AddComponents(requestButton);
                await _interactionContext.EditOriginalResponseAsync(builder);
            }
            else
            {
                var requestButton = new DiscordButtonComponent(ButtonStyle.Primary, $"ATT/{_interactionContext.User.Id}/{tvShow.TheTvDbId}/999", Language.Current.DiscordCommandRequestedButton, true);
                var builder = (await AddPreviousDropdownsAsync(tvShow, new DiscordWebhookBuilder().AddEmbed(embed))).AddComponents(requestButton).WithContent(Language.Current.DiscordCommandTvRequestAlreadyExistSeries);
                await _interactionContext.EditOriginalResponseAsync(builder);
            }
        }

        private async Task<DiscordWebhookBuilder> AddPreviousDropdownsAsync(TvShow tvShow, DiscordWebhookBuilder builder)
        {
            var selectors = (await _interactionContext.GetOriginalResponseAsync()).FilterComponents<DiscordSelectComponent>();
            DiscordSelectComponent previousTvSelector = selectors.FirstOrDefault(x => x.CustomId.StartsWith("ARS", true, null));

            if (previousTvSelector != null)
            {
                var tvSelector = new DiscordSelectComponent(previousTvSelector.CustomId, GetFormatedTvShowTitle(tvShow), previousTvSelector.Options);
                builder.AddComponents(tvSelector);
            }

            DiscordSelectComponent previousSeasonSelector = selectors.FirstOrDefault(x => x.CustomId.StartsWith("ASS", true, null));

            if (previousSeasonSelector != null)
            {
                if (!tvShow.AllSeasonsAvailable() && previousSeasonSelector.Options.Any(x => x.Value.Contains(tvShow.TheTvDbId.ToString(), StringComparison.OrdinalIgnoreCase)))
                {
                    if (!_interactionContext.Data.CustomId.StartsWith("ARS", true, null))
                    {
                        var newOptions = tvShow.Seasons.Select(x => new DiscordSelectComponentOption(GetFormattedSeasonName(tvShow, x), $"{tvShow.TheTvDbId.ToString()}/{x.SeasonNumber.ToString()}")).ToDictionary(x => x.Value, x => x);
                        var oldOptions = previousSeasonSelector.Options;
                        var currentOptions = oldOptions.Select(x => new DiscordSelectComponentOption(newOptions.ContainsKey(x.Value) ? LimitStringSize(newOptions[x.Value].Label) : LimitStringSize(x.Label), x.Value)).ToList();

                        string defaultSelectedValue = currentOptions.First().Label;
                        try
                        {
                            defaultSelectedValue = _interactionContext.Data.Values.Any()
                                ? currentOptions.Single(x => x.Value == _interactionContext.Data.Values.Single()).Label
                                : currentOptions.Single(x => x.Value == string.Join("/", _interactionContext.Data.CustomId.Split("/").Skip(2))).Label;
                        }
                        catch { }

                        var seasonSelector = new DiscordSelectComponent(previousSeasonSelector.CustomId, LimitStringSize(defaultSelectedValue), currentOptions);
                        builder.AddComponents(seasonSelector);
                    }
                }
            }

            return builder;
        }

        private string GetFormatedTvShowTitle(SearchedTvShow tvShow) => GetFormatedTvShowTitle(tvShow.Title, tvShow.FirstAired);
        private string GetFormatedTvShowTitle(TvShow tvShow) => GetFormatedTvShowTitle(tvShow.Title, tvShow.FirstAired);

        private string GetFormatedTvShowTitle(string title, string firstAired)
        {
            var releaseYear = !string.IsNullOrWhiteSpace(firstAired) && firstAired.Length >= 4 ? firstAired.Substring(0, 4) : null;
            if (releaseYear != null && !title.Contains(releaseYear, StringComparison.InvariantCultureIgnoreCase))
                return $"{LimitStringSize(title, 93)} ({releaseYear})";
            return LimitStringSize(title);
        }

        private string LimitStringSize(string value, int limit = 100)
            => value.Count() > limit ? value[..(limit - 3)] + "..." : value;

        private string GetFormattedSeasonName(TvShow tvShow, TvSeason season)
        {
            var seasonName = season is AllTvSeasons
                ? $"{Language.Current.DiscordEmbedTvAllSeasons}"
                : season is FutureTvSeasons
                    ? $"{Language.Current.DiscordEmbedTvFutureSeasons}"
                    : $"{Language.Current.DiscordEmbedTvSeason.ReplaceTokens(LanguageTokens.SeasonNumber, season.SeasonNumber.ToString())}";

            if (season is AllTvSeasons)
                seasonName += tvShow.AllSeasonsFullyRequested() ? $" ({Language.Current.DiscordEmbedTvFullyRequested})" : string.Empty;
            else
                seasonName += season.IsRequested == RequestedState.Full ? $" ({Language.Current.DiscordEmbedTvFullyRequested})" : season.IsRequested == RequestedState.Partial ? $" ({Language.Current.DiscordEmbedTvPartiallyRequested})" : string.Empty;

            return seasonName;
        }
    }
}
