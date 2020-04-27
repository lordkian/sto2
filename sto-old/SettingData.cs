using sto.DataObjects;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace sto
{
    [DataContract]
    public class SettingData
    {
        [DataMember]
        public List<HostPriority> HostPriorities
        {
            get { return Main.HostPriorities; }
            set { Main.HostPriorities = value; }
        }
        [DataMember]
        public int DownloadThread
        {
            get { return Process.DownloadThreadNumber; }
            set { Process.DownloadThreadNumber = value; }
        }
        [DataMember]
        public int ProcessThread
        {
            get { return Process.ProcessThreadNumber; }
            set { Process.ProcessThreadNumber = value; }
        }
        [DataMember]
        public int CaptchaSolvingThread
        {
            get { return Captcha.CaptchaThreadNumber; }
            set { Captcha.CaptchaThreadNumber = value; }
        }
        [DataMember]
        public int CaptchaSaveAfter
        {
            get { return Captcha.CaptchaNumberToSave; }
            set { Captcha.CaptchaNumberToSave = value; }
        }
        [DataMember]
        public int HostFindNumber
        {
            get { return Main.KeepingHost; }
            set { Main.KeepingHost = value; }
        }
        [DataMember]
        public string[] Serie_ListXPath
        {
            get { return Serie.ListXpath; }
            set { Serie.ListXpath = value; }
        }
        [DataMember]
        public string[] Serie_SingleXpath
        {
            get { return Serie.SingleXpath; }
            set { Serie.SingleXpath = value; }
        }
        [DataMember]
        public string[] Staffel_ListXPath
        {
            get { return Staffel.ListXpath; }
            set { Staffel.ListXpath = value; }
        }
        [DataMember]
        public string[] Staffel_SingleXpath
        {
            get { return Staffel.SingleXpath; }
            set { Staffel.SingleXpath = value; }
        }
        [DataMember]
        public string[] Folge_ListXPath
        {
            get { return Folge.ListXpath; }
            set { Folge.ListXpath = value; }
        }
        [DataMember]
        public string[] Folge_SingleXpath
        {
            get { return Folge.SingleXpath; }
            set { Folge.SingleXpath = value; }
        }
        [DataMember]
        public string[] Host_ListXPath
        {
            get { return Host.ListXpath; }
            set { Host.ListXpath = value; }
        }
        [DataMember]
        public string[] Host_SingleXpath
        {
            get { return Host.SingleXpath; }
            set { Host.SingleXpath = value; }
        }

    }
}
