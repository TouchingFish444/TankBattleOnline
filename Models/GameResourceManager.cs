using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Media;
using System.Reflection;
using System.Windows.Forms;

namespace TankBattleOnline
{
    public class GameResourceManager
    {
        public Dictionary<string, Image> Images = new Dictionary<string, Image>();
        private readonly Dictionary<string, Image> tintedImages = new Dictionary<string, Image>();
        private readonly Dictionary<string, SoundPlayer[]> soundPlayerPools = new Dictionary<string, SoundPlayer[]>();
        private readonly Dictionary<string, int> soundPoolIndexes = new Dictionary<string, int>();
        private readonly List<Stream> embeddedImageStreams = new List<Stream>();
        private readonly List<Stream> embeddedSoundStreams = new List<Stream>();

        private string BasePath
        {
            get
            {
                return Path.Combine(Application.StartupPath, "assets");
            }
        }

        public void LoadAll()
        {
            LoadTankGroup("p1", "p1tank");
            LoadTankGroup("p2", "p2tank");
            LoadTankGroup("enemy1", "enemy1");
            LoadTankGroup("enemy2", "enemy2");
            LoadTankGroup("enemy3", "enemy3");
            LoadPowerTankGroups();

            LoadImage("brick", @"img\wall\wall.gif");
            LoadImage("steel", @"img\wall\steel.gif");
            LoadImage("grass", @"img\wall\grass.gif");
            LoadImage("water", @"img\wall\water.gif");
            LoadImage("bullet", @"img\fire\tankmissile.gif");
            LoadImage("born1", @"img\other\born1.gif");
            LoadImage("born2", @"img\other\born2.gif");
            LoadImage("born3", @"img\other\born3.gif");
            LoadImage("born4", @"img\other\born4.gif");
            LoadImage("star", @"img\other\star.gif");
            LoadImage("over", @"img\other\over.gif");
            LoadSoundPool("start.wav");
            LoadSoundPool("fire.wav");
        }

        private void LoadTankGroup(string keyPrefix, string filePrefix)
        {
            LoadImage(keyPrefix + "U", @"img\tank\" + filePrefix + "U.gif");
            LoadImage(keyPrefix + "D", @"img\tank\" + filePrefix + "D.gif");
            LoadImage(keyPrefix + "L", @"img\tank\" + filePrefix + "L.gif");
            LoadImage(keyPrefix + "R", @"img\tank\" + filePrefix + "R.gif");
        }

        private void LoadPowerTankGroups()
        {
            LoadPowerTankGroup(
                "power1",
                "tugai.net.20101117235303.gif",
                "tugai.net.20101117235441.gif",
                "tugai.net.20101117235418.gif",
                "tugai.net.20101117235357.gif");

            LoadPowerTankGroup(
                "power2",
                "tugai.net.20101117235754.gif",
                "tugai.net.20101117235923.gif",
                "tugai.net.20101117235843.gif",
                "tugai.net.20101117235819.gif");

            LoadPowerTankGroup(
                "power3",
                "tugai.net.20101117235941.gif",
                "tugai.net.20101118000114.gif",
                "tugai.net.20101118000048.gif",
                "tugai.net.20101118000029.gif");
        }

        private void LoadPowerTankGroup(string keyPrefix, string upFile, string downFile, string leftFile, string rightFile)
        {
            LoadImage(keyPrefix + "U", @"img\tank\" + upFile);
            LoadImage(keyPrefix + "D", @"img\tank\" + downFile);
            LoadImage(keyPrefix + "L", @"img\tank\" + leftFile);
            LoadImage(keyPrefix + "R", @"img\tank\" + rightFile);
        }

        private void LoadImage(string key, string relativePath)
        {
            Stream embeddedStream = OpenEmbeddedAsset(relativePath);

            if (embeddedStream != null)
            {
                embeddedImageStreams.Add(embeddedStream);
                Images[key] = Image.FromStream(embeddedStream);
                return;
            }

            string fullPath = Path.Combine(BasePath, relativePath);

            if (!File.Exists(fullPath))
            {
                return;
            }

            Images[key] = Image.FromFile(fullPath);
        }

        public Image GetTankImage(int playerId, Direction direction, int upgradeLevel)
        {
            string key = GetTankImageKey(GetPowerTankPrefix(upgradeLevel), direction);
            Image image = Images.ContainsKey(key) ? Images[key] : null;

            if (image != null)
            {
                return image;
            }

            return GetTankImageByPlayer(playerId, direction);
        }

        public Image GetTankImage(int playerId, Direction direction, int upgradeLevel, int tankColorArgb)
        {
            Image image = GetTankImage(playerId, direction, upgradeLevel);

            if (image == null)
            {
                return image;
            }

            string cacheKey = "tank_" + upgradeLevel.ToString() + "_" + direction.ToString() + "_" + tankColorArgb.ToString();

            if (tintedImages.ContainsKey(cacheKey))
            {
                return tintedImages[cacheKey];
            }

            Image tintedImage = CreateTintedImage(image, Color.FromArgb(tankColorArgb));
            tintedImages[cacheKey] = tintedImage;
            return tintedImage;
        }

        private string GetPowerTankPrefix(int upgradeLevel)
        {
            if (upgradeLevel <= 0)
            {
                return "p1";
            }

            if (upgradeLevel >= 3)
            {
                return "power3";
            }

            return "power" + upgradeLevel.ToString();
        }

        private Image GetTankImageByPlayer(int playerId, Direction direction)
        {
            string prefix = "enemy3";

            if (playerId == 1)
            {
                prefix = "p1";
            }
            else if (playerId == 2)
            {
                prefix = "p2";
            }
            else if (playerId == 3)
            {
                prefix = "enemy1";
            }
            else if (playerId == 4)
            {
                prefix = "enemy2";
            }

            string key = GetTankImageKey(prefix, direction);
            return Images.ContainsKey(key) ? Images[key] : null;
        }

        private string GetTankImageKey(string prefix, Direction direction)
        {
            if (direction == Direction.Up)
            {
                return prefix + "U";
            }

            if (direction == Direction.Down)
            {
                return prefix + "D";
            }

            if (direction == Direction.Left)
            {
                return prefix + "L";
            }

            return prefix + "R";
        }

        public Image GetImage(string key)
        {
            return Images.ContainsKey(key) ? Images[key] : null;
        }

        private Image CreateTintedImage(Image source, Color color)
        {
            Bitmap sourceBitmap = new Bitmap(source);
            Bitmap result = new Bitmap(source.Width, source.Height);

            for (int y = 0; y < sourceBitmap.Height; y++)
            {
                for (int x = 0; x < sourceBitmap.Width; x++)
                {
                    Color pixel = sourceBitmap.GetPixel(x, y);

                    if (pixel.A == 0)
                    {
                        result.SetPixel(x, y, Color.Transparent);
                        continue;
                    }

                    int luminance = (pixel.R * 30 + pixel.G * 59 + pixel.B * 11) / 100;
                    double shade = 0.45 + luminance / 255.0 * 0.75;
                    int r = ClampColor((int)(color.R * shade));
                    int g = ClampColor((int)(color.G * shade));
                    int b = ClampColor((int)(color.B * shade));
                    result.SetPixel(x, y, Color.FromArgb(pixel.A, r, g, b));
                }
            }

            sourceBitmap.Dispose();
            return result;
        }

        private int ClampColor(int value)
        {
            if (value < 0)
            {
                return 0;
            }

            if (value > 255)
            {
                return 255;
            }

            return value;
        }

        public void PlaySound(string fileName)
        {
            if (!IsAllowedSound(fileName))
            {
                return;
            }

            SoundPlayer player = GetNextSoundPlayer(fileName);

            if (player == null)
            {
                return;
            }

            try
            {
                player.Play();
            }
            catch
            {
            }
        }

        private SoundPlayer GetNextSoundPlayer(string fileName)
        {
            string key = NormalizeSoundFileName(fileName);
            SoundPlayer[] pool;

            if (!soundPlayerPools.TryGetValue(key, out pool))
            {
                pool = LoadSoundPool(key);
            }

            if (pool == null || pool.Length == 0)
            {
                return null;
            }

            int index = soundPoolIndexes.ContainsKey(key) ? soundPoolIndexes[key] : 0;
            soundPoolIndexes[key] = (index + 1) % pool.Length;
            return pool[index];
        }

        private SoundPlayer[] LoadSoundPool(string fileName)
        {
            if (!IsAllowedSound(fileName))
            {
                return null;
            }

            string key = NormalizeSoundFileName(fileName);
            int channelCount = GetSoundChannelCount(key);
            List<SoundPlayer> players = new List<SoundPlayer>();

            for (int i = 0; i < channelCount; i++)
            {
                SoundPlayer player = CreateSoundPlayer(key);

                if (player != null)
                {
                    players.Add(player);
                }
            }

            if (players.Count == 0)
            {
                return null;
            }

            SoundPlayer[] pool = players.ToArray();
            soundPlayerPools[key] = pool;
            soundPoolIndexes[key] = 0;
            return pool;
        }

        private SoundPlayer CreateSoundPlayer(string fileName)
        {
            string key = NormalizeSoundFileName(fileName);
            string relativePath = Path.Combine("music", key);
            Stream embeddedStream = OpenEmbeddedAsset(relativePath);
            SoundPlayer player = null;

            if (embeddedStream != null)
            {
                player = new SoundPlayer(embeddedStream);
            }
            else
            {
                string fullPath = Path.Combine(BasePath, relativePath);

                if (!File.Exists(fullPath))
                {
                    return null;
                }

                player = new SoundPlayer(fullPath);
            }

            try
            {
                player.Load();

                if (embeddedStream != null)
                {
                    embeddedSoundStreams.Add(embeddedStream);
                }

                return player;
            }
            catch
            {
                if (embeddedStream != null)
                {
                    embeddedStream.Dispose();
                }

                return null;
            }
        }

        private int GetSoundChannelCount(string fileName)
        {
            return NormalizeSoundFileName(fileName) == "fire.wav" ? 4 : 1;
        }

        private bool IsAllowedSound(string fileName)
        {
            string key = NormalizeSoundFileName(fileName);
            return key == "start.wav" || key == "fire.wav";
        }

        private string NormalizeSoundFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return "";
            }

            return Path.GetFileName(fileName).ToLowerInvariant();
        }

        private Stream OpenEmbeddedAsset(string relativePath)
        {
            string resourceName = FindEmbeddedAssetName(relativePath);

            if (resourceName == null)
            {
                return null;
            }

            return Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
        }

        private string FindEmbeddedAssetName(string relativePath)
        {
            string target = NormalizeAssetPath(Path.Combine("assets", relativePath));
            string[] resourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();

            for (int i = 0; i < resourceNames.Length; i++)
            {
                if (NormalizeAssetPath(resourceNames[i]) == target)
                {
                    return resourceNames[i];
                }
            }

            return null;
        }

        private string NormalizeAssetPath(string path)
        {
            return path.Replace('\\', '/').ToLowerInvariant();
        }
    }
}
