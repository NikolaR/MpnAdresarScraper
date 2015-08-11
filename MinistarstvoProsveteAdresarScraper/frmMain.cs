using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MinistarstvoProsveteAdresarScraper.ObjectModel;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;

namespace MinistarstvoProsveteAdresarScraper
{
    public partial class frmMain : Form
    {
        private List<Institution> predskolskeUstanove;
        private List<Institution> osnovneSkole;
        private List<Institution> srednjeSkole;

        public frmMain()
        {
            InitializeComponent();
        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            btnDownload.Enabled = false;
            StartDownload()
                .ContinueWith(t =>
                {
                    btnDownload.Enabled = true;
                    if (t.Exception != null)
                    {
                        MessageBox.Show("Desila se greška prilikom skidanja podataka.\r\n\r\n" + t.Exception.InnerExceptions[0].Message);
                    }
                }, TaskContinuationOptions.ExecuteSynchronously);
        }

        private async Task StartDownload()
        {
            ClearLog();
            MpnScraper scraper = new MpnScraper(txtAdresa.Text);

            Log("Počinjem skidanje opština za predškolske ustanove...");
            var municipalitiesPs =
                await
                    scraper.GetMunicipalitiesForInstitutionTypeAndCounty(MpnKnownItems.VrstaUstanove.Predskolska,
                        MpnKnownItems.AllCountiesId);
            Log("Opštine skinute. Broj: " + municipalitiesPs.Count);

            Log("Počinjem skidanje opština za osnovne škole...");
            var municipalitiesOs =
                await
                    scraper.GetMunicipalitiesForInstitutionTypeAndCounty(MpnKnownItems.VrstaUstanove.OsnovnaSkola,
                        MpnKnownItems.AllCountiesId);
            Log("Opštine skinute. Broj: " + municipalitiesOs.Count);

            Log("Počinjem skidanje opština za srednje škole...");
            var municipalitiesSs =
                await
                    scraper.GetMunicipalitiesForInstitutionTypeAndCounty(MpnKnownItems.VrstaUstanove.SrednjaSkola,
                        MpnKnownItems.AllCountiesId);
            Log("Opštine skinute. Broj: " + municipalitiesSs.Count);

            
            predskolskeUstanove = new List<Institution>();
            foreach (var mun in municipalitiesPs)
            {
                Log("Skidam podatke o predškolskim ustanovama za opštinu: " + mun.Name);
                var inst = await scraper.GetInstitutionsDetailsUrls(MpnKnownItems.VrstaUstanove.OsnovnaSkola, MpnKnownItems.AllCountiesId,
                    mun.Id, MpnKnownItems.Vlasnistvo.Sve);
                Log("Postoji {0} ustanova u opštini. Skidam detalje...", inst.Count);
                await scraper.PopulateInstitutionDetailsUsingDetailUrls(inst);
                Log("Skinuti svi podaci o predškolskim ustanovama za opštinu " + mun.Name);
                predskolskeUstanove.AddRange(inst);
            }

            osnovneSkole = new List<Institution>();
            foreach (var mun in municipalitiesOs)
            {
                Log("Skidam podatke o osnovnim školama za opštinu: " + mun.Name);
                var inst = await scraper.GetInstitutionsDetailsUrls(MpnKnownItems.VrstaUstanove.OsnovnaSkola, MpnKnownItems.AllCountiesId,
                    mun.Id, MpnKnownItems.Vlasnistvo.Sve);
                Log("Postoji {0} ustanova u opštini. Skidam detalje...", inst.Count);
                await scraper.PopulateInstitutionDetailsUsingDetailUrls(inst);
                Log("Skinuti svi podaci o osnovnim školama za opštinu " + mun.Name);
                osnovneSkole.AddRange(inst);
            }

            srednjeSkole = new List<Institution>();
            foreach (var mun in municipalitiesSs)
            {
                Log("Skidam podatke o srednjim školama za opštinu: " + mun.Name);
                var inst = await scraper.GetInstitutionsDetailsUrls(MpnKnownItems.VrstaUstanove.SrednjaSkola, MpnKnownItems.AllCountiesId,
                    mun.Id, MpnKnownItems.Vlasnistvo.Sve);
                Log("Postoji {0} ustanova u opštini. Skidam detalje...", inst.Count);
                await scraper.PopulateInstitutionDetailsUsingDetailUrls(inst);
                Log("Skinuti svi podaci o srednjim školama za opštinu " + mun.Name);
                srednjeSkole.AddRange(inst);
            }

            Log("------------------------------");
            Log("Skinuto podataka o ukupno institucija: " + (predskolskeUstanove.Count + osnovneSkole.Count + srednjeSkole.Count));
            Log("Snimam podatke u Excel fajl...");

            FileDialog fd = new SaveFileDialog();
            fd.Filter = "Excel|*.xlsx";
            if (fd.ShowDialog(this) != DialogResult.OK)
                return;

            var path = fd.FileName;
            if (File.Exists(path))
                File.Delete(path);
            using (var xls = new ExcelPackage(new FileInfo(path)))
            {
                var worksheet = xls.Workbook.Worksheets.Add("Предшколске установе");
                WriteToWorksheet(predskolskeUstanove, worksheet);

                worksheet = xls.Workbook.Worksheets.Add("Основне школе");
                WriteToWorksheet(osnovneSkole, worksheet);

                worksheet = xls.Workbook.Worksheets.Add("Средње школе");
                WriteToWorksheet(srednjeSkole, worksheet);

                xls.Save();
            }
            Log("------------------------------");
            Log("------------------------------");
            Log("Skidanje podataka uspešno završeno!");
            Log("------------------------------");
        }

        private void WriteToWorksheet(List<Institution> institutions, ExcelWorksheet worksheet)
        {
            worksheet.Cells[1, 1].Value = "Врста установе";
            worksheet.Cells[1, 2].Value = "Назив школске управе";
            worksheet.Cells[1, 3].Value = "Назив установе";
            worksheet.Cells[1, 4].Value = "Власништво";
            worksheet.Cells[1, 5].Value = "Матични број";
            worksheet.Cells[1, 6].Value = "ПИБ";
            worksheet.Cells[1, 7].Value = "Округ";
            worksheet.Cells[1, 8].Value = "Општина";
            worksheet.Cells[1, 9].Value = "Поштански број";
            worksheet.Cells[1, 10].Value = "Место";
            worksheet.Cells[1, 11].Value = "Улица";
            worksheet.Cells[1, 12].Value = "Контакт телефон";
            worksheet.Cells[1, 13].Value = "Факс";
            worksheet.Cells[1, 14].Value = "Контакт е-пошта";
            worksheet.Cells[1, 15].Value = "Веб сајт";

            var i = 2;
            foreach (var institution in institutions)
            {
                worksheet.Cells[i, 1].Value = institution.VrstaUstanove;
                worksheet.Cells[i, 2].Value = institution.NazivSkolskeUprave;
                worksheet.Cells[i, 3].Value = institution.Name;
                worksheet.Cells[i, 4].Value = institution.Vlasnistvo;
                worksheet.Cells[i, 5].Value = institution.MaticniBroj;
                worksheet.Cells[i, 6].Value = institution.Pib;
                worksheet.Cells[i, 7].Value = institution.Okrug;
                worksheet.Cells[i, 8].Value = institution.Opstina;
                worksheet.Cells[i, 9].Value = institution.PostanskiBroj;
                worksheet.Cells[i, 10].Value = institution.Mesto;
                worksheet.Cells[i, 11].Value = institution.Ulica;
                worksheet.Cells[i, 12].Value = institution.KontaktTelefon;
                worksheet.Cells[i, 13].Value = institution.Faks;
                worksheet.Cells[i, 14].Value = institution.KontaktTelefon;
                worksheet.Cells[i, 15].Value = institution.VebSajt;
                i++;
            }


            worksheet.Cells[1, 1, 100, 15].AutoFitColumns(80);
        }

        private void ClearLog()
        {
            listView1.Items.Clear();
        }

        private void Log(string message)
        {
            var item = new ListViewItem(DateTime.Now.ToShortTimeString());
            item.SubItems.Add(message);
            listView1.Items.Insert(0, item);
        }

        private void Log(string messageFormat, params object[] args)
        {
            var item = new ListViewItem(DateTime.Now.ToShortTimeString());
            item.SubItems.Add(string.Format(messageFormat, args));
            listView1.Items.Insert(0, item);
        }
    }
}
