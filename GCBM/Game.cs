﻿using GCBM.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using sio = System.IO;
using ste = System.Text.Encoding;

namespace GCBM;

public class Game
{
    public delegate int ModCB(int val);

    //public delegate void ResetProgressBarCB(int min, int max, int val);
    //public delegate void UpdateProgressBarCB(int val);
    //public delegate void UpdateActionLabelCB(string text);
    public delegate void ResetControlsCB(bool error, string errorText);

    public delegate string ShowMTFolderDialogCB(string path);

    public delegate bool ShowMTMBoxCB(string text, string caption, MessageBoxButtons btns, MessageBoxIcon icon,
        MessageBoxDefaultButton defBtn, DialogResult desRes);

    private const string WIITDB_FILE = "wiitdb.xml";
    private static readonly string RES_PATH;
    private static string IMAGE_PATH;
    private readonly bool RETRIEVE_FILES_INFO = true;
    private char REGION;
    private bool ROOT_OPENED = true;


    public TOCClass toc;

    public void GetFilDirInfo(sio.DirectoryInfo pDir, ref int itemNum, ref int filePos)
    {
        TOCItemFil tif;
        var tocDirIdx = itemNum - 1;

        var dirs = pDir.GetDirectories();
        for (var cnt = 0; cnt < dirs.Length; cnt++)
            if (dirs[cnt].Name.ToLower() == "&&systemdata")
            {
                (dirs[0], dirs[cnt]) = (dirs[cnt], dirs[0]);
                break;
            }

        foreach (var t in dirs)
        {
            tif = new TOCItemFil(itemNum, tocDirIdx, tocDirIdx, 0, true,
                t.Name, t.FullName.Replace(RES_PATH, ""), t.FullName);
            toc.fils.Add(tif);
            itemNum += 1;
            toc.dirCount += 1;
            GetFilDirInfo(t, ref itemNum, ref filePos);
        }

        var fils = pDir.GetFiles();
        foreach (var t in fils)
        {
            tif = new TOCItemFil(itemNum, tocDirIdx, filePos, (int)t.Length, false,
                t.Name, t.FullName.Replace(RES_PATH, ""), t.FullName);
            toc.fils.Add(tif);
            toc.fils[0].len = toc.fils.Count;
            filePos += 2;
            itemNum += 1;
            toc.filCount += 1;
        }

        toc.fils[tocDirIdx].len = itemNum;
    }

    public bool GenerateTreeView(bool fileNameSort)
    {
        var tns = new List<TreeNode>();
        TreeNode tnn;

        var tn = new TreeNode(toc.fils[0].name, 0, 0)
        {
            Name = toc.fils[0].TOCIdx.ToString(),
            ToolTipText = RES_PATH
        };
        toc.fils[0].node = tn;
        tns.Add(tn);

        if (fileNameSort)
        {
            for (var i = 1; i < toc.fils.Count; i++)
            {
                int j;
                if (toc.fils[i].isDir)
                {
                    for (j = 0; j < tns.Count; j++)
                        if (tns[j].Name == toc.fils[i].dirIdx.ToString())
                            break;

                    if (j == tns.Count)
                    {
                        _ = MessageBox.Show("GenerateTreeView() error: dir2dir not found", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }

                    tn = tns[j];
                    tnn = new TreeNode(toc.fils[i].name, 1, 2)
                    {
                        Name = toc.fils[i].TOCIdx.ToString(),
                        ToolTipText = toc.fils[i].path,
                        Tag = i
                    };
                    toc.fils[i].node = tnn;
                    tns.Add(tnn);
                    _ = tn.Nodes.Add(tnn);
                }
                else
                {
                    for (j = 0; j < tns.Count; j++)
                        if (tns[j].Name == toc.fils[i].dirIdx.ToString())
                            break;

                    if (j == tns.Count)
                    {
                        _ = MessageBox.Show("GenerateTreeView() error: dir2fil not found", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }

                    tn = tns[j];
                    tnn = new TreeNode(toc.fils[i].name, 3, 3)
                    {
                        Name = toc.fils[i].TOCIdx.ToString(),
                        ToolTipText = toc.fils[i].path,
                        Tag = i
                    };
                    toc.fils[i].node = tnn;
                    _ = tn.Nodes.Add(tnn);
                }
            }
        }
        else
        {
            var idx = 2;
            for (var i = 1; i < toc.fils.Count; i++)
                if (!toc.fils[i].isDir)
                {
                    tnn = new TreeNode(toc.fils[idx].gamePath, 3, 3)
                    {
                        Name = toc.fils[idx].TOCIdx.ToString(),
                        ToolTipText = toc.fils[idx].path,
                        Tag = idx
                    };
                    toc.fils[idx].node = tnn;
                    _ = tn.Nodes.Add(tnn);
                    if (toc.fils[idx].name == "opening.bnr")
                    {
                    }

                    idx = toc.fils[i].nextIdx;
                }
        }

        return true;
    }

    public bool ReadImageTOC()
    {
        sio.FileStream fsr;

        var itemIsDir = false;
        var itemGamePath = "";

        var dirEntry = new int[512];
        var dirEntryCount = 0;
        dirEntry[1] = 99999999;

        var error = false;
        var errorText = "";

        toc = new TOCClass(RES_PATH);
        var itemNum = toc.fils.Count;
        var shift = toc.fils.Count - 1;

        try
        {
            fsr = new sio.FileStream(IMAGE_PATH, sio.FileMode.Open, sio.FileAccess.Read, sio.FileShare.Read);
        }
        catch (sio.IOException)
        {
            error = true;
            errorText = Resources.CantOpenImage;
            return false;
        }

        var brr = new sio.BinaryReader(fsr, ste.Default);

        if (fsr.Length > 0x0438)
        {
            fsr.Position = 0x0400;
            toc.fils[2].pos = 0x0;
            toc.fils[2].len = 0x2440;
            toc.fils[3].pos = 0x2440;
            toc.fils[3].len = brr.ReadInt32BE();
            fsr.Position += 0x1c;
            toc.fils[4].pos = brr.ReadInt32BE();
            toc.fils[5].pos = brr.ReadInt32BE();
            toc.fils[5].len = brr.ReadInt32BE();
            toc.fils[4].len = toc.fils[5].pos - toc.fils[4].pos;
            fsr.Position += 0x08;
            toc.dataStart = brr.ReadInt32BE();

            toc.totalLen = (int)fsr.Length;
        }
        else
        {
            errorText = Resources.ReadImage_String1 + " " + IMAGE_PATH;
            error = true;
        }

        if (fsr.Length < toc.dataStart && !IMAGE_PATH.ToLower().EndsWith(".nkit.iso"))
        {
            errorText = Resources.ReadImage_String1 + " " + IMAGE_PATH + ": expected length of " + toc.dataStart +
                        " but got " + fsr.Length + ".";
            error = true;
        }

        if (!error)
        {
            fsr.Position = toc.fils[5].pos;
            var msr = new sio.MemoryStream(brr.ReadBytes(toc.fils[5].len));
            var mbr = new sio.BinaryReader(msr, ste.Default);

            var i = mbr.ReadInt32();
            if (i != 1)
            {
                error = true;
                errorText = Resources.ReadImage_String2;
            }

            i = mbr.ReadInt32();
            if (i != 0)
            {
                error = true;
                errorText = Resources.ReadImage_String2;
            }

            var namesTableEntryCount = mbr.ReadInt32BE() - 1;
            var namesTableStart = namesTableEntryCount * 12 + 12;

            for (var cnt = 0; cnt < namesTableEntryCount; cnt++)
            {
                var itemNamePtr = mbr.ReadInt32BE();
                if (itemNamePtr >> 0x18 == 1) itemIsDir = true;

                itemNamePtr &= 0x00ffffff;
                var itemPos = mbr.ReadInt32BE();
                var itemLen = mbr.ReadInt32BE();
                var prevPos = msr.Position;
                long newPos = namesTableStart + itemNamePtr;
                msr.Position = newPos;
                var itemName = mbr.ReadStringNT();
                msr.Position = prevPos;

                while (dirEntry[dirEntryCount + 1] <= itemNum) dirEntryCount -= 2;

                if (itemIsDir)
                {
                    dirEntryCount += 2;
                    dirEntry[dirEntryCount] = itemPos > 0 ? itemPos + shift : itemPos;
                    itemPos += shift;
                    itemLen += shift;
                    dirEntry[dirEntryCount + 1] = itemLen;
                    toc.dirCount += 1;
                }
                else
                {
                    toc.filCount += 1;
                }

                var itemPath = itemName;
                var j = dirEntry[dirEntryCount];
                for (i = 0; i < 256; i++)
                    if (j == 0)
                    {
                        itemGamePath = itemPath;
                        itemPath = RES_PATH + itemPath;
                        break;
                    }
                    else
                    {
                        itemPath = itemPath.Insert(0, toc.fils[j].name + sio.Path.DirectorySeparatorChar);
                        j = toc.fils[j].dirIdx;
                    }

                if (itemIsDir) itemPath += sio.Path.DirectorySeparatorChar;

                if (RETRIEVE_FILES_INFO)
                {
                    if (!itemIsDir)
                        if (fsr.Length < itemPos + itemLen)
                        {
                            errorText = string.Format(Resources.ReadImage_String3, itemPath);
                            error = true;
                        }

                    if (error) break;
                }

                var tif = new TOCItemFil(itemNum, dirEntry[dirEntryCount], itemPos, itemLen, itemIsDir, itemName,
                    itemGamePath, itemPath);
                toc.fils.Add(tif);
                toc.fils[0].len = toc.fils.Count;

                if (itemIsDir)
                {
                    dirEntry[dirEntryCount] = itemNum;
                    itemIsDir = false;
                }

                itemNum += 1;
            }

            mbr.Close();
            msr.Close();
        }

        brr.Close();
        fsr.Close();

        if (error)
        {
            _ = MessageBox.Show(errorText, Resources.ReadImage_String4, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        //        private void DisplaySourceFiles(string sourceFolder)

        ROOT_OPENED = false;
        LoadInfo(!ROOT_OPENED);

        return error;
    }

    //*****************************
    public bool ReadImageDiscTOC()
    {
        var itemIsDir = false;
        var itemGamePath = "";

        var dirEntry = new int[512];
        var dirEntryCount = 0;
        dirEntry[1] = 99999999;

        var error = false;
        var errorText = "";

        toc = new TOCClass(RES_PATH);
        var itemNum = toc.fils.Count;
        var shift = toc.fils.Count - 1;

        var fsr = new sio.FileStream(IMAGE_PATH, sio.FileMode.Open, sio.FileAccess.Read, sio.FileShare.Read);
        var brr = new sio.BinaryReader(fsr, ste.Default);

        if (fsr.Length > 0x0438)
        {
            fsr.Position = 0x0400;
            toc.fils[2].pos = 0x0;
            toc.fils[2].len = 0x2440;
            toc.fils[3].pos = 0x2440;
            toc.fils[3].len = brr.ReadInt32BE();
            fsr.Position += 0x1c;
            toc.fils[4].pos = brr.ReadInt32BE();
            toc.fils[5].pos = brr.ReadInt32BE();
            toc.fils[5].len = brr.ReadInt32BE();
            toc.fils[4].len = toc.fils[5].pos - toc.fils[4].pos;
            fsr.Position += 0x08;
            toc.dataStart = brr.ReadInt32BE();

            toc.totalLen = (int)fsr.Length;
        }
        else
        {
            errorText = Resources.ReadImage_String1 + " " + IMAGE_PATH;
            error = true;
        }

        if (fsr.Length < toc.dataStart && !IMAGE_PATH.ToLower().EndsWith(".nkit.iso"))
        {
            errorText = Resources.ReadImage_String1 + " " + IMAGE_PATH + ": expected length of " + toc.dataStart +
                        " but got " + fsr.Length + ".";
            error = true;
        }

        if (!error)
        {
            fsr.Position = toc.fils[5].pos;
            var msr = new sio.MemoryStream(brr.ReadBytes(toc.fils[5].len));
            var mbr = new sio.BinaryReader(msr, ste.Default);

            var i = mbr.ReadInt32();
            if (i != 1)
            {
                error = true;
                errorText = Resources.ReadImage_String2;
            }

            i = mbr.ReadInt32();
            if (i != 0)
            {
                error = true;
                errorText = Resources.ReadImage_String2;
            }

            var namesTableEntryCount = mbr.ReadInt32BE() - 1;
            var namesTableStart = namesTableEntryCount * 12 + 12;

            for (var cnt = 0; cnt < namesTableEntryCount; cnt++)
            {
                var itemNamePtr = mbr.ReadInt32BE();
                if (itemNamePtr >> 0x18 == 1) itemIsDir = true;

                itemNamePtr &= 0x00ffffff;
                var itemPos = mbr.ReadInt32BE();
                var itemLen = mbr.ReadInt32BE();
                var prevPos = msr.Position;
                long newPos = namesTableStart + itemNamePtr;
                msr.Position = newPos;
                var itemName = mbr.ReadStringNT();
                msr.Position = prevPos;

                while (dirEntry[dirEntryCount + 1] <= itemNum) dirEntryCount -= 2;

                if (itemIsDir)
                {
                    dirEntryCount += 2;
                    dirEntry[dirEntryCount] = itemPos > 0 ? itemPos + shift : itemPos;
                    itemPos += shift;
                    itemLen += shift;
                    dirEntry[dirEntryCount + 1] = itemLen;
                    toc.dirCount += 1;
                }
                else
                {
                    toc.filCount += 1;
                }

                var itemPath = itemName;
                var j = dirEntry[dirEntryCount];
                for (i = 0; i < 256; i++)
                    if (j == 0)
                    {
                        itemGamePath = itemPath;
                        itemPath = RES_PATH + itemPath;
                        break;
                    }
                    else
                    {
                        itemPath = itemPath.Insert(0, toc.fils[j].name + sio.Path.DirectorySeparatorChar);
                        j = toc.fils[j].dirIdx;
                    }

                if (itemIsDir) itemPath += sio.Path.DirectorySeparatorChar;

                if (RETRIEVE_FILES_INFO)
                {
                    if (!itemIsDir)
                        if (fsr.Length < itemPos + itemLen)
                        {
                            errorText = string.Format(Resources.ReadImage_String3, itemPath);
                            error = true;
                        }

                    if (error) break;
                }

                var tif = new TOCItemFil(itemNum, dirEntry[dirEntryCount], itemPos, itemLen, itemIsDir, itemName,
                    itemGamePath, itemPath);
                toc.fils.Add(tif);
                toc.fils[0].len = toc.fils.Count;

                if (itemIsDir)
                {
                    dirEntry[dirEntryCount] = itemNum;
                    itemIsDir = false;
                }

                itemNum += 1;
            }

            mbr.Close();
            msr.Close();
        }

        brr.Close();
        fsr.Close();

        if (error)
        {
            _ = MessageBox.Show(errorText, Resources.ReadImage_String4, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        //        private void DisplaySourceFiles(string sourceFolder)

        ROOT_OPENED = false;
        LoadInfo(!ROOT_OPENED);

        return error;
    }

    #region Properties

    public string Title { get; set; }
    public string InternalName { get; set; }
    public string ID { get; set; }
    public string Region { get; set; }
    public string Extension { get; set; }
    public int Size { get; set; }
    public string Path { get; set; }
    public string IDMakerCode { get; set; }
    public string IDGameCode { get; set; }
    public string IDRegionCode { get; set; }
    public string DiscID { get; set; }
    public string Date { get; set; }

    #endregion

    #region fromInfo.cs

    public byte b;
    public byte[] bb;

    public sio.BinaryReader br;
    //sio.MemoryStream bnr = null;
    //sio.BinaryReader bnrr = null;

    public sio.FileStream fs;
    public string loadPath;

    public string _IDMakerCode { get; private set; }
    public string _IDRegionCode { get; private set; }
    public string _oldNameInternal { get; private set; }

    public string _IDRegionName { get; private set; }

    public async void LoadInfo(string path, bool useXmlTitle)
    {
        await GetGameInfo(path, useXmlTitle).ConfigureAwait(false);
    }

    public async void LoadInfo(bool image)
    {
    }

    #endregion

    #region Constructors

    public Game()
    {
    }

    public Task<Game> GetGameInfo(string path, bool useXmlTitle)
    {
        var game = new Game();
        IMAGE_PATH = path;
        loadPath = true ? IMAGE_PATH : toc.fils[2].path;
        fs = new sio.FileStream(loadPath, sio.FileMode.Open, sio.FileAccess.Read, sio.FileShare.Read);
        br = new sio.BinaryReader(fs, ste.Default);

        bb = br.ReadBytes(4); // 4
        //tbIDGameCode.Text = SIOExtensions.ToStringC(ste.Default.GetChars(bb));
        game.IDGameCode = SIOExtensions.ToStringC(ste.Default.GetChars(bb)); // ID Game Code - String

        game.IDRegionCode = Convert.ToString(ste.Default.GetChars(new[] { bb[3] })[0]).ToLower();

        switch (Convert.ToString(ste.Default.GetChars(new[] { bb[3] })[0]).ToLower())
        {
            //case "e":
            //    game.Region = "USA/NTSC-U";
            //    REGION = 'u';
            //    break;
            //case "j":
            //    game.Region = "JAP/NTSC-J";
            //    REGION = 'j';
            //    break;
            //case "p":
            //    game.Region = "EUR/PAL";
            //    REGION = 'e';
            //    break;
            //default:
            //    game.Region = "UNK";
            //    REGION = 'n';
            //    break;
            case "e": // AMERICA - USA
                game.Region = "USA/NTSC-U";
                REGION = 'u';
                break;
            case "j": // ASIA - JAPAN
            case "t": // ASIA - TAIWAN
            case "k": // ASIA - KOREA
                game.Region = "JAP/NTSC-J";
                REGION = 'j';
                break;
            case "p": // EUROPE - ALL
            case "f": // EUROPE - FRANCE
            case "d": // EUROPE - GERMANY
            case "s": // EUROPE - SPAIN
            case "i": // EUROPE - ITALY
            case "r": // EUROPE - RUSSIA
            case "y": // EUROPE - France, Belgium, Netherlands ???
                game.Region = "EUR/PAL";
                REGION = 'e';
                break;
            case "u": // AUSTRALIA
                game.Region = "AUS/PAL";
                REGION = 'e';
                break;
            default:
                game.Region = "UNK (EUR/PAL?)";
                REGION = 'n';
                break;
        }

        //Catch GAMEREGION
        bb = br.ReadBytes(2); // 2
        game.IDMakerCode = SIOExtensions.ToStringC(ste.Default.GetChars(bb)); // ID Maker Code - String

        b = br.ReadByte();
        game.DiscID = string.Format("0x{0:x2}", b);
        fs.Position += 0x19;


        //Catch GAMETITLE

        game.InternalName = br.ReadStringNT();

        game.Title = game.InternalName;
        game.ID = game.IDGameCode + game.IDMakerCode; // GameID (IDGameCode + IDMakerCode)
        //Catch GAMEID here

        if (useXmlTitle) game.Title = getXmlTitle(game.ID);
        br.Close();
        fs.Close();

        loadPath = true ? IMAGE_PATH : toc.fils[3].path;

        fs = new sio.FileStream(loadPath, sio.FileMode.Open, sio.FileAccess.Read, sio.FileShare.Read);
        br = new sio.BinaryReader(fs, ste.Default);
        //if (true)
        //{
        //    fs.Position = toc.fils[3].pos;
        //}

        game.Date = br.ReadStringNT();

        br.Close();
        fs.Close();
        var f = new sio.FileInfo(path);
        game.Extension = f.Extension;
        game.Size = Convert.ToInt32(f.Length);
        game.Path = path;

        br.Close();
        fs.Close();
        return Task.FromResult(game);
    }

    public string getXmlTitle(string id)
    {
        string title = "";
            if (sio.File.Exists(WIITDB_FILE))
            {
                var root = XElement.Load(WIITDB_FILE);
                IEnumerable<XElement> tests = root.Elements("game").AsParallel()
                    .Where(el => (string)el.Element("id") == id);
                foreach (var el in tests) title = (string)el.Element("locale")?.Element("title");
            }
            else
            {
                CheckWiiTdbXml();
                title = "error";
            }

            return title;

    }

    public Game(string Title, string ID, string Region, string Extension, int Size, string Path)
    {
        this.Title = Title;
        this.ID = ID;
        this.Region = Region;
        this.Extension = Extension;
        this.Size = Size;
        this.Path = Path;
    }

    #endregion

    #region Methods

    /// <summary>
    ///     Inform the user they should download WiiTDB.
    /// </summary>
    public void CheckWiiTdbXml()
    {
        _ = MessageBox.Show(Resources.ProcessTaskDelay_String1 + Environment.NewLine +
                            Resources.ProcessTaskDelay_String2 +
                            Environment.NewLine + Environment.NewLine +
                            Resources.ProcessTaskDelay_String3 +
                            Environment.NewLine + Environment.NewLine +
                            Resources.ProcessTaskDelay_String4,
            Resources.Notice, MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    public override string ToString()
    {
        return Title + " [" + ID + "]";
    }

    #endregion

    #region TOC class

    public class TOCClass : IComparer<TOCItemFil>, ICloneable
    {
        public readonly List<TOCItemFil> fils;
        public int dataStart;
        public int dirCount = 1;
        public int filCount = 4;
        public int lastIdx;
        public int startIdx;
        public int totalLen;

        public TOCClass(string resPath)
        {
            fils = new List<TOCItemFil>
            {
                new(0, 0, 0, 99999, true, "root", "", resPath),
                new(1, 0, 0, 6, true, "&&SystemData", "&&systemdata" + sio.Path.DirectorySeparatorChar,
                    resPath + "&&systemdata" + sio.Path.DirectorySeparatorChar),
                new(2, 1, 0, 99999, false, "ISO.hdr", "&&SystemData" + sio.Path.DirectorySeparatorChar + "iso.hdr",
                    resPath + "&&SystemData" + sio.Path.DirectorySeparatorChar + "iso.hdr"),
                new(3, 1, 9280, 99999, false, "AppLoader.ldr",
                    "&&SystemData" + sio.Path.DirectorySeparatorChar + "apploader.ldr",
                    resPath + "&&SystemData" + sio.Path.DirectorySeparatorChar + "apploader.ldr"),
                new(4, 1, 0, 99999, false, "Start.dol", "&&SystemData" + sio.Path.DirectorySeparatorChar + "start.dol",
                    resPath + "&&SystemData" + sio.Path.DirectorySeparatorChar + "start.dol"),
                new(5, 1, 0, 99999, false, "Game.toc", "&&SystemData" + sio.Path.DirectorySeparatorChar + "game.toc",
                    resPath + "&&SystemData" + sio.Path.DirectorySeparatorChar + "game.toc")
            };

            totalLen = 0;
            dataStart = totalLen;
            startIdx = totalLen;
        }

        #region ICloneable Members

        public object Clone()
        {
            var res = new TOCClass(fils[0].path);
            res.fils.Clear();
            res.dirCount = dirCount;
            res.filCount = filCount;
            for (var i = 0; i < fils.Count; i++) res.fils.Add((TOCItemFil)fils[i].Clone());

            return res;
        }

        #endregion

        #region IComparer<TOCItemFil> Members

        public int Compare(TOCItemFil x, TOCItemFil y)
        {
            return x.pos > y.pos ? 1 : x.pos < y.pos ? -1 : 0;
        }

        #endregion
    }

    public class TOCItemFil : ICloneable
    {
        public readonly int dirIdx;
        public readonly string gamePath;
        public readonly bool isDir;
        public readonly string name;
        public readonly string path;
        public readonly int TOCIdx;
        public int len;
        public int nextIdx;
        public TreeNode node;
        public int pos;
        public int prevIdx;

        public TOCItemFil(int TOCIdx, int dirIdx, int pos, int len, bool isDir, string name, string gamePath,
            string path)
        {
            this.TOCIdx = TOCIdx;
            this.dirIdx = dirIdx;
            this.pos = pos;
            this.len = len;
            this.isDir = isDir;
            this.name = name;
            this.gamePath = gamePath;
            this.path = path;
        }

        #region ICloneable Members

        public object Clone()
        {
            return new TOCItemFil(TOCIdx, dirIdx, pos, len, isDir, name, gamePath, path);
        }

        #endregion
    }

    #endregion
}