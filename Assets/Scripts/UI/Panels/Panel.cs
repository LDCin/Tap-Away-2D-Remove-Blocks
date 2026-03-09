using UnityEngine;

namespace UI.Panels
{
    public class Panel : MonoBehaviour
    {
        [SerializeField] private bool destroyOnClose = false;
        private string _panelName;
        public virtual void Open()
        {
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
        }
        public virtual void Close()
        {
            gameObject.SetActive(false);
            if (destroyOnClose)
            {
                Destroy(gameObject);
            }
        }

        public string GetPanelName()
        {
            return _panelName;
        }

        public void SetPanelName(string newName)
        {
            _panelName = newName;
        }
    }
}