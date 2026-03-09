using System.Collections.Generic;
using UI.Panels;
using UnityEngine;

namespace Scripts
{
    public class PanelManager : Singleton<PanelManager>
    {
        private Dictionary<string, Panel> panelList = new Dictionary<string, Panel>();

        public override void Awake()
        {
            base.Awake();
            var existPanels = GetComponentsInChildren<Panel>(true);
            foreach (var panel in existPanels)
            {
                if (panel == null)
                {
                    continue;
                }

                panelList[panel.name] = panel;
                panel.SetPanelName(panel.name);
            }
        }

        private bool TryGetLivePanel(string panelName, out Panel panel)
        {
            if (panelList.TryGetValue(panelName, out panel) && panel != null)
            {
                return true;
            }

            panelList.Remove(panelName);
            panel = null;
            return false;
        }

        public Panel GetPanel(string panelName)
        {
            if (TryGetLivePanel(panelName, out Panel cachedPanel))
            {
                return cachedPanel;
            }

            Panel panelPrefab = Resources.Load<Panel>(GameConfig.PANEL_PATH + panelName);
            if (panelPrefab == null)
            {
                return null;
            }

            Panel newPanel = Instantiate(panelPrefab, transform);
            newPanel.SetPanelName(panelName);
            newPanel.transform.SetAsLastSibling();
            newPanel.gameObject.SetActive(false);

            panelList[panelName] = newPanel;
            return newPanel;
        }

        public void OpenPanel(string panelName)
        {
            Panel panel = GetPanel(panelName);
            if (panel == null)
            {
                return;
            }

            panel.Open();
        }

        public void ClosePanel(string panelName)
        {
            if (!TryGetLivePanel(panelName, out Panel panel))
            {
                return;
            }

            panel.Close();
        }

        public void CloseAllPanel()
        {
            var keys = new List<string>(panelList.Keys);
            for (int i = 0; i < keys.Count; i++)
            {
                ClosePanel(keys[i]);
            }
        }
    }
}