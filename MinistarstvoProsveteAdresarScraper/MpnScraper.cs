using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using MinistarstvoProsveteAdresarScraper.ObjectModel;

namespace MinistarstvoProsveteAdresarScraper
{
    public class MpnScraper
    {
        private readonly string _address;

        public MpnScraper(string address)
        {
            _address = address;
        }

        public async Task<List<County>> GetCountiesForInstitutionType(string institutionTypeId)
        {
            HttpClient client = GetClient();
            var respObj = await client.PostAsync("",
                new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("vrsta", institutionTypeId)
                }));
            var respHtml = await respObj.Content.ReadAsStringAsync();

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(respHtml);
            var okrugSelects = doc.DocumentNode.SelectNodes("//select[@name=\"okrug\"]");
            if (okrugSelects.Count == 0)
                throw new UnknownHtmlException("okrug select not found");
            var okrugSelect = okrugSelects[0];
            var option = okrugSelect.FirstChild;
            if (option == null)
                throw new UnknownHtmlException("okrug select has no children");

            List<County> toReturn = new List<County>();
            while (option != null)
            {
                var id = option.GetAttributeValue("value", string.Empty);
                if (option.NextSibling == null || option.NextSibling.Name != "#text")
                    throw new UnknownHtmlException("okrug option not followed by text");
                var name = option.NextSibling.InnerText;
                toReturn.Add(new County(id, name));
                option = option.NextSibling.NextSibling;
            }

            return toReturn;
        }

        public async Task<List<Municipality>> GetMunicipalitiesForInstitutionTypeAndCounty(string institutionTypeId,
            string countyId)
        {
            HttpClient client = GetClient();
            var respObj = await client.PostAsync("",
                new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("vrsta", institutionTypeId),
                    new KeyValuePair<string, string>("okrug1", countyId),
                    new KeyValuePair<string, string>("okrug", countyId)
                }));
            var respHtml = await respObj.Content.ReadAsStringAsync();

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(respHtml);
            var opstinaSelect = doc.DocumentNode.SelectSingleNode("//select[@name=\"opstina\"]");
            if (opstinaSelect == null)
                throw new UnknownHtmlException("opstina select not found");
            var option = opstinaSelect.FirstChild;

            List<Municipality> toReturn = new List<Municipality>();
            while (option != null)
            {
                var id = option.GetAttributeValue("value", string.Empty);
                if (id == string.Empty)
                {
                    option = option.NextSibling.NextSibling;
                    continue;
                }
                if (option.NextSibling == null || option.NextSibling.Name != "#text")
                    throw new UnknownHtmlException("okrug option not followed by text");
                var name = option.NextSibling.InnerText;
                toReturn.Add(new Municipality(id, name));
                option = option.NextSibling.NextSibling;
            }

            return toReturn;
        }

        public async Task<List<Institution>> GetInstitutionsDetailsUrls(string institutionTypeId, string countyId,
            string municipalityId, string ownershipType)
        {
            HttpClient client = GetClient();
            var respObj = await client.PostAsync("",
                new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("vrsta", institutionTypeId),
                    new KeyValuePair<string, string>("okrug1", countyId),
                    new KeyValuePair<string, string>("okrug", countyId),
                    new KeyValuePair<string, string>("opstina", municipalityId),
                    new KeyValuePair<string, string>("vlasnistvo", ownershipType),
                    new KeyValuePair<string, string>("dugme", MpnKnownItems.PronadjiDugmeValue)
                }));
            var respHtml = await respObj.Content.ReadAsStringAsync();

            HtmlDocument doc = new HtmlDocument();
            List<Institution> toReturn = new List<Institution>();
            doc.LoadHtml(respHtml);
            var allRows = doc.DocumentNode.SelectNodes("//tr");
            var dataRows = allRows.Skip(2).Where(r => r.ChildNodes.Count > 4);
            foreach (var row in dataRows)
            {
                var cells = row.ChildNodes.Where(cn => cn.Name == "td").ToList();
                string detailsRelativeUrl, name;
                detailsRelativeUrl = cells[0].ChildNodes.Where(cn => cn.Name == "a").Single().GetAttributeValue("href", string.Empty);
                var institution = new Institution()
                {
                    DetailsRelativeUrl = detailsRelativeUrl
                };
                toReturn.Add(institution);
            }

            return toReturn;
        }

        public async Task PopulateInstitutionDetailsUsingDetailUrls(List<Institution> institutions)
        {
            List<HtmlNode> nodes;
            foreach (var institution in institutions)
            {
                HttpClient client = GetClient();
                var respHtml = await client.GetStringAsync(institution.DetailsRelativeUrl);
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(respHtml);

                var tables = doc.DocumentNode.SelectNodes("//table");
                if (tables.Count != 2)
                    throw new UnknownHtmlException("expected 2 tables in details view");

                var dataTable = tables[1];
                var rows = dataTable.ChildNodes.Where(cn => cn.Name == "tr").ToList();
                foreach (var row in rows)
                {
                    string name, value;
                    var cells = row.ChildNodes.Where(cn => cn.Name == "td").ToList();
                    if (cells.Count != 2)
                        throw new UnknownHtmlException("expected 2 columns in details table");
                    nodes = cells[0].ChildNodes.Where(cn => cn.Name == "b").ToList();
                    if (nodes.Count != 1)
                        throw new UnknownHtmlException("expected detail field name in b tag");
                    name = nodes[0].InnerText;
                    value = cells[1].InnerText;

                    if (name == "Врста установе:")
                        institution.VrstaUstanove = value;
                    if (name == "Назив школске управе:")
                        institution.NazivSkolskeUprave = value;
                    if (name == "Назив установе:")
                        institution.Name = value;
                    if (name == "Власништво:")
                        institution.Vlasnistvo = value;
                    if (name == "Матични број:")
                        institution.MaticniBroj = value;
                    if (name == "ПИБ:")
                        institution.Pib = value;
                    if (name == "Округ:")
                        institution.Okrug = value;
                    if (name == "Општина:")
                        institution.Opstina = value;
                    if (name == "Поштански број:")
                        institution.PostanskiBroj = value;
                    if (name == "Место:")
                        institution.Mesto = value;
                    if (name == "Улица:")
                        institution.Ulica = value;
                    if (name == "Контакт телефон:")
                        institution.KontaktTelefon = value;
                    if (name == "Факс:")
                        institution.Faks = value;
                    if (name == "Контакт е-пошта:")
                        institution.KontaktEPosta = value;
                    if (name == "Веб сајт:")
                        institution.VebSajt = value;
                }
            }
        }

        private HttpClient GetClient()
        {
            HttpClient client = new HttpClient()
            {
                BaseAddress = new Uri(_address)
            };

            return client;
        }
    }
}
