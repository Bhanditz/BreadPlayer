﻿using BreadPlayer.Database;
using BreadPlayer.Extensions;
using BreadPlayer.Models.Common;
using BreadPlayer.Web.BaiduLyricsAPI;
using BreadPlayer.Web.Lastfm;
using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Api.Helpers;
using IF.Lastfm.Core.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Networking.Connectivity;

namespace BreadPlayer.ViewModels
{
    public class NowPlayingViewModel : ViewModelBase
    {
        #region Loading Properties
        bool artistInfoLoading;
        public bool ArtistInfoLoading { get => artistInfoLoading; set => Set(ref artistInfoLoading, value); }
        bool albumInfoLoading;
        public bool AlbumInfoLoading { get => albumInfoLoading; set => Set(ref albumInfoLoading, value); }
        bool artistFetchFailed;
        public bool ArtistFetchFailed { get => artistFetchFailed; set => Set(ref artistFetchFailed, value); }
        bool albumFetchFailed;
        public bool AlbumFetchFailed { get => albumFetchFailed; set => Set(ref albumFetchFailed, value); }
        #endregion

        LibraryService service = new LibraryService(new KeyValueStoreDatabaseService(Init.SharedLogic.DatabasePath, "Tracks", "TracksText"));
        public string CorrectArtist { get; set; }
        public string CorrectAlbum { get; set; }
        string artistBio;
        public string ArtistBio
        {
            get => artistBio;
            set => Set(ref artistBio, value);
        }
        ThreadSafeObservableCollection<LastTrack> albumTracks;
        public ThreadSafeObservableCollection<LastTrack> AlbumTracks
        {
            get => albumTracks;
            set => Set(ref albumTracks, value);
        }
        ThreadSafeObservableCollection<LastArtist> similarArtists;
        public ThreadSafeObservableCollection<LastArtist> SimilarArtists
        {
            get => similarArtists;
            set => Set(ref similarArtists, value);
        }
        public ICommand RetryCommand { get; set; }
        private LastfmClient LastfmClient
        {
            get => new Lastfm().LastfmClient;
        }
        public NowPlayingViewModel()
        {
            RetryCommand = new RelayCommand(Retry);
            AlbumInfoTokenSource = new CancellationTokenSource();
            ArtistInfoTokenSource = new CancellationTokenSource();
            ArtistInfoToken = ArtistInfoTokenSource.Token;
            AlbumInfoToken = AlbumInfoTokenSource.Token;

            //the work around to knowing when the new song has started.
            //the event is needed to update the bio etc.
            Init.SharedLogic.Player.MediaChanging += (sender, e) =>
            {
                Init.SharedLogic.Player.MediaStateChanged += Player_MediaStateChanged;
            };
        }
        private async void Retry(object para)
        {
            if(para.ToString() == "Artist")
            {
                if (string.IsNullOrEmpty(CorrectArtist))
                    return;
                await GetArtistInfo(CorrectArtist, ArtistInfoToken);
                Init.SharedLogic.Player.CurrentlyPlayingFile.LeadArtist = CorrectArtist;
            }
            else if(para.ToString() == "Album")
            {
                if (string.IsNullOrEmpty(CorrectAlbum) || string.IsNullOrEmpty(CorrectArtist))
                    return;
                await GetAlbumInfo(CorrectArtist, CorrectAlbum, AlbumInfoToken);
                Init.SharedLogic.Player.CurrentlyPlayingFile.LeadArtist = CorrectArtist;
                Init.SharedLogic.Player.CurrentlyPlayingFile.Album = CorrectAlbum;
            }
            await service.UpdateMediafile(Init.SharedLogic.Player.CurrentlyPlayingFile);
        }
        private async void Player_MediaStateChanged(object sender, Events.MediaStateChangedEventArgs e)
        {
            if (e.NewState == PlayerState.Playing)
            {
                Init.SharedLogic.Player.MediaStateChanged -= Player_MediaStateChanged;
                await GetInfo(Init.SharedLogic.Player.CurrentlyPlayingFile.LeadArtist, Init.SharedLogic.Player.CurrentlyPlayingFile.Album);
            }
        }

        //this whole bulk of variables is very annoying and must be
        //replaced with more sensible and readable code. I hate this
        //but it works.
        Task FetchArtistInfoTask;
        Task FetchAlbumInfoTask;
        CancellationToken ArtistInfoToken;
        CancellationToken AlbumInfoToken;
        CancellationTokenSource ArtistInfoTokenSource;
        CancellationTokenSource AlbumInfoTokenSource;
        IAsyncOperation<LastResponse<LastArtist>> ArtistInfoOperation;
        IAsyncOperation<LastResponse<LastAlbum>> AlbumInfoOperation;
        int retries = 0;
        private async Task GetInfo(string artistName, string albumName)
        {
            try
            {
                //start the tasks on another thread so that the UI doesn't hang.

                ConnectionProfile InternetConnectionProfile = NetworkInformation.GetInternetConnectionProfile();

                if (InternetConnectionProfile != null)
                {
                    if (FetchArtistInfoTask?.IsCompleted == false)
                        ArtistInfoTokenSource.Cancel();
                    if (FetchAlbumInfoTask?.IsCompleted == false)
                        AlbumInfoTokenSource.Cancel();
                    FetchArtistInfoTask = GetArtistInfo(artistName, ArtistInfoToken);
                    FetchAlbumInfoTask = GetAlbumInfo(artistName, albumName, AlbumInfoToken);
                    //start both tasks
                    await FetchAlbumInfoTask.ConfigureAwait(false);
                    await FetchArtistInfoTask.ConfigureAwait(false);
                }
                else
                {
                    AlbumFetchFailed = true;
                    ArtistFetchFailed = true;
                }
            }
            catch (Exception)
            {
                //we use this simple logic to avoid too many retries.
                //MAX_RETRIES = 10;
                if (retries == 10)
                {
                    //increase retry count
                    retries++;

                    //retry
                    await GetInfo(artistName, albumName);
                }
            }
        }
        private async Task GetArtistInfo(string artistName, CancellationToken token)
        {
            await Core.SharedLogic.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                CheckAndCancelOperation(ArtistInfoOperation, token);
                ArtistInfoLoading = true;
                ArtistInfoOperation = LastfmClient.Artist.GetInfoAsync(artistName, "en", true).AsAsyncOperation();
                ArtistBio = "";
                ArtistFetchFailed = false;
                var artistInfoResponse = await ArtistInfoOperation;
                if (artistInfoResponse.Success)
                {
                    LastArtist artist = artistInfoResponse.Content;
                    ArtistBio = artist.Bio.Content.ScrubHtml();
                    SimilarArtists = new ThreadSafeObservableCollection<LastArtist>(artist.Similar);
                }
                else
                {
                    ArtistFetchFailed = true;
                    ArtistInfoLoading = false;
                }
                //if it is empty or it starts with [unknown],
                //which is the identifier for unknown artists;
                //just fail.
                if (string.IsNullOrEmpty(ArtistBio) || ArtistBio.StartsWith("[unknown]") || ArtistBio.StartsWith("This is not an artist"))
                    ArtistFetchFailed = true;
                ArtistInfoLoading = false;
            });
        }
        private async Task GetAlbumInfo(string artistName, string albumName, CancellationToken token)
        {
            await Core.SharedLogic.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                CheckAndCancelOperation(AlbumInfoOperation, token);
                AlbumInfoLoading = true;
                AlbumTracks?.Clear();
                AlbumFetchFailed = false;
                AlbumInfoOperation = LastfmClient.Album.GetInfoAsync(artistName, albumName, true).AsAsyncOperation();
                var albumInfoResponse = await AlbumInfoOperation;
                if (albumInfoResponse.Success)
                {
                    LastAlbum album = albumInfoResponse.Content;
                    AlbumTracks = new ThreadSafeObservableCollection<LastTrack>(album.Tracks);
                }
                else
                {
                    AlbumFetchFailed = true;
                    AlbumInfoLoading = false;
                }
                if (AlbumTracks?.Any() == false)
                    AlbumFetchFailed = true;

                AlbumInfoLoading = false;
            });
        }
        private void CheckAndCancelOperation<T>(IAsyncOperation<T> operation, CancellationToken token)
        {
            //check if there is any old operation running.
            if (operation != null && token.IsCancellationRequested && operation.Status != AsyncStatus.Completed)
            {
                //cancel old operiation
                operation.Cancel();
            }
        }       
    }
}
