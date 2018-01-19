﻿using Common;
using Model.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Application
{
    public class WxMessage
    {

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

            responseModel.ToUserName = ReciveModel.FromUserName;
            responseModel.FromUserName = ReciveModel.ToUserName;
            responseModel.CreateTime = TimeHelper.GetTimeStamp(DateTime.Now);
            responseModel.MsgType = "text";
            switch (ReciveModel.MsgType)
            {
                case "text":
                    responseModel.Content = "收到文本消息，内容：" + ReciveModel.Content;
                    break;
                case "image"://图片
                    responseModel.MediaId = "0";
                    break;
                default:
                    responseModel.Content = "收到消息，内容：" + ReciveModel.Content;
                    break;
            }
            

            return responseModel;
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

    }
}
