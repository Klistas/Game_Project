using System.IO;
using UnityEngine;

namespace ViralPartyPrototypeLab.Debugging
{
    public sealed class SimpleScreenshotCapture : MonoBehaviour
    {
        [SerializeField] private string folderName = "PrototypeScreenshots";

        public string Capture(string filePrefix)
        {
            string folder = Path.Combine(Application.persistentDataPath, folderName);
            Directory.CreateDirectory(folder);

            string safePrefix = string.IsNullOrWhiteSpace(filePrefix) ? "prototype" : filePrefix;
            string fileName = safePrefix + "_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
            string fullPath = Path.Combine(folder, fileName);
            ScreenCapture.CaptureScreenshot(fullPath);
            UnityEngine.Debug.Log("Screenshot capture requested: " + fullPath);
            return fullPath;
        }
    }
}
