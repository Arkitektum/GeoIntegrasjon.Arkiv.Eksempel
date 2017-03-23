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

            // Hent sak lagret med fagnøkkel INNSYN/12345
            SakEksternNoekkel ekstnok = new SakEksternNoekkel();
            ekstnok.eksternnoekkel = new EksternNoekkel();
            ekstnok.eksternnoekkel.fagsystem = "INNSYN";
            ekstnok.eksternnoekkel.noekkel = "12345";

            var saker = arkivInnsynClient.FinnSaksmapperGittNoekkel(ekstnok, false, false, false, false, kontekst);

            // Hent sak med gnr/bnr 1/5 inkl. saksparter
            List<Soekskriterie> sok = new List<Soekskriterie>();
            Soekefelt arg1 = new Soekefelt();
            arg1.feltnavn = "matrikkelnummer.gaardsnummer";
            arg1.feltverdi = "1";
            Soekskriterie sk1 = new Soekskriterie();
            sk1.@operator = SoekeOperatorEnum.EQ;
            sk1.Kriterie = arg1;
            sok.Add(sk1);

            Soekefelt arg2 = new Soekefelt();
            arg2.feltnavn = "matrikkelnummer.bruksnummer";
            arg2.feltverdi = "5";
            Soekskriterie sk2 = new Soekskriterie();
            sk2.@operator = SoekeOperatorEnum.EQ;
            sk2.Kriterie = arg2;
            sok.Add(sk2);
            
            var sakserPågnr = arkivInnsynClient.FinnSaksmapper(sok.ToArray(), false, false, false, false, kontekst);

            JournpostEksternNoekkel ekstnok2 = new JournpostEksternNoekkel();
            ekstnok2.eksternnoekkel = new EksternNoekkel();
            ekstnok2.eksternnoekkel.fagsystem = "INNSYN";
            ekstnok2.eksternnoekkel.noekkel = "12345";

            var journalposter = arkivInnsynClient.FinnJournalposterGittNoekkel(ekstnok2, false, false, true, true, kontekst);

            // Finn journalposter med Status M eller S
            List<Soekskriterie> sokjournalposter = new List<Soekskriterie>();
            Soekefelt jarg1 = new Soekefelt();
            jarg1.feltnavn = "journalpost.journalstatus.kodeverdi";
            jarg1.feltverdi = "M";
            Soekskriterie jsk1 = new Soekskriterie();
            jsk1.@operator = SoekeOperatorEnum.EQ;
            jsk1.Kriterie = jarg1;
            sokjournalposter.Add(jsk1);

            Soekefelt jarg2 = new Soekefelt();
            jarg2.feltnavn = "journalpost.journalstatus.kodeverdi";
            jarg2.feltverdi = "S";
            Soekskriterie jsk2 = new Soekskriterie();
            jsk2.@operator = SoekeOperatorEnum.EQ;
            jsk2.Kriterie = jarg2;
            sokjournalposter.Add(jsk2);

            Soekefelt jarg3 = new Soekefelt();
            jarg3.feltnavn = "journalpost.referanseArkivdel.kodeverdi";
            jarg3.feltverdi = "BYGG";
            Soekskriterie jsk3 = new Soekskriterie();
            jsk3.@operator = SoekeOperatorEnum.EQ;
            jsk3.Kriterie = jarg3;
            sokjournalposter.Add(jsk3);

            var journres = arkivInnsynClient.FinnJournalposter(sokjournalposter.ToArray(), false, false, false, false, kontekst);

           
        }

    }
}
