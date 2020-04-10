using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using XI.CommonTypes;

namespace XI.Demos.Models
{
    public class LocalDemo : IDemo
    {
        private readonly bool _isCorrupted;

        /// <summary>
        ///     Initializes a new instance of the <see cref="LocalDemo" /> class.
        /// </summary>
        /// <param name="path">The path to the demo file.</param>
        /// <param name="version">The version of the demo.</param>
        public LocalDemo(string path, GameType version)
        {
            Path = path;
            Version = version;

            try
            {
                using (var stream = Open())
                {
                    var reader = new DemoReader(stream, Version);

                    var config = reader.ReadConfiguration();

                    config.TryGetValue("mapname", out var map);
                    config.TryGetValue("fs_game", out var mod);
                    config.TryGetValue("g_gametype", out var gameType);
                    config.TryGetValue("sv_hostname", out var server);
                    config.TryGetValue("sv_referencedIwdNames", out var iwds);
                    config.TryGetValue("sv_referencedFFNames", out var ffs);

                    if (!string.IsNullOrWhiteSpace(mod) && mod.ToLower().StartsWith("mods/"))
                        mod = mod.Substring(5);

                    Map = map;
                    Mod = mod;
                    GameType = gameType;
                    Server = server;
                    IWDs = iwds == null ? new string[0] : iwds.Split(' ');
                    FFs = ffs == null
                        ? new string[0]
                        : ffs.Split(' ').Select(ff =>
                        {
                            // Change path of usermaps files to full paths. 
                            // e.g. usermaps/mp_caen2_load -> usermaps/mp_caen2/mp_caen2_load

                            if (!ff.StartsWith("usermaps/mp_"))
                                return ff;

                            var mapName = ff.Split('/').Last();
                            if (mapName.EndsWith("_load"))
                            {
                                mapName = mapName.Substring(0, mapName.Length - 5);
                                return string.Format("usermaps/{0}/{0}_load", mapName);
                            }

                            return string.Format("usermaps/{0}/{0}", mapName);
                        });
                }
            }
            catch (Exception)
            {
                Map = "???";
                Mod = "???";
                GameType = "???";
                Server = "File corrupted!";
                IWDs = new string[0];
                FFs = new string[0];
                _isCorrupted = true;
            }
        }

        /// <summary>
        ///     Gets the path to the demo file.
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        ///     Gets a collection of IWD files referenced by the demo.
        /// </summary>
        public IEnumerable<string> IWDs { get; }

        /// <summary>
        ///     Gets a collection of FF files referenced by the demo.
        /// </summary>
        public IEnumerable<string> FFs { get; }

        /// <summary>
        ///     Gets a value indicating whether this instance is valid.
        /// </summary>
        public bool IsValid => File.Exists(Path) && !_isCorrupted;

        #region Overrides of Object

        public override string ToString()
        {
            return Name;
        }

        #endregion

        /// <summary>
        ///     Deletes this demo file.
        /// </summary>
        public void Delete()
        {
            File.Delete(Path);
        }

        #region Implementation of IDemo

        /// <summary>
        ///     Gets the version of this instance.
        /// </summary>
        public GameType Version { get; }

        /// <summary>
        ///     Gets or sets the name of this instance.
        /// </summary>
        public string Name
        {
            get => System.IO.Path.GetFileNameWithoutExtension(Path);
            set
            {
                var newPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Path),
                    $"{value}{System.IO.Path.GetExtension(Path)}");

                File.Move(Path, newPath);

                Path = newPath;
            }
        }

        /// <summary>
        ///     Gets the UTC date this instance was recorded at.
        /// </summary>
        public DateTime Date => File.GetCreationTimeUtc(Path);

        /// <summary>
        ///     Gets the map this instance was recorded in.
        /// </summary>
        public string Map { get; }

        /// <summary>
        ///     Gets the mod this instance was recorded in.
        /// </summary>
        public string Mod { get; }

        /// <summary>
        ///     Gets the game type this instance was recorded in.
        /// </summary>
        public string GameType { get; }

        /// <summary>
        ///     Gets the server this instance was recorded on.
        /// </summary>
        public string Server { get; }

        /// <summary>
        ///     Gets the size of the file.
        /// </summary>
        public long Size => new FileInfo(Path).Length;

        /// <summary>
        ///     Opens a stream of the demo file.
        /// </summary>
        /// <returns>The stream of the demo file.</returns>
        public Stream Open()
        {
            return File.OpenRead(Path);
        }

        #endregion

        #region Equality members

        protected bool Equals(LocalDemo other)
        {
            return string.Equals(Path, other.Path);
        }

        /// <summary>
        ///     Determines whether the specified <see cref="T:System.Object" /> is equal to the current
        ///     <see cref="T:System.Object" />.
        /// </summary>
        /// <returns>
        ///     true if the specified object  is equal to the current object; otherwise, false.
        /// </returns>
        /// <param name="obj">The object to compare with the current object. </param>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((LocalDemo) obj);
        }

        /// <summary>
        ///     Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        ///     A hash code for the current <see cref="T:System.Object" />.
        /// </returns>
        public override int GetHashCode()
        {
            return Path != null ? Path.GetHashCode() : 0;
        }

        #endregion
    }
}