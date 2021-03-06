﻿using Common;
using Model.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Tencent;

namespace Application
{
    public class WxMessage
    {
        private readonly string Appid = "wxc7b16818ed7c01c2";
        private readonly string Token = "pandahouse";
        private readonly string SEncodingAESKey = "UC0BGzmPRJDUTx7j8FFOlwGbHiOImkye44oZmFEV76w";
        private WXBizMsgCrypt WxDecode;
        public WxMessage()
        {
            this.WxDecode = new WXBizMsgCrypt(Token, SEncodingAESKey, Appid);

        }

        public WxMessageRecXmlModel InitMessageModel(string Msg)
        {
            WxMessageRecXmlModel model = new WxMessageRecXmlModel();

            if (string.IsNullOrEmpty(Msg)) return model;

            //封装请求类
            XmlDocument requestDocXml = new XmlDocument();
            requestDocXml.LoadXml(Msg);
            XmlElement rootElement = requestDocXml.DocumentElement;
            model.ToUserName = rootElement.SelectSingleNode("ToUserName").InnerText;
            model.FromUserName = rootElement.SelectSingleNode("FromUserName").InnerText;
            model.CreateTime = rootElement.SelectSingleNode("CreateTime").InnerText;
            model.MsgType = rootElement.SelectSingleNode("MsgType").InnerText;
            switch (model.MsgType)
            {
                case "text"://文本
                    model.Content = rootElement.SelectSingleNode("Content").InnerText;
                    break;
                case "image"://图片
                    model.PicUrl = rootElement.SelectSingleNode("PicUrl").InnerText;
                    break;
                case "event"://事件
                    model.Event = rootElement.SelectSingleNode("Event").InnerText;
                    if (model.Event == "subscribe")//关注类型
                    {
                        model.EventKey = rootElement.SelectSingleNode("EventKey").InnerText;
                    }
                    break;
                default:
                    model.Content = rootElement.SelectSingleNode("Content").InnerText;
                    break;
            }

            return model;
        }

        public WxMessageResXmlModel ResponseModel(WxMessageRecXmlModel ReciveModel)
        {
            WxMessageResXmlModel responseModel = new WxMessageResXmlModel();

            switch (ReciveModel.MsgType)
            {
                case "text":

                    if (CatchKeyWord.IsKeyWord(ReciveModel.Content))
                    {
                        responseModel.ToUserName = ReciveModel.FromUserName;
                        responseModel.FromUserName = ReciveModel.ToUserName;
                        responseModel.CreateTime = TimeHelper.GetTimeStamp(DateTime.Now);
                        responseModel.MsgType = "text";
                        //处理关键字消息，暂时做文本消息处理
                        responseModel.Content = "收到关键字消息，内容：" + ReciveModel.Content;
                    }
                    else
                    {
                        string user = ReciveModel.FromUserName.Replace("_", "");
                        TuringResponseModel TuringModel = TuringRebotRequest.AskTuring(user, ReciveModel.Content);
                        responseModel = TuringResponseModel(TuringModel, ReciveModel);
                    }
                    break;
                case "image"://图片
                    responseModel.ToUserName = ReciveModel.FromUserName;
                    responseModel.FromUserName = ReciveModel.ToUserName;
                    responseModel.CreateTime = TimeHelper.GetTimeStamp(DateTime.Now);
                    responseModel.MsgType = "text";
                    responseModel.Content = "发的是什么鬼！！";//，内容：" + ReciveModel.Content;
                    break;
                default:
                    responseModel.ToUserName = ReciveModel.FromUserName;
                    responseModel.FromUserName = ReciveModel.ToUserName;
                    responseModel.CreateTime = TimeHelper.GetTimeStamp(DateTime.Now);
                    responseModel.MsgType = "text";
                    responseModel.Content = "虽然你说了那么多，我就当没听见吧。";
                    break;
            }


            return responseModel;
        }

        public bool ResponseModel(WxMessageRecXmlModel ReciveModel, ref string sEncryptMsg)
        {
            WxMessageResXmlModel replyMsg = ResponseModel(ReciveModel);
            string timeStamp = "";
            string nonce = "";
            WxDecode.EncryptMsg(GetResponse(replyMsg), timeStamp, nonce, ref sEncryptMsg);
            return true;
        }

        public WxMessageResXmlModel TuringResponseModel(TuringResponseModel TuringResponseModel, WxMessageRecXmlModel ReciveModel)
        {
            WxMessageResXmlModel WxMessageResXmlModel = new WxMessageResXmlModel();
            WxMessageResXmlModel.ToUserName = ReciveModel.FromUserName;
            WxMessageResXmlModel.FromUserName = ReciveModel.ToUserName;
            WxMessageResXmlModel.CreateTime = TimeHelper.GetTimeStamp(DateTime.Now);

            switch (TuringResponseModel.code)
            {
                case 100000://文本类
                    WxMessageResXmlModel.MsgType = "text";
                    WxMessageResXmlModel.Content = TuringResponseModel.text;
                    break;
                case 200000://链接类
                    WxMessageResXmlModel.MsgType = "text";
                    WxMessageResXmlModel.Content = TuringResponseModel.text + System.Environment.NewLine + TuringResponseModel.url;
                    break;
                case 302000://新闻类
                    WxMessageResXmlModel.MsgType = "text";
                    WxMessageResXmlModel.Content = TuringResponseModel.text;
                    break;
                case 308000://菜谱类
                    WxMessageResXmlModel.MsgType = "text";
                    WxMessageResXmlModel.Content = TuringResponseModel.text;
                    break;
                default:
                    WxMessageResXmlModel.MsgType = "text";
                    WxMessageResXmlModel.Content = TuringResponseModel.text;
                    break;
            }
            return WxMessageResXmlModel;
        }

        public string GetResponse(WxMessageResXmlModel ResponseModel)
        {
            XmlDocument xml = new XmlDocument();

            XmlElement root = xml.CreateElement("xml");
            xml.AppendChild(root);

            XmlElement toUserName = xml.CreateElement("ToUserName");
            XmlCDataSection toName = xml.CreateCDataSection(ResponseModel.ToUserName);
            toUserName.AppendChild(toName);
            root.AppendChild(toUserName);

            XmlElement fromUserName = xml.CreateElement("FromUserName");
            XmlCDataSection fromName = xml.CreateCDataSection(ResponseModel.FromUserName);
            fromUserName.AppendChild(fromName);
            root.AppendChild(fromUserName);

            XmlElement createTime = xml.CreateElement("CreateTime");
            createTime.InnerText = ResponseModel.CreateTime;
            root.AppendChild(createTime);

            XmlElement msgType = xml.CreateElement("MsgType");
            XmlCDataSection type = xml.CreateCDataSection(ResponseModel.MsgType);
            msgType.AppendChild(type);
            root.AppendChild(msgType);

            XmlElement content = xml.CreateElement("Content");
            XmlCDataSection cont = xml.CreateCDataSection(ResponseModel.Content);
            content.AppendChild(cont);
            root.AppendChild(content);

            return xml.InnerXml;
        }

        public string Response(string msg)
        {
            WxMessageRecXmlModel requestModec = InitMessageModel(msg);
            WxMessageResXmlModel responseModel = ResponseModel(requestModec);
            return GetResponse(responseModel);
        }

        public bool Response(string msg, string sTimeStamp, string sNonce,string sMsgSignature, ref string responseMsg)
        {
            int errorCode = WxDecode.DecryptMsg(sMsgSignature, sTimeStamp, sNonce, msg, ref responseMsg);
            if (errorCode != 0)
            {
                string error = ErrorMessage.TranslateErrorCode(errorCode);
                return false;
            }
            else
            {
                WxMessageRecXmlModel requestModec = InitMessageModel(responseMsg);
                return ResponseModel(requestModec, ref responseMsg);
            }

        }
    }
}
