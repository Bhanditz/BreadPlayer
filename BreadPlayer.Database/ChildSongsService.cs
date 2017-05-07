﻿using BreadPlayer.Models;
using BreadPlayer.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreadPlayer.Database
{
    public class ChildSongsService
    {
        string tablename, texttablename;
        private IDatabaseService Database
        {
            get; set;
        }
        public ChildSongsService(IDatabaseService database, string tableName, string textTableName)
        {
            Database = database;
            tablename = tableName;
            texttablename = textTableName;
        }
        public async Task InsertTracksAsync(IEnumerable<Mediafile> fileCol, IDBRecord pList)
        {
            List<ChildSong> PlaylistSongs = new List<ChildSong>();
            foreach (var file in fileCol)
            {
                PlaylistSongs.Add(new ChildSong()
                {
                    SongId = file.Id,
                    PlaylistId = pList.Id
                });
            }
            Database.ChangeTable(tablename, texttablename);
            await Database.InsertRecords(PlaylistSongs);
        }
        public void InsertSong(ChildSong file)
        {
            Database.ChangeTable(tablename, texttablename);
            Database.InsertRecord(file);
        }
        public async Task RemoveSongAsync(Mediafile file)
        {
            Database.ChangeTable(tablename, texttablename);
            var record = await Database.GetRecordByQueryAsync<ChildSong>(string.Format("songid={0}", file.Id));
            await Database.RemoveRecord(record);
        }
        public bool Exists(long id)
        {
            Database.ChangeTable(tablename, texttablename);
            return Database.CheckExists(id);
        }
        public async Task<IEnumerable<Mediafile>> GetTracksAsync(long parentID)
        {
            return await Task.Run(async () =>
            {
                Database.ChangeTable(tablename, texttablename);
                var trackIds = (await Database.QueryRecords<ChildSong>(string.Format("pId={0}", parentID))).Select(t => t.SongId);
                Database.ChangeTable("Tracks", "TracksText");
                List<Mediafile> Tracks = new List<Mediafile>();
                foreach (var id in trackIds)
                {
                    Tracks.Add(Database.GetRecordById<Mediafile>(id));
                }
                return Tracks;
            });
        }
    }
}
