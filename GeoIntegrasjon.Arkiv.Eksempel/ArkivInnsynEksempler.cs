using GeoIntegrasjon.Arkiv.Eksempel.ArkivInnsyn;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoIntegrasjon.Arkiv.Eksempel
{
    public static class ArkivInnsynEksempler
    {
        public static void TestInnsyn()
        {
            string urlTilArkivinnsyn = ConfigurationManager.AppSettings["arkivinnsynurl"];
            ArkivInnsynPortClient arkivInnsynClient = new ArkivInnsynPortClient("ArkivInnsyn", urlTilArkivinnsyn);

            arkivInnsynClient.ClientCredentials.UserName.UserName = ConfigurationManager.AppSettings["brukernavn"];
            arkivInnsynClient.ClientCredentials.UserName.Password = ConfigurationManager.AppSettings["passord"];

            //************************************************************
            //Oppsett kontekst
            ArkivKontekst kontekst = new ArkivKontekst();
            kontekst.klientnavn = "Fagsystem 1";
            kontekst.klientversjon = "v2.1";
            kontekst.referanseoppsett = "Fagsystem 1 referanseoppsett"; //Nøkkel til oppsett i arkivet, må være registrert der.

            var arkivdeler = arkivInnsynClient.HentKodeliste("Arkivdel", kontekst);
            var mappetyper = arkivInnsynClient.HentKodeliste("Mappetype", kontekst);
            var klassifikasjonssystemer = arkivInnsynClient.HentKodeliste("Klassifikasjonssystem", kontekst);
            var informasjonstyper = arkivInnsynClient.HentKodeliste("Informasjonstype", kontekst);
            var korrespondanseparttyper = arkivInnsynClient.HentKodeliste("Korrespondanseparttype", kontekst);
            var journalenheter = arkivInnsynClient.HentKodeliste("Journalenhet", kontekst);
            var dokumenttyper = arkivInnsynClient.HentKodeliste("Dokumenttype", kontekst);
            
            //var saksbehandlere = arkivInnsynClient.HentKodeliste("SaksbehandlerInit", kontekst);
            //var administrativEnheter = arkivInnsynClient.HentKodeliste("AdministrativEnhetInit", kontekst);



        }
        
    }
}
