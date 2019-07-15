﻿using BusinessRefinery.Barcode;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio;
using NAudio.Wave;
using System.Runtime.InteropServices;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace ProfilerBySSFProject
{
    public partial class MainFRM : Form
    {
        // ------------------------------------     متغیرها
        private bool DeviceStat = false;

        private bool runtime = true;
        private bool mute = false;
        private int idle = 60;
        private bool founded = false;
        private bool onProcess = false;
        private IWavePlayer waveOutDevice;
        private AudioFileReader audioFileReader;
        private string cityBarcode01, cityBarcode02, cityBarcode03, cityBarcode04, cityBarcode05;
        private bool pre = false;
        private bool current = false;
        private static Image img;
        private string fn;
        private Thread t;
        private const string barcodeReaderAppliaction = "WindowsFormsApplication1";

        private string[] cities = new string[] {
            "همدان",
            "تهران",
            "مشهد",
            "اردبیل",
            "یزد",
            "کرمان",
            "اصفهان",
            "اراک",
            "اهواز",
            "قزوین",
            "گرگان"
        };

        private class Box
        {
            public double lenght;
            public double width;
            public double height;
            public double weight;

            public Box(double lenght, double width, double height, double weight)
            {
                this.lenght = lenght;
                this.width = width;
                this.height = height;
                this.weight = weight;
            }
        }

        private Box box;

        private ToolTip toolTip1 = new ToolTip();

        // توابع و متدها
        public MainFRM()
        {
            InitializeComponent();
        }

        private void MainFRM_Load(object sender, EventArgs e)
        {
         
            // camera -----

            m_bInitSDK = CHCNetSDK.NET_DVR_Init();
            if (m_bInitSDK == false)
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show("NET_DVR_Init error!");
                return;
            }
            else
            {
                //保存SDK日志 To save the SDK log
                CHCNetSDK.NET_DVR_SetLogToFile(3, app.WorkingPath, true);

                //comboBoxView.SelectedIndex = 0;

                for (int i = 0; i < 64; i++)
                {
                    iIPDevID[i] = -1;
                    iChannelNum[i] = -1;
                }
            }


            this.Cursor = Cursors.Default;
        }


      

        private Image TempShot_top;
       
       

        private void MainFRM_KeyUp(object sender, KeyEventArgs e)
        {
           
        }


        private void openSyncDialog()
        {
           
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            openSyncDialog();
        }

        private void pictureBox6_Click(object sender, EventArgs e)
        {
        }

      
        private void MainFRM_FormClosing(object sender,
            FormClosingEventArgs e) // هنگام خروج از برنامه
        {
           
        }


        //  CAMERAS AND ALL OF THE DATAS ..........................................................................

        private bool m_bInitSDK = false;
        private bool m_bRecord = false;
        private uint iLastErr = 0;
        private Int32 Camera_id_top = -1;
        private Int32 CameraHandel_top = -1;

        private Int32 i = 0;
        private Int32 m_lTree = 0;
        private string str;
        private long iSelIndex = 0;
        private uint dwAChanTotalNum = 0;
        private Int32 m_lPort = -1;
        private IntPtr m_ptrRealHandle;
        private int[] iIPDevID = new int[96];
        private int[] iChannelNum = new int[96];

        public CHCNetSDK.REALDATACALLBACK RealData = null;
        public CHCNetSDK.NET_DVR_DEVICEINFO_V30 DeviceInfo_top;

        public CHCNetSDK.NET_DVR_IPPARACFG_V40 m_struIpParaCfgV40;
        public CHCNetSDK.NET_DVR_STREAM_MODE m_struStreamMode;
        public CHCNetSDK.NET_DVR_IPCHANINFO m_struChanInfo;
        public CHCNetSDK.NET_DVR_IPCHANINFO_V40 m_struChanInfoV40;
        private PlayCtrl.DECCBFUN m_fDisplayFun = null;
        private string str1;
        private string str2;

        public delegate void MyDebugInfo(string str);

        public void InfoIPChannel()
        {
            uint dwSize = (uint)Marshal.SizeOf(m_struIpParaCfgV40);

            IntPtr ptrIpParaCfgV40 = Marshal.AllocHGlobal((Int32)dwSize);
            Marshal.StructureToPtr(m_struIpParaCfgV40, ptrIpParaCfgV40, false);

            uint dwReturn = 0;
            int iGroupNo = 0;
            if (!CHCNetSDK.NET_DVR_GetDVRConfig(Camera_id_top, CHCNetSDK.NET_DVR_GET_IPPARACFG_V40, iGroupNo, ptrIpParaCfgV40, dwSize, ref dwReturn))
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                str = "NET_DVR_GET_IPPARACFG_V40 failed, error code= " + iLastErr;
                //获取IP资源配置信息失败，输出错误号 Failed to get configuration of IP channels and output the error code
                DebugInfo(str);
            }
            else
            {
                DebugInfo("NET_DVR_GET_IPPARACFG_V40 succ!");

                m_struIpParaCfgV40 = (CHCNetSDK.NET_DVR_IPPARACFG_V40)Marshal.PtrToStructure(ptrIpParaCfgV40, typeof(CHCNetSDK.NET_DVR_IPPARACFG_V40));

                for (i = 0; i < dwAChanTotalNum; i++)
                {
                    ListAnalogChannel(i + 1, m_struIpParaCfgV40.byAnalogChanEnable[i]);
                    iChannelNum[i] = i + (int)DeviceInfo_top.byStartChan;
                }

                byte byStreamType;
                for (i = 0; i < m_struIpParaCfgV40.dwDChanNum; i++)
                {
                    iChannelNum[i + dwAChanTotalNum] = i + (int)m_struIpParaCfgV40.dwStartDChan;
                    byStreamType = m_struIpParaCfgV40.struStreamMode[i].byGetStreamType;

                    dwSize = (uint)Marshal.SizeOf(m_struIpParaCfgV40.struStreamMode[i].uGetStream);
                    switch (byStreamType)
                    {
                        //目前NVR仅支持直接从设备取流 NVR supports only the mode: get stream from device directly
                        case 0:
                            IntPtr ptrChanInfo = Marshal.AllocHGlobal((Int32)dwSize);
                            Marshal.StructureToPtr(m_struIpParaCfgV40.struStreamMode[i].uGetStream, ptrChanInfo, false);
                            m_struChanInfo = (CHCNetSDK.NET_DVR_IPCHANINFO)Marshal.PtrToStructure(ptrChanInfo, typeof(CHCNetSDK.NET_DVR_IPCHANINFO));

                            //列出IP通道 List the IP channel
                            ListIPChannel(i + 1, m_struChanInfo.byEnable, m_struChanInfo.byIPID);
                            iIPDevID[i] = m_struChanInfo.byIPID + m_struChanInfo.byIPIDHigh * 256 - iGroupNo * 64 - 1;

                            Marshal.FreeHGlobal(ptrChanInfo);
                            break;

                        case 6:
                            IntPtr ptrChanInfoV40 = Marshal.AllocHGlobal((Int32)dwSize);
                            Marshal.StructureToPtr(m_struIpParaCfgV40.struStreamMode[i].uGetStream, ptrChanInfoV40, false);
                            m_struChanInfoV40 = (CHCNetSDK.NET_DVR_IPCHANINFO_V40)Marshal.PtrToStructure(ptrChanInfoV40, typeof(CHCNetSDK.NET_DVR_IPCHANINFO_V40));

                            //列出IP通道 List the IP channel
                            ListIPChannel(i + 1, m_struChanInfoV40.byEnable, m_struChanInfoV40.wIPID);
                            iIPDevID[i] = m_struChanInfoV40.wIPID - iGroupNo * 64 - 1;

                            Marshal.FreeHGlobal(ptrChanInfoV40);
                            break;

                        default:
                            break;
                    }
                }
            }
            Marshal.FreeHGlobal(ptrIpParaCfgV40);
        }

        public void ListIPChannel(Int32 iChanNo, byte byOnline, int byIPID)
        {
            str1 = String.Format("IPCamera {0}", iChanNo);
            m_lTree++;

            if (byIPID == 0)
            {
                str2 = "X"; //通道空闲，没有添加前端设备 the channel is idle
            }
            else
            {
                if (byOnline == 0)
                {
                    str2 = "offline"; //通道不在线 the channel is off-line
                }
                else
                    str2 = "online"; //通道在线 The channel is on-line
            }

            // listViewIPChannel.Items.Add(new ListViewItem(new string[] { str1, str2 }));//将通道添加到列表中 add the channel to the list
        }

        public void ListAnalogChannel(Int32 iChanNo, byte byEnable)
        {
            str1 = String.Format("Camera {0}", iChanNo);
            m_lTree++;

            if (byEnable == 0)
            {
                str2 = "Disabled"; //通道已被禁用 This channel has been disabled
            }
            else
            {
                str2 = "Enabled"; //通道处于启用状态 This channel has been enabled
            }

            // listViewIPChannel.Items.Add(new ListViewItem(new string[] { str1, str2 }));//将通道添加到列表中 add the channel to the list
        }

        private void listViewIPChannel_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (listViewIPChannel.SelectedItems.Count > 0)
            {
                iSelIndex = listViewIPChannel.SelectedItems[0].Index;  //当前选中的行
            }
        }

        //解码回调函数
        private void DecCallbackFUN(int nPort, IntPtr pBuf, int nSize, ref PlayCtrl.FRAME_INFO pFrameInfo, int nReserved1, int nReserved2)
        {
            // 将pBuf解码后视频输入写入文件中（解码后YUV数据量极大，尤其是高清码流，不建议在回调函数中处理）
            if (pFrameInfo.nType == 3) //#define T_YV12	3
            {
            }
        }

        private void StartLiveView()
        {
            // نمایش زنده ویدیو ها  // Video Live
            CHCNetSDK.NET_DVR_PREVIEWINFO lpPreviewInfoA = new CHCNetSDK.NET_DVR_PREVIEWINFO();
            lpPreviewInfoA.hPlayWnd = picCamLeft.Handle;//预览窗口 live view window
            lpPreviewInfoA.lChannel = iChannelNum[(int)iSelIndex];//预览的设备通道 the device channel number
            lpPreviewInfoA.dwStreamType = 0;//码流类型：0-主码流，1-子码流，2-码流3，3-码流4，以此类推
            lpPreviewInfoA.dwLinkMode = 0;//连接方式：0- TCP方式，1- UDP方式，2- 多播方式，3- RTP方式，4-RTP/RTSP，5-RSTP/HTTP
            lpPreviewInfoA.bBlocked = true; //0- 非阻塞取流，1- 阻塞取流

            IntPtr pUser = IntPtr.Zero;//用户数据 user data

            if (true)
            {
                //打开预览 Start live view
                CameraHandel_top = CHCNetSDK.NET_DVR_RealPlay_V40(Camera_id_top, ref lpPreviewInfoA, null/*RealData*/, pUser);
            }
        }

        private void StopLiveView()
        {
            try
            {
                CHCNetSDK.NET_DVR_StopRealPlay(CameraHandel_top);
                CameraHandel_top = -1;

                //btnStart.Enabled = true;
            }
            catch
            {
                //  StopAllTimers();
                app.SystemError("Error in Stopping Camera\nContact SSF Groups");

                return;
            }
        }

        private void camraConnect()
        {
            try
            {
                string ip_address_top = "192.168.2.200"; // top

                Int16 DVRPortNumber = 8000;
                string DVRUserName = "admin";
                string DVRPassword = "12345";
                //登录设备 Login the device
                Camera_id_top = CHCNetSDK.NET_DVR_Login_V30(ip_address_top, DVRPortNumber, DVRUserName, DVRPassword, ref DeviceInfo_top);
                dwAChanTotalNum = (uint)DeviceInfo_top.byChanNum;

                for (i = 0; i < dwAChanTotalNum; i++)
                {
                    ListAnalogChannel(i + 1, 1);
                    iChannelNum[i] = i + (int)DeviceInfo_top.byStartChan;
                }
                if (Camera_id_top < 0)
                {
                    if (MessageBox.Show("IP Cameras Connecting Error \n\nContact SSF Groups\n+98 9370047967 P.Majidi", "Fatal Error - IP Camera", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == DialogResult.Retry)
                    {
                        btnStart_Click(null, null);
                        return;
                    }
                }
                if (m_bRecord)
                {
                    MessageBox.Show("Please stop recording firstly!");
                    return;
                }
                if (CameraHandel_top < 0)
                {
                    //  StartLiveView();

                    if (CameraHandel_top < 0)
                    {
                        iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                        str = "NET_DVR_RealPlay_V40 failed, error code= " + iLastErr; //预览失败，输出错误号 failed to start live view, and output the error code.
                        DebugInfo(str);
                        return;
                    }
                    else
                    {
                        //预览成功
                        DebugInfo("NET_DVR_RealPlay_V40 succ!");
                        // btnStart.Text = "Stop View";
                    }
                }
                else
                {
                    //停止预览 Stop live view
                    if (!CHCNetSDK.NET_DVR_StopRealPlay(CameraHandel_top))
                    {
                        iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                        str = "NET_DVR_StopRealPlay failed, error code= " + iLastErr;
                        DebugInfo(str);
                        return;
                    }

                    if ((true) && (m_lPort >= 0))
                    {
                        if (!PlayCtrl.PlayM4_Stop(m_lPort))
                        {
                            iLastErr = PlayCtrl.PlayM4_GetLastError(m_lPort);
                            str = "PlayM4_Stop failed, error code= " + iLastErr;
                            DebugInfo(str);
                        }
                        if (!PlayCtrl.PlayM4_CloseStream(m_lPort))
                        {
                            iLastErr = PlayCtrl.PlayM4_GetLastError(m_lPort);
                            str = "PlayM4_CloseStream failed, error code= " + iLastErr;
                            DebugInfo(str);
                        }
                        if (!PlayCtrl.PlayM4_FreePort(m_lPort))
                        {
                            iLastErr = PlayCtrl.PlayM4_GetLastError(m_lPort);
                            str = "PlayM4_FreePort failed, error code= " + iLastErr;
                            DebugInfo(str);
                        }
                        m_lPort = -1;
                    }

                    DebugInfo("NET_DVR_StopRealPlay succ!");
                    CameraHandel_top = -1;
                    //btnStart.Text= "Live View";
                    picCamLeft.Invalidate();//刷新窗口 refresh the window
                }
            }
            catch
            {
                //btnStart.Enabled = true;
                //btnStop.Enabled = false;

                app.SystemError("Error in Connecting Camera\nContact SSF Groups");
                Application.Exit();
                this.Close();
                return;
            }

            //btnStart.Enabled = false;
        }

        private void btnPreview_Click(object sender, EventArgs e)
        {
            if (Camera_id_top < 0)
            {
                MessageBox.Show("Please login the device firstly!");
                return;
            }

            if (m_bRecord)
            {
                MessageBox.Show("Please stop recording firstly!");
                return;
            }
            //    MessageBox.Show();
            if (CameraHandel_top < 0)
            {
                CHCNetSDK.NET_DVR_PREVIEWINFO lpPreviewInfo = new CHCNetSDK.NET_DVR_PREVIEWINFO();
                lpPreviewInfo.hPlayWnd = picCamLeft.Handle;//预览窗口 live view window
                lpPreviewInfo.lChannel = iChannelNum[(int)iSelIndex];//预览的设备通道 the device channel number
                lpPreviewInfo.dwStreamType = 0;//码流类型：0-主码流，1-子码流，2-码流3，3-码流4，以此类推
                lpPreviewInfo.dwLinkMode = 0;//连接方式：0- TCP方式，1- UDP方式，2- 多播方式，3- RTP方式，4-RTP/RTSP，5-RSTP/HTTP
                lpPreviewInfo.bBlocked = true; //0- 非阻塞取流，1- 阻塞取流

                IntPtr pUser = IntPtr.Zero;//用户数据 user data

                if (true)
                {
                    //打开预览 Start live view
                    CameraHandel_top = CHCNetSDK.NET_DVR_RealPlay_V40(Camera_id_top, ref lpPreviewInfo, null/*RealData*/, pUser);
                }
                else
                {
                    lpPreviewInfo.hPlayWnd = IntPtr.Zero;//预览窗口 live view window
                    m_ptrRealHandle = picCamLeft.Handle;
                    RealData = new CHCNetSDK.REALDATACALLBACK(RealDataCallBack);//预览实时流回调函数 real-time stream callback function
                    CameraHandel_top = CHCNetSDK.NET_DVR_RealPlay_V40(Camera_id_top, ref lpPreviewInfo, RealData, pUser);
                }

                if (CameraHandel_top < 0)
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    str = "NET_DVR_RealPlay_V40 failed, error code= " + iLastErr; //预览失败，输出错误号 failed to start live view, and output the error code.
                    DebugInfo(str);
                    return;
                }
                else
                {
                    //预览成功
                    DebugInfo("NET_DVR_RealPlay_V40 succ!");
                    //btnPreview.Text = "Stop View";
                }
            }
            else
            {
                //停止预览 Stop live view
                if (!CHCNetSDK.NET_DVR_StopRealPlay(CameraHandel_top))
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    str = "NET_DVR_StopRealPlay failed, error code= " + iLastErr;
                    DebugInfo(str);
                    return;
                }

                if ((true) && (m_lPort >= 0))
                {
                    if (!PlayCtrl.PlayM4_Stop(m_lPort))
                    {
                        iLastErr = PlayCtrl.PlayM4_GetLastError(m_lPort);
                        str = "PlayM4_Stop failed, error code= " + iLastErr;
                        DebugInfo(str);
                    }
                    if (!PlayCtrl.PlayM4_CloseStream(m_lPort))
                    {
                        iLastErr = PlayCtrl.PlayM4_GetLastError(m_lPort);
                        str = "PlayM4_CloseStream failed, error code= " + iLastErr;
                        DebugInfo(str);
                    }
                    if (!PlayCtrl.PlayM4_FreePort(m_lPort))
                    {
                        iLastErr = PlayCtrl.PlayM4_GetLastError(m_lPort);
                        str = "PlayM4_FreePort failed, error code= " + iLastErr;
                        DebugInfo(str);
                    }
                    m_lPort = -1;
                }

                DebugInfo("NET_DVR_StopRealPlay succ!");
                CameraHandel_top = -1;
                // btnPreview.Text = "Live View";
                picCamLeft.Invalidate();//刷新窗口 refresh the window
            }
            return;
        }

        private void btnJPEG_Click(object sender, EventArgs e)
        {
            int lChannel = iChannelNum[(int)iSelIndex]; //通道号 Channel number

            CHCNetSDK.NET_DVR_JPEGPARA lpJpegPara = new CHCNetSDK.NET_DVR_JPEGPARA();
            lpJpegPara.wPicQuality = 0; //图像质量 Image quality
            lpJpegPara.wPicSize = 0xff; //抓图分辨率 Picture size: 0xff-Auto(使用当前码流分辨率)
                                        //抓图分辨率需要设备支持，更多取值请参考SDK文档

            //JPEG抓图保存成文件 Capture a JPEG picture
            string sJpegPicFileName;
            sJpegPicFileName = "filetest.jpg";//图片保存路径和文件名 the path and file name to save

            if (!CHCNetSDK.NET_DVR_CaptureJPEGPicture(Camera_id_top, lChannel, ref lpJpegPara, sJpegPicFileName))
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                str = "NET_DVR_CaptureJPEGPicture failed, error code= " + iLastErr;
                DebugInfo(str);
                return;
            }
            else
            {
                str = "NET_DVR_CaptureJPEGPicture succ and the saved file is " + sJpegPicFileName;
                DebugInfo(str);
            }

            //JEPG抓图，数据保存在缓冲区中 Capture a JPEG picture and save in the buffer
            uint iBuffSize = 400000; //缓冲区大小需要不小于一张图片数据的大小 The buffer size should not be less than the picture size
            byte[] byJpegPicBuffer = new byte[iBuffSize];
            uint dwSizeReturned = 0;

            if (!CHCNetSDK.NET_DVR_CaptureJPEGPicture_NEW(Camera_id_top, lChannel, ref lpJpegPara, byJpegPicBuffer, iBuffSize, ref dwSizeReturned))
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                str = "NET_DVR_CaptureJPEGPicture_NEW failed, error code= " + iLastErr;
                DebugInfo(str);
                return;
            }
            else
            {
                //将缓冲区里的JPEG图片数据写入文件 save the data into a file
                string str = "buffertest.jpg";
                FileStream fs = new FileStream(str, FileMode.Create);
                int iLen = (int)dwSizeReturned;
                fs.Write(byJpegPicBuffer, 0, iLen);
                fs.Close();

                str = "NET_DVR_CaptureJPEGPicture_NEW succ and save the data in buffer to 'buffertest.jpg'.";
                DebugInfo(str);
            }

            return;
        }

        private void cameraExit()
        {
            //停止预览
            if (CameraHandel_top >= 0)
            {
                CHCNetSDK.NET_DVR_StopRealPlay(CameraHandel_top);
                CameraHandel_top = -1;
            }

            //注销登录
            if (Camera_id_top >= 0)
            {
                CHCNetSDK.NET_DVR_Logout(Camera_id_top);
                Camera_id_top = -1;
            }

            CHCNetSDK.NET_DVR_Cleanup();

            Application.Exit();
        }

        public void start_Shown(object sender, EventArgs e)
        {
       
            this.Cursor = Cursors.WaitCursor;
            m_bInitSDK = CHCNetSDK.NET_DVR_Init();
            if (m_bInitSDK == false)
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show("NET_DVR_Init error!");
                return;
            }
            else
            {
                //保存SDK日志 To save the SDK log
                CHCNetSDK.NET_DVR_SetLogToFile(3, app.WorkingPath, true);

                //comboBoxView.SelectedIndex = 0;

                for (int i = 0; i < 64; i++)
                {
                    iIPDevID[i] = -1;
                    iChannelNum[i] = -1;
                }
            }
        }

        public void DebugInfo(string str)
        {
            if (str.Length > 0)
            {
                str += "\n";
                // TextBoxInfo.AppendText(str);
            }
        }

        public void RealDataCallBack(Int32 lRealHandle, UInt32 dwDataType, IntPtr pBuffer, UInt32 dwBufSize, IntPtr pUser)
        {
            MyDebugInfo AlarmInfo = new MyDebugInfo(DebugInfo);
            switch (dwDataType)
            {
                case CHCNetSDK.NET_DVR_SYSHEAD:     // sys head
                    if (dwBufSize > 0)
                    {
                        //获取播放句柄 Get the port to play
                        if (!PlayCtrl.PlayM4_GetPort(ref m_lPort))
                        {
                            iLastErr = PlayCtrl.PlayM4_GetLastError(m_lPort);
                            str = "PlayM4_GetPort failed, error code= " + iLastErr;
                            this.BeginInvoke(AlarmInfo, str);
                            break;
                        }

                        //设置流播放模式 Set the stream mode: real-time stream mode
                        if (!PlayCtrl.PlayM4_SetStreamOpenMode(m_lPort, PlayCtrl.STREAME_REALTIME))
                        {
                            iLastErr = PlayCtrl.PlayM4_GetLastError(m_lPort);
                            str = "Set STREAME_REALTIME mode failed, error code= " + iLastErr;
                            this.BeginInvoke(AlarmInfo, str);
                        }

                        //打开码流，送入头数据 Open stream
                        if (!PlayCtrl.PlayM4_OpenStream(m_lPort, pBuffer, dwBufSize, 2 * 1024 * 1024))
                        {
                            iLastErr = PlayCtrl.PlayM4_GetLastError(m_lPort);
                            str = "PlayM4_OpenStream failed, error code= " + iLastErr;
                            this.BeginInvoke(AlarmInfo, str);
                            break;
                        }

                        //设置显示缓冲区个数 Set the display buffer number
                        if (!PlayCtrl.PlayM4_SetDisplayBuf(m_lPort, 15))
                        {
                            iLastErr = PlayCtrl.PlayM4_GetLastError(m_lPort);
                            str = "PlayM4_SetDisplayBuf failed, error code= " + iLastErr;
                            this.BeginInvoke(AlarmInfo, str);
                        }

                        //设置显示模式 Set the display mode
                        if (!PlayCtrl.PlayM4_SetOverlayMode(m_lPort, 0, 0/* COLORREF(0)*/)) //play off screen
                        {
                            iLastErr = PlayCtrl.PlayM4_GetLastError(m_lPort);
                            str = "PlayM4_SetOverlayMode failed, error code= " + iLastErr;
                            this.BeginInvoke(AlarmInfo, str);
                        }

                        //设置解码回调函数，获取解码后音视频原始数据 Set callback function of decoded data
                        m_fDisplayFun = new PlayCtrl.DECCBFUN(DecCallbackFUN);
                        if (!PlayCtrl.PlayM4_SetDecCallBackEx(m_lPort, m_fDisplayFun, IntPtr.Zero, 0))
                        {
                            this.BeginInvoke(AlarmInfo, "PlayM4_SetDisplayCallBack fail");
                        }

                        //开始解码 Start to play
                        if (!PlayCtrl.PlayM4_Play(m_lPort, m_ptrRealHandle))
                        {
                            iLastErr = PlayCtrl.PlayM4_GetLastError(m_lPort);
                            str = "PlayM4_Play failed, error code= " + iLastErr;
                            this.BeginInvoke(AlarmInfo, str);
                            break;
                        }
                    }
                    break;

                case CHCNetSDK.NET_DVR_STREAMDATA:     // video stream data
                    if (dwBufSize > 0 && m_lPort != -1)
                    {
                        for (int i = 0; i < 999; i++)
                        {
                            //送入码流数据进行解码 Input the stream data to decode
                            if (!PlayCtrl.PlayM4_InputData(m_lPort, pBuffer, dwBufSize))
                            {
                                iLastErr = PlayCtrl.PlayM4_GetLastError(m_lPort);
                                str = "PlayM4_InputData failed, error code= " + iLastErr;
                                Thread.Sleep(2);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    break;

                default:
                    if (dwBufSize > 0 && m_lPort != -1)
                    {
                        //送入其他数据 Input the other data
                        for (int i = 0; i < 999; i++)
                        {
                            if (!PlayCtrl.PlayM4_InputData(m_lPort, pBuffer, dwBufSize))
                            {
                                iLastErr = PlayCtrl.PlayM4_GetLastError(m_lPort);
                                str = "PlayM4_InputData failed, error code= " + iLastErr;
                                Thread.Sleep(2);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    break;
            }
        }

        private void cameraDisconnect()
        {
            listViewIPChannel.Items.Clear();//清空通道列表 Clean up the channel list
            CHCNetSDK.NET_DVR_Logout(Camera_id_top);
            Camera_id_top = -1;
        }

        private int m_lRealHandle = -1;

        private void playOnBig(int userid)
        {
            CHCNetSDK.NET_DVR_StopRealPlay(m_lRealHandle);
            picCamLeft.Visible = true;
            CHCNetSDK.NET_DVR_PREVIEWINFO lpPreviewInfoBig = new CHCNetSDK.NET_DVR_PREVIEWINFO();
            lpPreviewInfoBig.hPlayWnd = picCamLeft.Handle;//预览窗口 live view window
            lpPreviewInfoBig.lChannel = iChannelNum[(int)iSelIndex];//预览的设备通道 the device channel number
            lpPreviewInfoBig.dwStreamType = 0;//码流类型：0-主码流，1-子码流，2-码流3，3-码流4，以此类推
            lpPreviewInfoBig.dwLinkMode = 0;//连接方式：0- TCP方式，1- UDP方式，2- 多播方式，3- RTP方式，4-RTP/RTSP，5-RSTP/HTTP
            lpPreviewInfoBig.bBlocked = true; //0- 非阻塞取流，1- 阻塞取流
            IntPtr pUser = IntPtr.Zero;//用户数据 user data
            m_lRealHandle = CHCNetSDK.NET_DVR_RealPlay_V40(userid, ref lpPreviewInfoBig, null/*RealData*/, pUser);
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
        }

        private bool CameraRunning = false;
        private bool shotted01, shotted02, shotted03;

        private void pictureBox14_Click(object sender, EventArgs e)
        {
            if (!CameraRunning)
            {
                this.Cursor = Cursors.WaitCursor;
                StopLiveView();
                cameraDisconnect();
                //StopLiveView();
                camraConnect();
                // StartLiveView();
                playOnBig(Camera_id_top);
                this.Cursor = Cursors.Default;
                CameraRunning = true;
                picCamLeft.Visible = true;
            }
            else
            {
                CameraRunning = false;
                StopLiveView();
                cameraDisconnect();
                picCamLeft.Visible = false;
            }
        }

      

      

    

        private string[] filenamesdroped;

        private void MainFRM_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
                filenamesdroped = (string[])e.Data.GetData(DataFormats.FileDrop);
            }
        }

        private void MainFRM_DragOver(object sender, DragEventArgs e)
        {
        }

        private void MainFRM_DragDrop(object sender, DragEventArgs e)
        {
            
        }

        private bool handyBarcode = false;

        public string pathoSavePhoto { get; private set; }

        private void picCamLeft_Click(object sender, EventArgs e)
        {
            if (!CameraRunning) return;
            try
            {
                shotImage(Camera_id_top, "pic.jpg");
                System.Diagnostics.Process.Start("pic.jpg");
            }
            catch
            {
            }
        }

        private Image shotImage(int cameraID, string path)
        {
            int lChannel = iChannelNum[(int)iSelIndex]; //通道号 Channel number

            CHCNetSDK.NET_DVR_JPEGPARA lpJpegPara = new CHCNetSDK.NET_DVR_JPEGPARA();
            lpJpegPara.wPicQuality = 0; //图像质量 Image quality
            lpJpegPara.wPicSize = 0xff; //抓图分辨率 Picture size: 0xff-Auto(使用当前码流分辨率)
            //抓图分辨率需要设备支持，更多取值请参考SDK文档
            //JPEG抓图保存成文件 Capture a JPEG picture
            string sJpegPicFileName;
            sJpegPicFileName = path;

           try
            {
                File.Delete(path);
            }
            catch
            {
            }
            try
            {
                CHCNetSDK.NET_DVR_CaptureJPEGPicture(cameraID, lChannel, ref lpJpegPara, sJpegPicFileName);
                return Image.FromFile(sJpegPicFileName);
            }
            catch
            {
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();

            return null;
        }


        // END OF CAMERA .............................................................................................
    }
}