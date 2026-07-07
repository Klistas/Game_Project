using UnityEngine;

namespace ViralPartyPrototypeLab.Prototype
{
    public sealed class PrototypeSceneMetadata : MonoBehaviour
    {
        [SerializeField] private string prototypeId;
        [SerializeField] private string prototypeTitle;
        [SerializeField] private string status;

        public string PrototypeId => prototypeId;
        public string PrototypeTitle => prototypeTitle;
        public string Status => status;

        public void Configure(string id, string title, string currentStatus)
        {
            prototypeId = id;
            prototypeTitle = title;
            status = currentStatus;
        }
    }
}
