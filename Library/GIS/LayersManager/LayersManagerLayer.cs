using System;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.Controls;

namespace GIS.LayersManager
{
    /// <summary>
    /// ͼ�����
    ///</summary>
    [Guid("65e1275d-73f9-4156-a598-405e562eb953")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("GIS.LayersManager.LayersManagerLayer")]
    public class LayersManagerLayer
    {
        private IToolbarMenu2 m_toolbarMenu = null;
        private bool m_beginGroupFlag = false;

        public LayersManagerLayer()
        {
        }

        /// <summary>
        /// ToolbarMenuʵ��������ӹ���/���ť
        /// </summary>
        public void SetHook(object hook)
        {
            m_toolbarMenu = new ToolbarMenuClass();
            m_toolbarMenu.SetHook(hook);
            //��ӹ���/����
            //m_toolbarMenu.AddItem(new RemoveLayer(), -1, 0, false, esriCommandStyles.esriCommandStyleTextOnly);
            //m_toolbarMenu.AddItem(new LayerSelectable(), 1, 0, true, esriCommandStyles.esriCommandStyleTextOnly);
            //m_toolbarMenu.AddItem(new LayerSelectable(), 2, 1, false, esriCommandStyles.esriCommandStyleTextOnly);
            m_toolbarMenu.AddItem(new LayerVisible(), 1, 0, false, esriCommandStyles.esriCommandStyleTextOnly);
            m_toolbarMenu.AddItem(new LayerVisible(), 2, 1, false, esriCommandStyles.esriCommandStyleTextOnly);
            m_toolbarMenu.AddItem(new ZoomToLayer(), -1, 2, true, esriCommandStyles.esriCommandStyleTextOnly);
        }

        /// <summary>
        /// ��ָ��λ�õ����˵�
        /// </summary>
        /// <param name="X">X����</param>
        /// <param name="Y">Y����</param>
        /// <param name="hWndParent">�����ھ��</param>
        public void PopupMenu(int X, int Y, int hWndParent)
        {
            if (m_toolbarMenu != null)
                m_toolbarMenu.PopupMenu(X, Y, hWndParent);
        }

        /// <summary>
        /// ����ʱ������Ҫ���»�ȡToolbarMenu
        /// </summary>
        public IToolbarMenu2 ContextMenu
        {
            get
            {
                return m_toolbarMenu;
            }
        }

        #region Helper methods to add items to the context menu
        /// <summary>
        /// ��ӷָ���
        /// </summary>
        private void BeginGroup()
        {
            m_beginGroupFlag = true;
        }

        /// <summary>
        /// ����UID��Ӷ�Ӧ����
        /// </summary>
        private void AddItem(UID itemUID)
        {
            m_toolbarMenu.AddItem(itemUID.Value, itemUID.SubType, -1, m_beginGroupFlag, esriCommandStyles.esriCommandStyleIconAndText);
            m_beginGroupFlag = false; //����ָ��־
        }

        /// <summary>
        /// ����ʶ���ַ�������������������ӵ���������
        /// </summary>
        private void AddItem(string itemID, int subtype)
        {
            UID itemUID = new UIDClass();
            try
            {
                itemUID.Value = itemID;
            }
            catch
            {
                //����ʱ������ӿ�GUID
                itemUID.Value = Guid.Empty.ToString("B");
            }

            if (subtype > 0)
                itemUID.SubType = subtype;
            AddItem(itemUID);

        }

        /// <summary>
        /// ����GUID����������������ӵ���������
        /// </summary>
        private void AddItem(Guid itemGuid, int subtype)
        {
            AddItem(itemGuid.ToString("B"), subtype);
        }

        /// <summary>
        /// �������ͺ���������������ӵ���������
        /// </summary>
        private void AddItem(Type itemType, int subtype)
        {
            if (itemType != null)
                AddItem(itemType.GUID, subtype);
        }

        #endregion

    }
}
