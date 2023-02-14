namespace GCBM;

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using GCBM.Properties;

internal static class Program
{
    public static readonly IniFile ConfigFile = new (Path.Combine(".", "config.ini"));
    public static readonly CultureInfo[] CultureInfos = new[]
    {
        new CultureInfo("en-US"), // English [US]
        new CultureInfo("pt-BR"), // Portuguese
        new CultureInfo("ko"), // Korean
        new CultureInfo("es"), // Spanish [Spain]
        new CultureInfo("es-MX"), // Spanish [Mexico]
        new CultureInfo("zh"), // Chinese Simplified
        new CultureInfo("de"), // German [Germany]
        new CultureInfo("hu"), // Hungarian
        new CultureInfo("id"), // Indonesian
        new CultureInfo("it"), // Italian
        new CultureInfo("ja"), // Japanese
        new CultureInfo("uk"), // Ukrainian
        new CultureInfo("zh-CN"), // Chinese (Simplified)
        new CultureInfo("zh-TW"), // Chinese (Traditional)
    };

    private static readonly string DateUpdated = "10/07/2022";
    private static readonly string DefaultTempDir = Path.Combine(".", "temp");
    private static readonly string DefaultCoversCacheDir = Path.Combine(".", "covers", "cache");

    public static Form SplashScreen { get; private set; }

    //public enum Language
    //{
    //    ICHINESE,
    //    IENGLISH,
    //    IGERMAN,
    //    IHUNGARIAN,
    //    IINDONESIAN,
    //    IITALIAN,
    //    IJAPANESE,
    //    IKOREAN,
    //    IPORTUGUESE,
    //    IUKRAINIAN
    //};

    public static void DefaultConfigSave()
    {
        // GCBM
        ConfigFile.IniWriteString("GCBM", "ProgUpdated", DateUpdated);
        //ConfigFile.IniWriteString("GCBM", "ProgVersion", VERSION());
        ConfigFile.IniWriteString("GCBM", "ConfigUpdated", DateTime.Now.ToString("dd/MM/yyyy"));
        ConfigFile.IniWriteString("GCBM", "Language", Resources.GCBM_Language);
        ConfigFile.IniWriteString("GCBM", "TranslatedBy", Resources.GCBM_TranslatedBy);

        // General
        ConfigFile.IniWriteBool("GENERAL", "DiscClean", true);
        ConfigFile.IniWriteBool("GENERAL", "DiscDelete", false);
        ConfigFile.IniWriteBool("GENERAL", "ExtractZip", false);
        ConfigFile.IniWriteBool("GENERAL", "Extract7z", false);
        ConfigFile.IniWriteBool("GENERAL", "ExtractRar", false);
        ConfigFile.IniWriteBool("GENERAL", "ExtractBZip2", false);
        ConfigFile.IniWriteBool("GENERAL", "ExtractSplitFile", false);
        ConfigFile.IniWriteBool("GENERAL", "ExtractNwb", false);
        ConfigFile.IniWriteInt("GENERAL", "FileSize", 0);
        ConfigFile.IniWriteString("GENERAL", "TemporaryFolder", DefaultTempDir);

        // Several
        ConfigFile.IniWriteInt("SEVERAL", "AppointmentStyle", 0);
        ConfigFile.IniWriteBool("SEVERAL", "CheckMD5", false);
        ConfigFile.IniWriteBool("SEVERAL", "CheckSHA1", false);
        ConfigFile.IniWriteBool("SEVERAL", "CheckNotify", true);
        ConfigFile.IniWriteBool("SEVERAL", "NetVerify", true);
        ConfigFile.IniWriteBool("SEVERAL", "RecursiveMode", true);
        ConfigFile.IniWriteBool("SEVERAL", "TemporaryBuffer", false);
        ConfigFile.IniWriteBool("SEVERAL", "WindowMaximized", false);
        ConfigFile.IniWriteBool("SEVERAL", "DisableSplash", false);
        ConfigFile.IniWriteBool("SEVERAL", "Screensaver", false);
        ConfigFile.IniWriteBool("SEVERAL", "LoadDatabase", true);
        ConfigFile.IniWriteBool("SEVERAL", "MultipleInstances", false);
        ConfigFile.IniWriteBool("SEVERAL", "LaunchedOnce", true);

        // TransferSystem
        ConfigFile.IniWriteBool("TRANSFERSYSTEM", "FST", false);
        ConfigFile.IniWriteBool("TRANSFERSYSTEM", "ScrubFlushSD", false);
        ConfigFile.IniWriteInt("TRANSFERSYSTEM", "ScrubAlign", 0);
        ConfigFile.IniWriteString("TRANSFERSYSTEM", "ScrubFormat", "DiscEx");
        ConfigFile.IniWriteInt("TRANSFERSYSTEM", "ScrubFormatIndex", 1);
        ConfigFile.IniWriteBool("TRANSFERSYSTEM", "Wipe", false);
        ConfigFile.IniWriteBool("TRANSFERSYSTEM", "XCopy", true);

        // Covers
        ConfigFile.IniWriteBool("COVERS", "DeleteCovers", false);
        ConfigFile.IniWriteBool("COVERS", "CoverRecursiveSearch", false);
        ConfigFile.IniWriteBool("COVERS", "TransferCovers", false);
        ConfigFile.IniWriteBool("COVERS", "WiiFlowCoverUSBLoader", false);
        ConfigFile.IniWriteBool("COVERS", "GXCoverUSBLoader", true);
        ConfigFile.IniWriteString("COVERS", "CoverDirectoryCache", DefaultCoversCacheDir);
        ConfigFile.IniWriteString("COVERS", "WiiFlowCoverDirectoryDisc", string.Empty);
        ConfigFile.IniWriteString("COVERS", "WiiFlowCoverDirectory2D", string.Empty);
        ConfigFile.IniWriteString("COVERS", "WiiFlowCoverDirectory3D", string.Empty);
        ConfigFile.IniWriteString("COVERS", "WiiFlowCoverDirectoryFull", string.Empty);
        ConfigFile.IniWriteString("COVERS", "GXCoverDirectoryDisc", string.Empty);
        ConfigFile.IniWriteString("COVERS", "GXCoverDirectory2D", string.Empty);
        ConfigFile.IniWriteString("COVERS", "GXCoverDirectory3D", string.Empty);
        ConfigFile.IniWriteString("COVERS", "GXCoverDirectoryFull", string.Empty);

        // Titles
        ConfigFile.IniWriteBool("TITLES", "GameCustomTitles", false);
        ConfigFile.IniWriteBool("TITLES", "GameTdbTitles", false);
        ConfigFile.IniWriteBool("TITLES", "GameInternalName", true);
        ConfigFile.IniWriteBool("TITLES", "GameXmlName", false);
        ConfigFile.IniWriteString("TITLES", "LocationTitles", Path.Combine("%APP%", "titles.txt"));
        ConfigFile.IniWriteString("TITLES", "LocationCustomTitles", Path.Combine("%APP%", "custom-titles.txt"));
        ConfigFile.IniWriteInt("TITLES", "TitleLanguage", 0);

        // Dolphin Emulator
        ConfigFile.IniWriteString("DOLPHIN", "DolphinFolder", string.Empty);
        ConfigFile.IniWriteBool("DOLPHIN", "DolphinDX11", true);
        ConfigFile.IniWriteBool("DOLPHIN", "DolphinDX12", false);
        ConfigFile.IniWriteBool("DOLPHIN", "DolphinVKGL", false);
        ConfigFile.IniWriteBool("DOLPHIN", "DolphinLLE", false);
        ConfigFile.IniWriteBool("DOLPHIN", "DolphinHLE", true);

        // Updates
        ConfigFile.IniWriteBool("UPDATES", "UpdateVerifyStart", false);
        ConfigFile.IniWriteBool("UPDATES", "UpdateBetaChannel", false);
        ConfigFile.IniWriteBool("UPDATES", "UpdateFileLog", false);
        ConfigFile.IniWriteBool("UPDATES", "UpdateServerProxy", false);
        ConfigFile.IniWriteString("UPDATES", "ServerProxy", string.Empty);
        ConfigFile.IniWriteString("UPDATES", "UserProxy", string.Empty);
        ConfigFile.IniWriteString("UPDATES", "PassProxy", string.Empty);
        ConfigFile.IniWriteInt("UPDATES", "VerificationInterval", 0);

        // Manager Log
        ConfigFile.IniWriteInt("MANAGERLOG", "LogLevel", 0);
        ConfigFile.IniWriteBool("MANAGERLOG", "LogSystemConsole", false);
        ConfigFile.IniWriteBool("MANAGERLOG", "LogDebugConsole", false);
        ConfigFile.IniWriteBool("MANAGERLOG", "LogWindow", false);
        ConfigFile.IniWriteBool("MANAGERLOG", "LogFile", true);

        // Language
        if (IsTranslated(DetectOSLanguage()))
        {
            ConfigFile.IniWriteString("LANGUAGE", "ConfigLanguage", DetectOSLanguage());
        }
        else
        {
            LanguagePrompt();
        }
    }

    /// <summary>
    ///     Check cultureInfos and see if the language is supported.
    /// </summary>
    public static bool IsTranslated(string language)
    {
        foreach (CultureInfo cultureInfo in CultureInfos)
        {
            if (cultureInfo.Name == language)
            {
                return true;
            }
        }

        return false;
    }

    public static void LanguagePrompt()
    {
        frmLanguagePrompt frmPrompt = new ();
        frmPrompt.ShowDialog();
    }
    #region Detect OS Language

    /// <summary>
    ///     Automatic detection of operating system default language.
    /// </summary>
    public static string DetectOSLanguage()
    {
        var sysLocale = Thread.CurrentThread.CurrentCulture;
        var sysLang = sysLocale.TwoLetterISOLanguageName;
        return sysLang;
    }

    /// <summary>
    ///     Check the config file to see which language is specified or default to the OS language if supported, else default to english.
    /// </summary>
    public static void AdjustLanguage(Thread t)
    {
        try
        {
            if (ConfigFile.IniReadString("LANGUAGE", "ConfigLanguage", string.Empty) == string.Empty)
            {
                var sysLocale = Thread.CurrentThread.CurrentCulture;
                var sysLang = CultureInfo.CurrentUICulture.Name;
                if (IsTranslated(sysLang))
                {
                    t.CurrentUICulture = new CultureInfo(sysLang);
                }
                else
                {
                    t.CurrentUICulture = new CultureInfo("en");
                }
            }
            else
            {
                t.CurrentUICulture = new CultureInfo(ConfigFile.IniReadString("LANGUAGE", "ConfigLanguage", string.Empty));
            }
        }
        catch (Exception exception)
        {
            if (exception.GetBaseException() is CultureNotFoundException)
            {
                File.Delete("config.ini");
                DefaultConfigSave();
            }
        }
    }

    //public static void AdjustLanguage(Thread t)
    //{
    //    while (true)
    //    {
    //        var ConfigFile = new IniFile("config.ini");
    //        //Get current system Locale -- Thread.CurrentThread.CurrentUICulture.Name
    //        if (ConfigFile.IniReadBool("SEVERAL", "LaunchedOnce"))
    //        {
    //            switch (ConfigFile.IniReadString("LANGUAGE", "ConfigLanguage"))
    //            {
    //                case "pt-BR":
    //                    t.CurrentUICulture = new CultureInfo("pt-BR");
    //                    break;
    //                case "en-US":
    //                    t.CurrentUICulture = new CultureInfo("en-US");
    //                    break;
    //                case "es":
    //                    t.CurrentUICulture = new CultureInfo("es");
    //                    break;
    //                case "ko":
    //                    t.CurrentUICulture = new CultureInfo("ko");
    //                    break;
    //                case "fr":
    //                    t.CurrentUICulture = new CultureInfo("fr");
    //                    break;
    //                case "de":
    //                    t.CurrentUICulture = new CultureInfo("de");
    //                    break;
    //                case "ja":
    //                    t.CurrentUICulture = new CultureInfo("ja");
    //                    break;
    //                case "zh":
    //                    t.CurrentUICulture = new CultureInfo("zh");
    //                    break;
    //                default:
    //                    t.CurrentUICulture = new CultureInfo("en-US");
    //                    break;
    //            }
    //        }
    //        else
    //        {
    //            DetectOSLanguage();
    //        }

    //        break;
    //    }
    //}

    #endregion
    private static void Start()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        SplashScreen = new frmSplashScreen();

        var splashThread = new Thread(() => Application.Run(SplashScreen));
        splashThread.CurrentUICulture = CultureInfo.CurrentCulture;
        splashThread.SetApartmentState(ApartmentState.STA);
        splashThread.Start();
    }

    /// <summary>
    ///     Point of entry for the application.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        var nomeProcesso = Process.GetCurrentProcess().ProcessName;
        var processos = Process.GetProcessesByName(nomeProcesso);

        // TODO: Validate if this is still a problem
        // We changed the variable that stores the selected language from an int to the culture string, this causes a crash when we try
        // to call CurrentUICulture = new CultureInfo(0)... etc. So we have to make sure that either they have a working/upated INI file.
        // Chosen to do this by presenting the user a new LanguagePrompt form, which will also appear upon first launch, If no supported language has been found.
        if (File.Exists("config.ini") && ConfigFile.IniReadBool("SEVERAL", "MultipleInstances") == false && processos.Length > 1)
        {
            _ = MessageBox.Show(Resources.CannotOpenTwoInstances, "GameCube Backup Manager", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            Application.Exit();
        }

        Start();
    }
}
