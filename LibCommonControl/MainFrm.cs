﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using LibSocket;
using LibCommon;
using LibConfig;

namespace LibCommonControl
{
    public partial class MainFrm : XtraForm
    {
        //客户端
        public static ClientSocket _clientSocket = null;

        public MainFrm()
        {
            InitializeComponent();
        }

        public void doInitilization()
        {
            // Initialize configuration manager.
            ConfigManager cfgMgr = ConfigManager.Instance;
            string msg = cfgMgr.init(Application.StartupPath);

            if (msg != string.Empty)
            {
                MessageBox.Show(msg);
                Application.Exit();
            }

            //初始化客户端Socket
            InitClientSocket();
        }

        public void InitClientSocket()
        {
            string serverIp = ConfigManager.Instance.getValueByKey(ConfigConst.CONFIG_SERVER_IP);
            int port = int.Parse(ConfigManager.Instance.getValueByKey(ConfigConst.CONFIG_PORT));

            //初始化客户端Socket，连接服务器
            string errorMsg = SocketHelper.InitClientSocket(serverIp, port, out MainFrm._clientSocket);
            if (errorMsg != "")
            {
                Alert.alert(Const.CONNECT_SOCKET_ERROR, Const.NOTES, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Log.Error(errorMsg);
            }
            else
            {
                //连接服务器成功
                Log.Info(Const.LOG_MSG_CONNECT_SUCCEED);
            }
        }

        /// <summary>
        /// 获取客户端Socket实例
        /// </summary>
        /// <returns>不会返回NULL</returns>
        public ClientSocket GetClientSocketInstance()
        {
            if (_clientSocket == null)
            {
                InitClientSocket();
            }
            return _clientSocket;
        }

        /// <summary>
        /// 发送消息至服务器
        /// </summary>
        /// <param name="msg"></param>
        public void SendMsg2Server(SocketMessage msg)
        {
            ClientSocket cs = GetClientSocketInstance();
            if (cs != null)
            {
                string errMsg = cs.SendSocketMsg2Server(msg);
                Log.Debug("Send message " + msg);
                if (errMsg != "")
                {
                    Log.Error(Const.SEND_MSG_FAILED + Const.CONNECT_ARROW + msg.ToString());
                }
            }
            else
            {
                Log.Info(Const.CLIENT_SOCKET_IS_NULL + Const.CONNECT_ARROW);
            }
        }
    }
}
