﻿using BreadPlayer.Core.Models;
using BreadPlayer.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace BreadPlayer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FavoritesView : Page
    {
        public FavoritesView()
        {
            this.InitializeComponent();
            favoritesListView.DataContext = this;
        }
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            await GetFavoriteSongs().ConfigureAwait(false);
        }
        public ThreadSafeObservableCollection<Mediafile> _favoriteSongsCollection;
        public ThreadSafeObservableCollection<Mediafile> FavoriteSongsCollection =>
            _favoriteSongsCollection ?? (_favoriteSongsCollection = new ThreadSafeObservableCollection<Mediafile>());

        private Task<ThreadSafeObservableCollection<Mediafile>> GetFavoriteSongs()
        {
            return Task.Run(() =>
            {
                FavoriteSongsCollection.AddRange(SettingsViewModel.TracksCollection.Elements.Where(t => t.IsFavorite));
                return FavoriteSongsCollection;
            });
        }
    }
}
