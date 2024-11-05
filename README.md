# Fiks Saksfaser nuget-pakke og eksempler

## Nuget pakke
Prosjektet `KS.Fiks.Saksfaser.Models.V1` inneholder hjelpeklasser for protokollen og produserer nuget pakken `KS.Fiks.Saksfaser.Models.V21 som inneholder hjelpeklassene, json skjema og genererte modeller for Fiks Saksfaser protokollen.

## Generering

Se prosjektet `KS.Fiks.Saksfaser.JsonModelGenerator` og `Generate.cs` for hvordan genereringen blir gjort

### Skript for å generere modeller

`GenerateModels.sh` forventer at json-skjemaene er plassert under `/Schema/V1`.
Den kopierer kopierer json skjema og genererer C# klasser i prosjektet `KS.Fiks.Saksfaser.Models` som blir pakket til nuget.

Bygg-pipeline med Jenkinsfile sørger for å hente siste versjon av skjema fra specification prosjektet [fiks-saksfaser-specification](https://github.com/ks-no/fiks-saksfaser-specification)
