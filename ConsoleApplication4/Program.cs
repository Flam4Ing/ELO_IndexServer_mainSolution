using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using EloixClient.IndexServer;
using System.Text.RegularExpressions;
using System.Globalization;

namespace EBDocu_ELO_Export
{
    class Program
    {
        
        static IXConnection conn;               
        const int findMax = 100000;

        static void Main(string[] args)
        {
            string[] files;
            
            DataAccess da = new DataAccess(); 

            IXConnFactory connFact = new IXConnFactory("http://SERVER-SQL:8080/ix-Projekte_und_Fibu/ix", "", "1.0");
            conn = connFact.Create("Faermann", "Faermann", "EDV-Faermann-8", null);


            string[] directs = getDirectory(da);

            string parentId = setParentId(da); 

            foreach (string direct in directs)
            {
                if (!isFolderDocuExist(nameFolderFile(direct), parentId, false))
                    createFolder(nameFolderFile(direct), parentId);
                files = Directory.GetFiles(direct);
                foreach (string file in files)
                {
                    if (!isFolderDocuExist(nameFolderFile(file), parentId,true))
                        createDocument(file, String.Format("{0}/{1}", parentId, nameFolderFile(direct)));
                }
            } 
        }

        static string[] getDirectory(DataAccess da)
        {
            string[] dir;
            const string directAddition = @"Engineering Base\EB-Exporte\Dokumentation";
            if (da.FolderProject.Substring(da.FolderProject.Length - 1, 1) == @"\")             
                dir = Directory.GetDirectories(String.Format("{0}{1}", da.FolderProject, directAddition));            
            else
                dir = Directory.GetDirectories(String.Format("{0}\\{1}", da.FolderProject, directAddition));            
            return dir;
        }

        static string setParentId(DataAccess da)
        {
            string parentId = "";

            parentId = String.Format("{0}/{1}/{2}/{3}", "ARCPATH:", "Projekte", da.FolderPrTypELOName, da.AnlageName);                                                                                
            if (!isFolderDocuExist(da.Ebene2Name, parentId, false))
                    createFolder(da.Ebene2Name, parentId);
            parentId = String.Format("{0}/{1}",parentId, da.Ebene2Name); 
            if (!isFolderDocuExist(da.Ebene3Name50, parentId, false))
                    createFolder(da.Ebene3Name50, parentId);
            if (!isFolderDocuExist(da.Ebene3Name60, parentId, false))
                    createFolder(da.Ebene3Name60, parentId);
            parentId = String.Format("{0}/{1}", parentId, da.Ebene3Name50);
           // parentId = String.Format("ARCPATH:{0}", parentId);
            return parentId;
        }

        static bool isFolderDocuExist(string folder, string actFolder, bool isFile)
        {
            bool result = false;
            FindInfo fi = new FindInfo();
            fi.findChildren = new FindChildren();
            fi.findChildren.parentId = actFolder;
            fi.findChildren.endLevel = 0;

            FindResult fr = conn.Ix.findFirstSords(fi, findMax, SordC.mbMin);
            foreach (Sord sord in fr.sords)
            {
                if (!isFile && sord.name == folder && sord.type < SordC.LBT_DOCUMENT && 0 < sord.type)
                    result = true;
                if (isFile && sord.name == folder && sord.type < SordC.LBT_DOCUMENT_MAX && SordC.LBT_DOCUMENT < sord.type)
                    result = true;
            }
            return result;
        }

        static void createFolder(string folder, string actFolder)
        {
            EditInfo ed = conn.Ix.createSord(actFolder, "", EditInfoC.mbSord);
            ed.sord.name = folder;
            ed.sord.id = conn.Ix.checkinSord(ed.sord, SordC.mbAll, LockC.NO);
        }

        static string nameFolderFile(string folder)
        {
            const String pattern = @"([.\\])";
            String[] elements = Regex.Split(folder, pattern);
            if (elements[elements.Length - 2] != ".")
                return elements[elements.Length - 1];	
            else
                return elements[elements.Length - 3];	
        }

        static void createDocument(string file, string actFolder)
        {
            Document doc = new Document();
            //Step 1
            EditInfo ed = conn.Ix.createDoc(actFolder, "", null, EditInfoC.mbSordDocAtt);
            Sord sord = ed.sord;
            sord.name = nameFolderFile(file);            
            sord.IDateIso = DateTime.Now.ToString("u");
            //Step 2            
            doc.docs = new DocVersion[] { new DocVersion() };
            doc.docs[0].pathId = sord.path;
            doc.docs[0].encryptionSet = sord.details.encryptionSet;
            doc.docs[0].ext = conn.GetFileExt(file);
            doc = conn.Ix.checkinDocBegin(doc);
            // Step 3
            doc.docs[0].uploadResult = conn.Upload(doc.docs[0].url, file);
            // Step 4
            doc = conn.Ix.checkinDocEnd(sord, SordC.mbAll, doc, LockC.NO);
        }
    }
}
