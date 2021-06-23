using System.Collections.Generic;

namespace TutumAPI.Models.EnumModels
{
    public enum MediaExtension
    {
        //video
        v3gp = 0,

        vmp4 = 1,
        vmkv = 2,
        vwebm = 3
    }

    public class MediaExtensionDictionaries
    {
        public static Dictionary<MediaExtension, string> MediaExtensionToString = new Dictionary<MediaExtension, string>()
        {
            { MediaExtension.v3gp, ".3gp" },
            { MediaExtension.vmp4, ".mp4" },
            { MediaExtension.vmkv, ".mkv" },
            { MediaExtension.vwebm, ".webm" }
        };

        public static Dictionary<string, MediaExtension> StringToMediaExtension = new Dictionary<string, MediaExtension>()
        {
            { ".3gp", MediaExtension.v3gp },
            { ".mp4", MediaExtension.vmp4 },
            { ".mkv", MediaExtension.vmkv },
            { ".webm", MediaExtension.vwebm }
        };
    }
}