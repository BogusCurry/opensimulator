/*
 * Copyright (c) Contributors, http://opensimulator.org/
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the OpenSimulator Project nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */


using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using log4net;
using Nini.Config;
using OpenMetaverse.StructuredData;


namespace OpenSim.Services.IntegrationService
{
    public static class IntegrationUtils
    {
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
  
        // ****[ Robust ] Only a couple of these are needed in the core management system
        // ****[ Robust ] These could be moved to the implementation instead of being in 
        // ****[ Robust ] a separate file
        //
        #region web utils
        public static bool ParseStringToOSDMap(string input, out OSDMap json)
        {
            try
            {
                json = null;
                OSD tmpbuff = null;

                try
                {
                    tmpbuff = OSDParser.DeserializeJson(input.ToString());
                }
                catch
                {
                    return false;
                }
    
                if (tmpbuff.Type == OSDType.Map)
                {
                    json = (OSDMap)tmpbuff;
                    return true;
                } else
                    return false;
            }
            catch (NullReferenceException e)
            {
                m_log.ErrorFormat("[IUtil]: exception on ParseStringToJson {0}", e.Message);
                json = null;
                return false;
            }
        }

        public static byte[] FailureResult()
        {
            return FailureResult(String.Empty);
        }

        public static byte[] FailureResult(string msg)
        {
            OSDMap doc = new OSDMap(2);
            doc["result"] = OSD.FromString("failure");
            doc["message"] = OSD.FromString(msg);

            return DocToBytes(doc);
        }

        public static byte[] ResponseMessage(string message)
        {
            OSDMap doc = new OSDMap(2);
            doc["result"] = OSD.FromString("success");
            doc["message"] = OSD.FromString(message);

            return DocToBytes(doc);
        }

        public static byte[] DocToBytes(OSDMap doc)
        {
            return Encoding.UTF8.GetBytes(OSDParser.SerializeJsonString(doc));
        }

        public static byte[] DocToBytes(string json)
        {
            return Encoding.UTF8.GetBytes(json);
        }
        #endregion web utils

        #region config utils
        public static IConfigSource GetConfigSource(string IniPath, string IniName)
        {
            string configFilePath = Path.GetFullPath(
                        Path.Combine(IniPath, IniName));

            if (File.Exists(configFilePath))
            {
                IConfigSource config = new IniConfigSource(configFilePath);
                return config;
            }
            else
            {
                return null;
            }
        }

        public static IConfigSource LoadInitialConfig(string url)
        {
            IConfigSource source = new XmlConfigSource();
            m_log.InfoFormat("[CONFIG]: {0} is a http:// URI, fetching ...", url);

            // The ini file path is a http URI
            // Try to read it
            try
            {
                XmlReader r = XmlReader.Create(url);
                IConfigSource cs = new XmlConfigSource(r);
                source.Merge(cs);
            }
            catch (Exception e)
            {
                m_log.FatalFormat("[CONFIG]: Exception reading config from URI {0}\n" + e.ToString(), url);
                Environment.Exit(1);
            }

            return source;
        }
        #endregion config utils

        public static T LoadPlugin<T>(string dllName, Object[] args) where T:class
        {
            return OpenSim.Server.Base.ServerUtils.LoadPlugin<T>(dllName, args);

        }
    }
}