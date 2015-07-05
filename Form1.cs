using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Resources;
using System.Collections;
using System.Text.RegularExpressions;
using System.Configuration;
using System.Reflection;

namespace WindowsFormsRestore
{
    public partial class Form1 : Form
    {
        private string selectPath;

        public Form1()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            //设置打开文件对话框
            FolderBrowserDialog dlg = new FolderBrowserDialog()
            {
                SelectedPath = GetConfig("path", "C:\\")
                //从配置文件读取默认
            };
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.richTextBox1.Text = "";
                SaveConfig("path", dlg.SelectedPath);
                selectPath = string.Concat(dlg.SelectedPath, "\\"); //选择路径
                string[] files = Directory.GetFiles(selectPath, "*.csproj");//得到要修改的文件
                string strProjFile = null;
                if ((int)files.Length > 0)
                {
                    strProjFile = File.ReadAllText(files[0]);//读出项目文件内容
                }
                ModifyAllCSFile(selectPath, ref strProjFile); //处理CS文件
                RestoreResXFromResource(selectPath, ref strProjFile); //处理资源文件
                if (strProjFile != null)
                {
                    File.WriteAllText(files[0], strProjFile); //更新csproj
                    UpdateMsg(files[0].Substring(selectPath.Length)); //消息更新
                }
                DateTime now = DateTime.Now;
                UpdateMsg(string.Concat("处理完成", now.ToString())); //处理完成
            }
        }

        /// <summary>
        /// 从resource文件还原resx文件
        /// </summary>
        /// <param name="strPath">文件路径</param>
        /// <param name="strCsProj">csproj内容</param>
        private void RestoreResXFromResource(string strPath, ref string strCsProj)
        {
            string[] files = Directory.GetFiles(strPath, "*.resources");//查询文件
            for (int i = 0; i < (int)files.Length; i++)
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(files[i].Substring(selectPath.Length));
                //取文件名去掉扩展名
                string strResxFile = Path.Combine(strPath, string.Concat(string.Join("\\", fileNameWithoutExtension.Split(new char[] { '.' })), ".resx")); 
                //resx文件路径
                ResourceReader resourceReaders = new ResourceReader(files[i]);  //读取资源文件
                ResXResourceWriter resXResourceWriter = new ResXResourceWriter(strResxFile); //打开resx文件
                foreach (DictionaryEntry resourceReader in resourceReaders)
                {//从resource文件中读取资源写入resx文件
                    try
                    {
                        resXResourceWriter.AddResource(resourceReader.Key.ToString(), resourceReader.Value);
                    }
                    catch (Exception ex)
                    {
                        UpdateMsg(resourceReader.Key.ToString() + "\r\n" + ex.ToString());
                    }
                }
                resXResourceWriter.Close();
                resourceReaders.Close();
                UpdateMsg(strResxFile.Substring(selectPath.Length));//resx文件更新消息
                if (strCsProj != null)
                {//更新csproj
                    string strResXName = strResxFile.Substring(selectPath.Length);
                    //resx文件名
                    if (!strCsProj.Contains(string.Concat("=\"", strResXName, "\"")))
                    {//csproj中不包含则添加
                        int num = strCsProj.LastIndexOf("</ItemGroup>");//最后位置
                        if (num <= 0)
                        {//适应低版本
                            num = strCsProj.LastIndexOf("=\"EmbeddedResource\"");
                            strCsProj = string.Concat(strCsProj.Substring(0, num)
                                , "=\"EmbeddedResource\" />\r\n\t\t<File RelPath=\""
                                , strResXName
                                , "\" DependentUpon=\""
                                , Path.GetFileNameWithoutExtension(strResXName)
                                ,".cs\" BuildAction"
                                , strCsProj.Substring(num));
                        }
                        else
                        {
                            strCsProj = string.Concat(strCsProj.Substring(0, num)
                                , " <EmbeddedResource Include=\""
                                , strResXName
                                , "\">\r\n\t<SubType>Designer</SubType>\r\n\t<DependentUpon>"
                                , Path.GetFileNameWithoutExtension(strResXName)
                                , ".cs</DependentUpon>\r\n\t</EmbeddedResource>"
                                , strCsProj.Substring(num));
                            //插入resx文件引用

                            strCsProj = strCsProj.Replace(string.Concat("<EmbeddedResource Include=\""
                                , Path.GetFileName(files[i])
                                , "\" />")
                                , "");//删除resources文件引用
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 处理目录下包括子目录的CS文件
        /// </summary>
        /// <param name="strDir">处理目录</param>
        /// <param name="strProjStr">csproj内容</param>
        private void ModifyAllCSFile(string strDir, ref string strProjStr)
        {
            string[] files = Directory.GetFiles(strDir, "*.cs");//查询所有CS文件
            for (int i = 0; i < (int)files.Length; i++)
            {
                ModifyCsFile(files[i], ref strProjStr);
            }
            string[] directories = Directory.GetDirectories(strDir);//获取子目录
            for (int j = 0; j < (int)directories.Length; j++)
            {
                ModifyAllCSFile(directories[j], ref strProjStr);
            }
        }

        /// <summary>
        /// 处理CS文件
        /// </summary>
        /// <param name="strFilePath">CS文件路径</param>
        /// <param name="strProjStr">项目文件</param>
        private void ModifyCsFile(string strFilePath, ref string strProjStr)
        {
            string strDesignFile = string.Concat(Path.GetDirectoryName(strFilePath), "\\" , Path.GetFileNameWithoutExtension(strFilePath) , ".Designer.cs");
            //Designer.cs文件路径
            StreamReader reader = new StreamReader(strFilePath, true);
            string strOrgCode = reader.ReadToEnd(); //读取CS文件
            string strCode = strOrgCode;
            if (this.chk_h2d.Checked)
            {
                //16进制转10进制
                strCode = Regex.Replace(strCode, "\\b0x([0-9a-f]+)[Ll]?\\b"
                    , (Match regx) => { return long.Parse(regx.Groups[1].Value, System.Globalization.NumberStyles.HexNumber).ToString(); }, RegexOptions.Compiled);
            }
            if ((this.chk_Cover.Checked || !File.Exists(strDesignFile)) && !strFilePath.EndsWith(".Designer.cs") && strCode.Contains(" InitializeComponent") && strCode.Contains("SuspendLayout();"))
            {//检查是否为需要更新的文件
                StringBuilder stringBuilder = new StringBuilder();

                stringBuilder.Append(GetDesignerString(ref strCode, "^", "[\\w ]+class", false, false));
                //第一部分,class前的数据
                stringBuilder.Append(string.Concat("\tpartial class ", GetDesignerString(ref strCode, "(?<=[\\w ]+class\\s+)\\w+", "\\s", false, false), "\r\n\t{\r\n"));  
                //第二部分，class名称
                strCode = Regex.Replace(strCode, "\\bclass\\b", "partial class");  
                //增加原字符串中的class修饰符partial
                stringBuilder.Append(string.Concat(GetFunctionString(ref strCode, "Dispose"), "\r\n")); 
                //Dispose方法
                string strInitial = GetFunctionString(ref strCode, "InitializeComponent"); //获取InitializeComponent方法
                strInitial = AddFullName(strInitial); //对内容修正，将控件方法补齐
                stringBuilder.Append(string.Concat(Regex.Replace(strInitial, "\\bmanager\\b", "resources"), "\r\n")); //附加
                //InitializeComponent方法
                //stringBuilder.Append(GetDesignerString(ref strCode, "[ \\t\\w\\.]+components", "\\r?\\n", true, true)); //附加
                ////components

                int num = 0;
                string strVar = string.Empty;
                while (true)
                {//循环查找private和protected变量声明
                    string strV = GetDesignerString(ref strCode, ref num, "[ \\t]*\\s(private|protected)\\s+[\\w\\.]+\\s+\\w+;", "\\r?\\n", true, true);
                    if (strV == null)
                    {
                        break;
                    }
                    strVar += strV;
                }

                stringBuilder.Append(strVar);
                stringBuilder.Append("\t}\r\n}");//附加结束符

                File.WriteAllText(strDesignFile, stringBuilder.ToString(), reader.CurrentEncoding); 
                //写入designer.cs文件
                string strDesignerFileName = strDesignFile.Substring(selectPath.Length);
                UpdateMsg(strDesignerFileName);  //更新消息design.cs
                if (strProjStr != null)
                {//更新csproj文件，加入Designer.cs引用
                    string strCsFile = strFilePath.Substring(selectPath.Length);
                    if (strProjStr.Contains(string.Concat("<Compile Include=\"", strCsFile, "\"")) && !strProjStr.Contains(string.Concat("<Compile Include=\"", strDesignerFileName, "\"")))
                    {//2010版本
                        //strProjStr = UpdateProj(strFilePath, strProjStr, strCsFile, strDesignerFileName);
                        strProjStr = Regex.Replace(strProjStr,
                                        string.Concat("\\<Compile Include=\"", strCsFile.Replace("\\", "\\\\"), "\"[\\s\\S]+?(\\</Compile\\>|\\s\\/\\>)"),
                                        (Match regx) => string.Concat(regx.Value, " <Compile Include=\"", strDesignerFileName, "\">\r\n\t <DependentUpon>", Path.GetFileName(strFilePath), "</DependentUpon>\r\n\t</Compile>"));
                    }
                    else if (!strProjStr.Contains(string.Concat("<File RelPath=\"", strDesignerFileName, "\"")))
                    {//旧版本
                        strProjStr = Regex.Replace(strProjStr
                            , string.Concat("\\<File RelPath=\"", strCsFile.Replace("\\", "\\\\"), "\" SubType=\"Code\" BuildAction=\"Compile\" />\r\n")
                            , (Match regx) => string.Concat(regx.Value, "\t<File RelPath=\"", strDesignerFileName, "\" DependentUpon=\"", Path.GetFileName(strFilePath), "\" BuildAction=\"Compile\" />\r\n"));
                    }
                }
                string strTmp = strFilePath.Substring(selectPath.Length);
                strTmp = strTmp.Remove(strTmp.LastIndexOf('.'));
                string strResx = string.Concat(selectPath, strTmp, ".resx");
                string strResx2 = string.Concat(selectPath, strTmp.Replace('\\', '.'), ".resx");
                if (!File.Exists(strResx) && File.Exists(strResx2))
                {//将资源文件放到相应的目录并更新csproj
                    File.Copy(strResx2, strResx);
                    File.Delete(strResx2);
                    //strProjStr = strProjStr.Replace(strResx2.Substring(selectPath.Length), strResx.Substring(selectPath.Length));
                    strProjStr = Regex.Replace(strProjStr
                        , string.Concat("\\<File RelPath=\"", strTmp.Replace('\\', '.'), ".resx\" BuildAction=\"EmbeddedResource\" />")
                        , (Match regx) => string.Concat("<File RelPath=\"", strResx.Substring(selectPath.Length), "\" DependentUpon=\"", Path.GetFileName(strTmp), ".cs\" BuildAction=\"EmbeddedResource\" />")); 
                    //旧版本更新
                    UpdateMsg(strResx2.Substring(selectPath.Length));
                }
            }
            Encoding coding = reader.CurrentEncoding;
            reader.Close();
            if (strCode != strOrgCode)
            {   //如果有更改，写入文件
                File.WriteAllText(strFilePath, strCode, coding);
                UpdateMsg(strFilePath.Substring(selectPath.Length));
            }
        }

        /// <summary>
        /// 更新日志记录
        /// </summary>
        /// <param name="strMsg"></param>
        private void UpdateMsg(string strMsg)
        {
            richTextBox1.AppendText(string.Concat(strMsg, "\r\n"));
            richTextBox1.SelectionStart = richTextBox1.TextLength;
            richTextBox1.Focus();
            Application.DoEvents();
        }

        /// <summary>
        /// 截取方法内容
        /// </summary>
        /// <param name="strCsStr">CS文件</param>
        /// <param name="strFuncName">方法名称</param>
        /// <returns>截取后的方法内容</returns>
        private string GetFunctionString(ref string strCsStr, string strFuncName)
        {
            int num = 0;
            string strFunc = GetDesignerString(ref strCsStr, ref num, string.Concat("[\\w \\t]+\\s", strFuncName, "\\("), "\\s+(?=[ \t]*\\s(private\\s|public\\s|protected\\s|internal\\s|$))", true, true);
            //截取方法后从源内容中删除
            if (num + 1 == strCsStr.Length)
            {
                char[] chrArray = new char[] { ' ', '\r', '\n' };
                string str2 = strFunc.TrimEnd(chrArray).TrimEnd(new char[] { '}' });//删除末尾的换行和结束括号
                char[] chrArray1 = new char[] { ' ', '\r', '\n' };
                strFunc = str2.TrimEnd(chrArray1).TrimEnd(new char[] { '}' });//删除末尾的换行和结束括号
                strCsStr = string.Concat(strCsStr, "\r\n\t}\r\n}"); //原始字符串末尾附加 应该是结束括号
            }
            return strFunc;
        }

        /// <summary>
        /// 从起始位置开始截取字符串
        /// </summary>
        /// <param name="strPointers">要检索的字符串</param>
        /// <param name="strRegx">正则表达式一</param>
        /// <param name="strRegx1">正则表达式二</param>
        /// <param name="bSkip">是否包含第二部分匹配内容</param>
        /// <param name="bDelete">是否删除截取内容</param>
        /// <returns>截取的字符串</returns>
        private string GetDesignerString(ref string strPointers, string strRegx, string strRegx1, bool bSkip, bool bDelete)
        {
            int num = 0;
            return GetDesignerString(ref strPointers, ref num, strRegx, strRegx1, bSkip, bDelete);
        }

        /// <summary>
        /// 根据正则表达式截取字符串
        /// </summary>
        /// <param name="strPointers">要检索的字符串</param>
        /// <param name="numPointer">检索起始位置，返回完成后的匹配位置</param>
        /// <param name="strRegx">正则表达式一</param>
        /// <param name="strRegx1">正则表达式二</param>
        /// <param name="bSkip">是否包含第二部分匹配内容</param>
        /// <param name="bDelete">是否删除截取内容</param>
        /// <returns>截取的字符串</returns>
        private string GetDesignerString(ref string strPointers, ref int numPointer, string strRegx, string strRegx1, bool bSkip, bool bDelete)
        {
            string strResult = null;
            Regex regex = new Regex(strRegx);
            Match match = regex.Match(strPointers, numPointer);//匹配正则表达式
            if (match.Success)
            {
                numPointer = match.Index + match.Length;//位置指针移到匹配项之后
                Regex regex1 = new Regex(strRegx1);
                Match match1 = regex1.Match(strPointers, numPointer); //在后面匹配另一个正则表达式
                if (match1.Success)
                {
                    numPointer = match1.Index + (bSkip ? match1.Length : 0); //是否包含第二部分
                    strResult = strPointers.Substring(match.Index, numPointer - match.Index); //截取匹配内容
                    if (bDelete)
                    {//如果需要删除匹配内容，执行删除并处理长度
                        strPointers = string.Concat(strPointers.Substring(0, match.Index), strPointers.Substring(numPointer));
                        numPointer = numPointer - strResult.Length;
                    }
                }
            }
            return strResult;
        }

        /// <summary>
        /// 补齐控件方法全名
        /// </summary>
        /// <param name="str">需要补齐的内容</param>
        /// <returns>补齐后的内容</returns>
        private static string AddFullName(string str)
        {
            Assembly _Assembyle = Assembly.GetAssembly(typeof(System.Windows.Forms.Label));
            Type[] _TypeList = _Assembyle.GetTypes();
            for (int i = 0; i < _TypeList.Length; i++)
            {
                if (_TypeList[i].IsClass && _TypeList[i].Namespace == "System.Windows.Forms")
                {
                    str = str.Replace("new " + _TypeList[i].Name, "new System.Windows.Forms." + _TypeList[i].Name);
                }
            }

            _Assembyle = Assembly.GetAssembly(typeof(System.Drawing.Size));
            _TypeList = _Assembyle.GetTypes();
            for (int i = 0; i < _TypeList.Length; i++)
            {
                if (_TypeList[i].IsClass && _TypeList[i].Namespace == "System.Drawing")
                {
                    str = str.Replace("new " + _TypeList[i].Name, "new System.Drawing." + _TypeList[i].Name);
                }
            }
            str = str.Replace("new EventHandler", "new System.EventHandler");
            str = str.Replace(" ImeMode.", " System.Windows.Forms.ImeMode.");
            str = str.Replace(" AutoScaleMode.", " System.Windows.Forms.AutoScaleMode.");
            str = str.Replace(" FormBorderStyle.", " System.Windows.Forms.FormBorderStyle.");
            str = str.Replace(" SizeGripStyle.", " System.Windows.Forms.SizeGripStyle.");
            str = str.Replace(" DialogResult.", " System.Windows.Forms.DialogResult.");
            str = str.Replace(" BorderStyle.", " System.Windows.Forms.BorderStyle.");
            str = str.Replace(" FormStartPosition.", " System.Windows.Forms.FormStartPosition.");
            str = str.Replace(" ComponentResourceManager", " System.ComponentModel.ComponentResourceManager");
            return str;
        }

        /// <summary>
        /// 更新csproj内容
        /// </summary>
        /// <param name="strCsFullPath">cs文件完整路径</param>
        /// <param name="strProjStr">csproj内容</param>
        /// <param name="strCsFile">cs文件相对路径</param>
        /// <param name="strDesignerCsFile">Design.cs相对路径</param>
        /// <returns>修改后的csproj内容</returns>
        private static string UpdateProj(string strCsFullPath, string strProjStr, string strCsFile, string strDesignerCsFile)
        {
            return Regex.Replace(strProjStr,
                string.Concat("\\<Compile Include=\"", strCsFile.Replace("\\", "\\\\"), "\"[\\s\\S]+?(\\</Compile\\>|\\s\\/\\>)"),
                (Match regx) => string.Concat(new string[] { regx.Value, " <Compile Include=\"", strDesignerCsFile, "\">\r\n\t<DependentUpon>", Path.GetFileName(strCsFullPath), "</DependentUpon>\r\n\t</Compile>" }));
        }

        /// <summary>
        /// 获取配置文件配置
        /// </summary>
        /// <param name="strKey">Key</param>
        /// <param name="strValue">Value</param>
        public static void SaveConfig(string strKey, string strValue)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            ConfigurationManager.AppSettings[strKey] = strValue;
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        /// <summary>
        /// 获取应用程序配置
        /// </summary>
        /// <param name="strKey">配置名称</param>
        /// <param name="strDefaultValue">默认值</param>
        /// <returns>返回数据</returns>
        public static string GetConfig(string strKey, string strDefaultValue)
        {
            string item;
            try
            {
                item = ConfigurationManager.AppSettings[strKey] ?? strDefaultValue;
            }
            catch
            {
                item = strDefaultValue;
            }
            return item;
        }
    }
}
