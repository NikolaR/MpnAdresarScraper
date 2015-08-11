using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinistarstvoProsveteAdresarScraper.ObjectModel
{
    public class Institution
    {
        public string DetailsRelativeUrl { get; set; }

        public string Id { get; set; }

        public string Name { get; set; }

        public string NazivSkolskeUprave { get; set; }

        public string Vlasnistvo { get; set; }

        public string MaticniBroj { get; set; }

        public string Pib { get; set; }

        public string Okrug { get; set; }

        public string Opstina { get; set; }

        public string PostanskiBroj { get; set; }

        public string Mesto { get; set; }

        public string Ulica { get; set; }

        public string KontaktTelefon { get; set; }

        public string Faks { get; set; }

        public string KontaktEPosta { get; set; }

        public string VebSajt { get; set; }

        public string VrstaUstanove { get; set; }

        //Врста установе:	Основна школа
        //Назив школске управе:	Ниш
        //Назив установе:	ОШ Ћеле кула
        //Власништво:	Државно
        //Матични број:	7174365
        //ПИБ:	100615178
        //Округ:	Нишавски округ
        //Општина:	Ниш - Медиана
        //Поштански број:	18000
        //Место:	Ниш
        //Улица:	Радних бригада бб
        //Контакт телефон:	018-232-979
        //Факс:	018-232-979
        //Контакт е-пошта:	oscelekula69@gmail.com
        //Веб сајт:	Податак није унесен
    }
}
