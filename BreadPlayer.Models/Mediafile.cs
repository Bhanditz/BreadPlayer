﻿/* 
	BreadPlayer. A music player made for Windows 10 store.
    Copyright (C) 2016  theweavrs (Abdullah Atta)

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using BreadPlayer.Models.Common;
using BreadPlayer.Models.Interfaces;
using Newtonsoft.Json;
using System;

namespace BreadPlayer.Models
{
    public class Mediafile : ObservableObject, IComparable<Mediafile>, IDBRecord
    {
        #region Fields
        private PlayerState state = PlayerState.Stopped;
        private string path;
        private string encrypted_meta_file;
        private string attached_picture;
        private string comment;
        private string folderPath;
        private string synchronized_lyric;
        private string album;
        private string beatsperminutes;
        private string composer;
        private string genre;
        private string copyright_message;
        private string date;
        private string encoded_by;
        private string lyric;
        private string content_group_description;
        private string title;
        private string subtitle;
        private string length;
        private string orginal_filename;
        private string lead_artist;
        private string publisher;
        private string track_number;
        private string size;
        private string year;
        private string NaN = "NaN";
        private int playCount;
        #endregion

        #region Properties
        public long Id { get; set; }
        string lastPlayed;
        public string LastPlayed { get => lastPlayed; set => Set(ref lastPlayed, value); }

        string addedDate;
        public string AddedDate { get => addedDate; set => Set(ref addedDate, value); }
        bool isFavorite;
        public bool IsFavorite
        {
            get => isFavorite;
            set => Set(ref isFavorite, value);
        }
        public int PlayCount { get => playCount; set => Set(ref playCount, value); }
        public string Path { get => path; set => Set(ref path, value); }
        //public long Id { get => id; set => Set(ref id, value); }
        public string AttachedPicture { get => attached_picture; set => Set(ref attached_picture, value); }
        public string FolderPath { get => folderPath; set => folderPath = string.IsNullOrEmpty(value) ? folderPath = "" : value; }
        public string Album { get => album; set => album = string.IsNullOrEmpty(value) ? album = "Unknown Album" : value; }
        public string Genre { get => genre; set => genre = string.IsNullOrEmpty(value) ? genre = "Other" : value; }
        public string Title { get => title; set => title = string.IsNullOrEmpty(value) ? title = System.IO.Path.GetFileNameWithoutExtension(path) : value; }
        public string TrackNumber { get => track_number; set => track_number = string.IsNullOrEmpty(value) ? track_number = NaN : value; }
        public string Year { get => year; set => year = value == "0" || string.IsNullOrEmpty(value) ? "" : value; }
        public string LeadArtist { get => lead_artist; set => lead_artist = string.IsNullOrEmpty(value) ? lead_artist = NaN : value; }
        public string OrginalFilename { get => orginal_filename; set => orginal_filename = string.IsNullOrEmpty(value) ? orginal_filename = NaN : value; }
        public string Length { get => length; set => length = string.IsNullOrEmpty(value) ? length = NaN : value; }

        #region JsonIgnore Properties
        [JsonIgnore]
        public string Comment { get => comment; set => comment = string.IsNullOrEmpty(value) ? comment = NaN : value; }
        [JsonIgnore]
        public string SynchronizedLyric
        {
            get => synchronized_lyric; set => synchronized_lyric = string.IsNullOrEmpty(value) ? synchronized_lyric = NaN : value;
        }
        [JsonIgnore]
        public PlayerState State { get => state; set => Set(ref state, value); }
        [JsonIgnore]
        public string EncryptedMetaFile { get => encrypted_meta_file; set => encrypted_meta_file = string.IsNullOrEmpty(value) ? encrypted_meta_file = NaN : value; }
        [JsonIgnore]
        public string Size { get => size; set => size = string.IsNullOrEmpty(value) ? size = NaN : value; }

        [JsonIgnore]
        public string Publisher { get => publisher; set => publisher = string.IsNullOrEmpty(value) ? publisher = NaN : value; }
        [JsonIgnore]
        public string Subtitle { get => subtitle; set => subtitle = string.IsNullOrEmpty(value) ? subtitle = NaN : value; }
        [JsonIgnore]
        public string CopyrightMessage { get => copyright_message; set => copyright_message = string.IsNullOrEmpty(value) ? copyright_message = NaN : value; }
        [JsonIgnore]
        public string Date { get => date; set => date = string.IsNullOrEmpty(value) ? date = NaN : value; }
        [JsonIgnore]
        public string EncodedBy { get => encoded_by; set => encoded_by = string.IsNullOrEmpty(value) ? encoded_by = NaN : value; }
        [JsonIgnore]
        public string Lyric { get => lyric; set => lyric = string.IsNullOrEmpty(value) ? lyric = NaN : value; }
        [JsonIgnore]
        public string ContentGroupDescription { get => content_group_description; set => content_group_description = string.IsNullOrEmpty(value) ? content_group_description = NaN : value; }
        [JsonIgnore]
        public string BeatsPerMinutes { get => beatsperminutes; set => beatsperminutes = string.IsNullOrEmpty(value) ? beatsperminutes = NaN : value; }
        [JsonIgnore]
        public string Composer { get => composer; set => composer = string.IsNullOrEmpty(value) ? composer = NaN : value; }
        #endregion

        #endregion

        public int CompareTo(Mediafile compareTo)
        {
            return this.Title.CompareTo(compareTo.Title);
        }

        public string GetTextSearchKey()
        {
            return string.Format("id={0} {1} {2} {3} {4} {5} {6}", Id, Title, Album, LeadArtist, Year, Genre, FolderPath);
        }
    }
}
