using System;
using DSharpPlus;
using DSharpPlus.Entities;
using Requestrr.WebApi.RequestrrBot.ChatClients.Discord;
using Requestrr.WebApi.RequestrrBot.DownloadClients;
using Requestrr.WebApi.RequestrrBot.DownloadClients.Sonarr;
using Requestrr.WebApi.RequestrrBot.Notifications;
using Requestrr.WebApi.RequestrrBot.Notifications.TvShows;
using Requestrr.WebApi.RequestrrBot.TvShows;

namespace Requestrr.WebApi.RequestrrBot.Anime
{
    public class AnimeWorkflowFactory
    {
        private readonly TvShowsSettingsProvider _tvShowsSettingsProvider;
        private readonly DiscordSettingsProvider _settingsProvider;
        private readonly TvShowNotificationsRepository _notificationsRepository;
        private readonly SonarrClient _sonarrDownloadClient;

        public AnimeWorkflowFactory(
            TvShowsSettingsProvider tvShowsSettingsProvider,
            DiscordSettingsProvider settingsProvider,
            TvShowNotificationsRepository notificationsRepository,
            SonarrClient sonarrDownloadClient)
        {
            _tvShowsSettingsProvider = tvShowsSettingsProvider;
            _settingsProvider = settingsProvider;
            _notificationsRepository = notificationsRepository;
            _sonarrDownloadClient = sonarrDownloadClient;
        }

        public TvShowRequestingWorkflow CreateRequestingWorkflow(DiscordInteraction interaction, int categoryId)
        {
            var settings = _settingsProvider.Provide();
            var userInterface = new DiscordAnimeUserInterface(interaction);
            var notificationWorkflow = CreateNotificationWorkflow(interaction, settings);

            return new TvShowRequestingWorkflow(
                new TvShowUserRequester(interaction.User.Id.ToString(), interaction.User.Username),
                categoryId,
                _sonarrDownloadClient,
                _sonarrDownloadClient,
                userInterface,
                notificationWorkflow,
                _tvShowsSettingsProvider.Provide());
        }

        public ITvShowNotificationWorkflow CreateNotificationWorkflow(DiscordInteraction interaction)
        {
            var settings = _settingsProvider.Provide();
            return CreateNotificationWorkflow(interaction, settings);
        }

        private ITvShowNotificationWorkflow CreateNotificationWorkflow(DiscordInteraction interaction, DiscordSettings settings)
        {
            var userInterface = new DiscordAnimeUserInterface(interaction);
            ITvShowNotificationWorkflow notificationWorkflow = new DisabledTvShowNotificationWorkflow(userInterface);

            if (settings.NotificationMode != NotificationMode.Disabled)
            {
                notificationWorkflow = new TvShowNotificationWorkflow(_notificationsRepository, userInterface, _sonarrDownloadClient, settings.AutomaticallyNotifyRequesters);
            }

            return notificationWorkflow;
        }
    }
}
