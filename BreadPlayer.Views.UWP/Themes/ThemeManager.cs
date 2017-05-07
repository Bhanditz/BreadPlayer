﻿using BreadPlayer.Common;
using BreadPlayer.Core;
using BreadPlayer.Extensions;
using BreadPlayer.Helpers;
using BreadPlayer.Helpers.Interfaces;
using BreadPlayer.ViewModels;
using System;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace BreadPlayer.Themes
{
    public class ThemeManager : IThemeManager
    {
        private static readonly string[] brushKeys = new[]
        {
            //wp
            "PhoneAccentBrush",
            // windows
                   
                   "SystemControlDisabledAccentBrush" ,
                   "SystemControlForegroundAccentBrush" ,
                   "SystemControlHighlightAccentBrush" ,
                   "SystemControlHighlightAltAccentBrush",
                   "SystemControlHighlightAltListAccentHighBrush" ,
                   "SystemControlHighlightAltListAccentLowBrush" ,
                   "SystemControlHighlightAltListAccentMediumBrush",
                   "SystemControlHighlightListAccentHighBrush" ,
                   "SystemControlHighlightListAccentLowBrush" ,
                   "SystemControlHighlightListAccentMediumBrush",
                   "SystemControlHyperlinkTextBrush" ,
                   "ContentDialogBorderThemeBrush" ,
                   "JumpListDefaultEnabledBackground" ,
                   "HoverBrush" ,
                   "SystemAccentColor1",
                   "SystemControlBackgroundAccentBrush",
                   "PlaybarBrush"

        };

        public new static async void SetThemeColor(string albumartPath)
        {
            await SharedLogic.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                if (SharedLogic.SettingsVM.ChangeAccentByAlbumArt == false)
                {
                    ChangeColor(GetAccentColor());
                    return;
                }
                if (RoamingSettingsHelper.GetSetting<string>("SelectedTheme", "Light") == "Light" && SharedLogic.SettingsVM.ChangeAccentByAlbumArt)
                {
                    try
                    {
                        Color color;
                        if (!string.IsNullOrEmpty(albumartPath) && albumartPath != "default")
                            color = await SharedLogic.GetDominantColor(await StorageFile.GetFileFromPathAsync(albumartPath));
                        else if (albumartPath == "default" && Init.SharedLogic.Player.CurrentlyPlayingFile != null)
                            color = await SharedLogic.GetDominantColor(await StorageFile.GetFileFromPathAsync(Init.SharedLogic.Player.CurrentlyPlayingFile.AttachedPicture));
                        else
                            color = GetAccentColor();
                        ChangeColor(color);
                    }
                    catch (Exception ex)
                    {
                        CrossPlatformHelper.Log.E("Failed to update accent.", ex);
                        await CrossPlatformHelper.NotificationManager.ShowMessageAsync(ex.Message);
                    }
                    //ThemeChanged?.Invoke(null, new Events.ThemeChangedEventArgs(oldColor, color));
                }
                else
                    ChangeColor(GetAccentColor());
            });
        }
        private static void ChangeColor(Color color)
        {
            ChangeTitleBarColor(color);
            var oldColor = GetThemeResource<SolidColorBrush>("PlaybarBrush").Color;
            if (oldColor == color)
                return;
            GetThemeResource<SolidColorBrush>("PlaybarBrush").AnimateBrush(oldColor, color, "(SolidColorBrush.Color)");
            foreach (var brushKey in brushKeys)
            {
                if (Application.Current.Resources.ContainsKey(brushKey))
                {
                    ((SolidColorBrush)App.Current.Resources[brushKey]).Color = color;
                }
            }
        }
        private static void ChangeTitleBarColor(Color color)
        {
            if (Windows.Foundation.Metadata.ApiInformation.IsApiContractPresent("Windows.Phone.PhoneContract", 1))
            {
                var statusBar = StatusBar.GetForCurrentView();
                statusBar.BackgroundColor = color;
                statusBar.BackgroundOpacity = 1;
            }
            else
            {
                ApplicationView.GetForCurrentView().TitleBar.BackgroundColor = Colors.Transparent;
                ApplicationView.GetForCurrentView().TitleBar.ButtonBackgroundColor = Colors.Transparent;
                ApplicationView.GetForCurrentView().TitleBar.ButtonForegroundColor = Colors.Black;
                ApplicationView.GetForCurrentView().TitleBar.ButtonHoverBackgroundColor = color;
                ApplicationView.GetForCurrentView().TitleBar.ButtonPressedForegroundColor = Colors.White;
            }
        }
        private static Color GetAccentColor()
        {
            return ((Color)App.Current.Resources["SystemAccentColor"]);
        }
        private static T GetThemeResource<T>(string key)
        {
            return ((T)App.Current.Resources[key]);
        }

        // public static event OnThemeChanged ThemeChanged;
    }
    // public delegate void OnThemeChanged(object sender, Events.ThemeChangedEventArgs e);
}
