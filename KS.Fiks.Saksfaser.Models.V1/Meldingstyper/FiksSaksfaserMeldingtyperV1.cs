namespace KS.Fiks.Saksfaser.Models.V1.Meldingstyper
{
    public class FiksSaksfaserMeldingtyperV1
    {
        // Forespørsler hent
        public const string HentSaksfaser = "no.ks.fiks.saksfaser.v1.saksfaser.hent";
        public const string HentSaksfase = "no.ks.fiks.saksfaser.v1.saksfase.hent";

        // Resultat på forespørsler-innsyn
        public const string ResultatHentSaksfaser = "no.ks.fiks.saksfaser.v1.saksfaser.hent.resultat";
        public const string ResultatHentSaksfase = "no.ks.fiks.saksfaser.v1.saksfase.hent.resultat";
        
        // Feilmeldinger
        public const string Ugyldigforespoersel = "no.ks.fiks.saksfaser.v1.feilmelding.ugyldigforespoersel";
        public const string Serverfeil = "no.ks.fiks.saksfaser.v1.feilmelding.serverfeil";
        public const string Ikkefunnet = "no.ks.fiks.saksfaser.v1.feilmelding.ikkefunnet";
        
        private static Dictionary<string, string> Skjemanavn;

        public static string GetSkjemanavn(string meldingstypeNavn)
        {
            if (Skjemanavn == null)
            {
                initSkjemanavn();
            }
            return Skjemanavn[meldingstypeNavn];
        }

        public static readonly List<string> HentTyper = new List<string>()
        {
            HentSaksfaser,
            HentSaksfase,
            ResultatHentSaksfaser,
            ResultatHentSaksfase
        };

        public static readonly List<string> FeilmeldingTyper = new List<string>()
        {
            Ugyldigforespoersel,
            Serverfeil,
            Ikkefunnet
        };

        public static bool IsHentType(string meldingstype)
        {
            return HentTyper.Contains(meldingstype);
        }

        public static bool IsFeilmelding(string meldingstype)
        {
            return FeilmeldingTyper.Contains(meldingstype);
        }

        public static bool IsGyldigProtokollType(string meldingstype)
        {
            return IsHentType(meldingstype) || IsFeilmelding(meldingstype);
        }

        private static void initSkjemanavn()
        {
            Skjemanavn = new Dictionary<string, string>();
            foreach (var meldingstype in HentTyper)
            {
                AddSkjemanavnTilDictionary(meldingstype);    
            }
            foreach (var meldingstype in FeilmeldingTyper)
            {
                AddSkjemanavnTilDictionary(meldingstype);    
            }
        }

        private static void AddSkjemanavnTilDictionary(string meldingstype)
        {
            Skjemanavn.Add(meldingstype, $"{meldingstype}.schema.json");
        }

    }
}
