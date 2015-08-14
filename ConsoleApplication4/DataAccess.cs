using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBDocu_ELO_Export
{
    class DataAccess
    {
        private string anlageName;
        private string ebene2Name;
        private string ebene3Name50;
        private string ebene3Name60;
        private string folderProject;
        private string folderPrTypELOName;
        private short folderPrTyp;

       

        public DataAccess()
        {
            using (ProjektELODataContext pr = new ProjektELODataContext())
            {
                string UserName = Environment.UserName;
                int? projektNumber = pr.tblEB_ELO_Projektinfos.Where(a => a.Benutzer == UserName).Select(b => b.Projektnummer).FirstOrDefault<int?>();
                if (projektNumber != null)
                {
                    var tPr = pr.tblProjektes.Where(a => a.Projektnummer == projektNumber).Select(b => b.Anlagenname).ToList<String>();
                    anlageName = projektNumber.ToString().Length == 4 ? String.Format("00{0}", projektNumber.ToString()) : projektNumber.ToString().Length == 5 ? String.Format("0{0}", projektNumber.ToString()) : projektNumber.ToString(); 
                    anlageName += " " + tPr[0];
                    tPr = pr.tblProjektes.Where(a => a.Projektnummer == projektNumber).Select(b => b.Projektpfad).ToList<String>();
                    folderProject = tPr[0];
                    tPr = pr.tblELONamenProjekteEbene2s.Where(a => a.Ebene2NameID == 110).Select(b => b.Ebene2Name).ToList<String>();
                    ebene2Name = String.Format("110 {0}", tPr[0]);
                    tPr = pr.tblELONamenProjekteEbene3s.Where(a => a.Ebene2NameID == 110 & a.Ebene3NameID == 50).Select(b => b.Ebene3Name).ToList<String>();
                    ebene3Name50 = String.Format("050 {0}", tPr[0]);
                    tPr = pr.tblELONamenProjekteEbene3s.Where(a => a.Ebene2NameID == 110 & a.Ebene3NameID == 60).Select(b => b.Ebene3Name).ToList<String>();
                    ebene3Name60 = String.Format("060 {0}", tPr[0]);
                    folderPrTyp = pr.tblProjektes.Where(a => a.Projektnummer == projektNumber).Select(b => b.Projekttyp).FirstOrDefault<short>();
                    tPr = pr.tblProjekttypens.Where(a => a.Projekttyp == folderPrTyp).Select(b => b.ELOName).ToList<String>();
                    folderPrTypELOName = tPr[0];
                }
                
            }
        }

        public string AnlageName { get { return anlageName; } }
        public string FolderProject { get { return folderProject; } }
        public string Ebene2Name { get { return ebene2Name; } }
        public string Ebene3Name50 { get { return ebene3Name50; } }
        public string Ebene3Name60 { get { return ebene3Name60; } }
        public string FolderPrTypELOName { get { return folderPrTypELOName; } }
        public short FolderPrTyp { get { return folderPrTyp; } }
    }
}
