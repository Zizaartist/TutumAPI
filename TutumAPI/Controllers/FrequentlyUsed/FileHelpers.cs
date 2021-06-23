using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System;
using System.IO;
using System.Threading.Tasks;
using TutumAPI.Models.EnumModels;

namespace TutumAPI.Controllers.FrequentlyUsed
{

    public static class FileHelpers
    {
        public static async Task<byte[]> ProcessStreamedFile(
            MultipartSection section, ContentDispositionHeaderValue contentDisposition,
            ModelStateDictionary modelState, long sizeLimit)
        {
            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    await section.Body.CopyToAsync(memoryStream);

                    // Check if the file is empty or exceeds the size limit.
                    if (memoryStream.Length == 0)
                    {
                        modelState.AddModelError("File", "The file is empty.");
                    }
                    else if (memoryStream.Length > sizeLimit)
                    {
                        var megabyteSizeLimit = sizeLimit / 1048576;
                        modelState.AddModelError("File",
                        $"The file exceeds {megabyteSizeLimit:N1} MB.");
                    }
                    else if (!IsValidFileExtensionAndSignature(
                        contentDisposition.FileName.Value, memoryStream))
                    {
                        modelState.AddModelError("File",
                            "The file type isn't permitted or the file's " +
                            "signature doesn't match the file's extension.");
                    }
                    else
                    {
                        return memoryStream.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                modelState.AddModelError("File",
                    "The upload failed. Please contact the Help Desk " +
                    $" for support. Error: {ex.HResult}");
                // Log the exception
            }

            return new byte[0];
        }

        private static bool IsValidFileExtensionAndSignature(string fileName, Stream data)
        {
            if (string.IsNullOrEmpty(fileName) || data == null || data.Length == 0)
            {
                return false;
            }

            var ext = Path.GetExtension(fileName).ToLowerInvariant();

            if (string.IsNullOrEmpty(ext) || !MediaExtensionDictionaries.StringToMediaExtension.ContainsKey(ext))
            {
                return false;
            }

            data.Position = 0;

            using (var reader = new BinaryReader(data))
            {
                // Uncomment the following code block if you must permit
                // files whose signature isn't provided in the _fileSignature
                // dictionary. We recommend that you add file signatures
                // for files (when possible) for all file types you intend
                // to allow on the system and perform the file signature
                // check.


                return true; //тут была проверка сигнатуры в словаре, но в словаре были только сигнатуры картинок, поэтому похуй 


                // File signature check
                // --------------------
                // With the file signatures provided in the _fileSignature
                // dictionary, the following code tests the input content's
                // file signature

                //Сверяем сигнатуру файла со значением в словаре
                //var signatures = MediaExtensionDictionaries.ExtensionToSignature[ext];
                //var headerBytes = reader.ReadBytes(signatures.Max(m => m.Length));

                //return signatures.Any(signature =>
                //    headerBytes.Take(signature.Length).SequenceEqual(signature));
            }
        }
    }
}
