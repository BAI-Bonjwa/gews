﻿// ******************************************************************
// 概  述：登录界面
// 作  者：杨小颖
// 创建日期：2014/03/07
// 版本号：V1.0
// 版本信息:
// V1.0 新建
// V1.1 添加用户登录
// ******************************************************************
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LibBusiness;
using LibEntity;
using LibCommon;
using LibCommonForm;
using LibConfig;
using System.IO;

namespace LibLoginForm
{
    public partial class LoginForm : Form
    {
        //获取曾经登录用户的信息
        private UserLogin[] entsLogined = null;

        //获取所有的用户信息
        private UserLogin[] ents = null;

        Form _showForm = null;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="form2ShowAfterLogin"></param>
        public LoginForm(Form form2ShowAfterLogin)
        {
            InitializeComponent();
            _showForm = form2ShowAfterLogin;
            //SkinReader sr = new SkinReader();
            //string sn = sr.ReadCurSkin();
            //skinEngine1.SkinFile = Application.StartupPath + "\\skin\\" + sn;
            //skinEngine1.SkinAllForm = true;
        }

        /// <summary>
        /// 窗体登录事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            //初始化调用不规则窗体生成代码
            //此为生成不规则窗体和控件的类
            BitmapRegion BitmapRegion = new BitmapRegion();
            string path = Application.StartupPath + LibCommon.Const.LOGIN_BACKGROUND_BMP_PATH;
            BitmapRegion.CreateControlRegion(this, new Bitmap(path));

            //获取所有用户信息
            ents = LoginFormBLL.GetUserLoginInformations();

            //添加已记录的登录用户
            entsLogined = LoginFormBLL.GetUserLoginedInformation();
            foreach (UserLogin ent in entsLogined)
            {
                _cbxUserName.Items.Add(ent.LoginName);
            }

            //默认显示第一个用户，应改成默认选择最后一个登录用户。
            //可采用表中记录登录时间来实现。为减少修改量，未采用修改数据表结构。
            //采用读取配置文件的方法。记录信息在DefaultUser           
            try
            {
                StreamReader sr = new StreamReader(Application.StartupPath + "\\DefaultUser");
                string str = sr.ReadLine();
                sr.Close();

                //赋值同时，触发_cbxUserName_SelectedIndexChanged事件，符合记住密码的用户名，自动赋值
                _cbxUserName.SelectedItem = (object)str;
            }
            catch (System.Exception ex)
            {
                Alert.alert(ex.Message);
            }
        }

        /// <summary>
        /// 确定登录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonOk_Click(object sender, EventArgs e)
        {

            bool status = false;

            string userName = this._cbxUserName.Text;
            string password = this._txtPassword.Text;

            UserLogin[] ents = LoginFormBLL.GetUserLoginInformations();
            //数据库中无用户名及密码信息
            if (ents == null)
            {
                LibCommon.Const.FIRST_TIME_LOGIN = true;
                status = false;
                Alert.alert(Const.ADD_USER_INFO, Const.LOGIN_FAILED_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);

                UserLogin ent = new UserLogin();
                ent.LoginName = _cbxUserName.Text.ToString();
                ent.PassWord = _txtPassword.Text;

                //显示添加用户信息界面，确定后将新创建的用户名及密码自动填写到相应控件上
                UserLoginInformationInput ulii = new UserLoginInformationInput(ent, true);
                ulii.ShowDialog();
                if (LibCommon.Const.FIRST_LOGIN_NAME != "")
                {
                    _cbxUserName.Text = LibCommon.Const.FIRST_LOGIN_NAME;
                    _txtPassword.Text = LibCommon.Const.FIRST_LOGIN_PASSWORD;
                    LibCommon.Const.FIRST_TIME_LOGIN = false;
                    //buttonOk_Click(sender, e);//添加成功用户后，自动登陆系统
                }
            }
            else
            {
                //验证帐号密码是否正确
                if (LoginSuccess(userName, password))
                {
                    status = true;
                }
                else
                {
                    Alert.alert(Const.USER_NAME_OR_PWD_ERROR_MSG, Const.LOGIN_FAILED_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            if (status)
            {
                this.Hide();
                _showForm.WindowState = FormWindowState.Maximized;
                _showForm.ShowDialog();
            }

        }

        /// <summary>
        /// 登录成功
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="password">密码</param>
        /// <returns></returns>
        private bool LoginSuccess(string userName, string password)
        {
            //定义记录登录成功与否的值
            bool isLogin = false;
            if (LoginFormBLL.LoginSuccess(userName, password) != null)
            {
                //set CurrentUser
                CurrentUser.CurLoginUserInfo = LoginFormBLL.LoginSuccess(userName, password);

                //记录最后一次登录用户
                StreamWriter sw = new StreamWriter(Application.StartupPath + "\\DefaultUser", false);
                sw.WriteLine(userName);
                sw.Close();

                //记住密码,登录成功，修改用户“尚未登录”为False；根据是否记住密码设定相应的值
                LoginFormBLL.RememberPassword(userName, _chkSavePassword.Checked);
                ConfigManager.Instance.add(ConfigConst.CONFIG_CURRENT_USER, userName);
                isLogin = true;
            }
            return isLogin;
        }

        /// <summary>
        /// 关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 用户名选择变化时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _cbxUserName_SelectedIndexChanged(object sender, EventArgs e)
        {
            string strSelLoginName = _cbxUserName.Text;
            foreach (UserLogin ent in ents)
            {
                if (strSelLoginName == ent.LoginName)
                {
                    //验证是否记住密码,并赋予相应的值
                    _chkSavePassword.Checked = ent.SavePassWord ? true : false;
                    _txtPassword.Text = ent.SavePassWord ? ent.PassWord : "";
                    //设置焦点
                    if (ent.SavePassWord)
                    {
                        buttonOk.Focus();
                    }
                    else
                    {
                        _txtPassword.Focus();
                    }
                    break;
                }
            }
        }
    }
}
