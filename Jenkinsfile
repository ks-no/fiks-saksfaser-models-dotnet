pipeline {
    options {
        buildDiscarder(logRotator(numToKeepStr: '50', artifactNumToKeepStr: '50'))
        disableConcurrentBuilds()
        timeout(time: 60, unit: 'MINUTES')
        timestamps ()
    }
    agent any
    parameters {
        booleanParam(name: 'isRelease', defaultValue: false, description: 'Skal prosjektet releases?')
        booleanParam(name: 'pushToNugetOrg', defaultValue: false, description: 'Skal artifaktet pushes til nuget.org? Kun relevant for pre-release bygg da release alltid pushes dit')
        string(name: "specifiedVersion", defaultValue: "", description: "Hva er det nye versjonsnummeret (X.X.X)? Som default releases snapshot-versjonen")
        text(name: "releaseNotes", defaultValue: "Ingen endringer utført", description: "Hva er endret i denne releasen?")
        text(name: "securityReview", defaultValue: "Endringene har ingen sikkerhetskonsekvenser", description: "Har endringene sikkerhetsmessige konsekvenser, og hvilke tiltak er i så fall iverksatt?")
        string(name: "reviewer", defaultValue: "Endringene krever ikke review", description: "Hvem har gjort review?")
        string(name: "triggerbranch", defaultValue: "main", description: "Hvilke branch fra KS.Fiks.Saksfaser.Specification skal det bygges fra?" )
    }
    environment {
        MODELS_FOLDER = 'KS.Fiks.Saksfaser.Models.V1'
        GENERATOR_FOLDER = 'KS.Fiks.Saksfaser.JsonModelGenerator'
    }
    stages {
        stage('Initialize') {
            steps {
                script {
                    env.GIT_SHA = sh(returnStdout: true, script: 'git rev-parse HEAD').substring(0, 7)
                    env.REPO_NAME = scm.getUserRemoteConfigs()[0].getUrl().tokenize('/').last().split("\\.")[0]
                    env.CURRENT_VERSION = findVersionPrefix()
                    env.NEXT_VERSION = params.specifiedVersion == "" ? incrementVersion(env.CURRENT_VERSION) : params.specifiedVersion
                    if(params.isRelease) {
                      env.VERSION_SUFFIX = ""
                      env.BUILD_SUFFIX = ""
                    } else {
                      def timestamp = getTimestamp()
                      def rc = params.triggerbranch == "main" && env.GIT_BRANCH == "main" ? "rc." : ""
                      env.VERSION_SUFFIX = "${rc}build.${timestamp}"
                      env.BUILD_SUFFIX = "--version-suffix ${env.VERSION_SUFFIX}"
                    }
                    print("Listing all environment variables:")
                    sh 'printenv'
                }
            }
        }
        stage('Fetch specification') {
          steps { 
            dir('temp') {
              git branch: params.triggerbranch,
              url: 'https://github.com/ks-no/fiks-saksfaser-specification.git'
              stash(name: 'jsonSchemas', includes: 'Schema/V1/*')
              stash(name: 'meldingstyper', includes: 'Schema/V1/meldingstyper/meldingstyper.json')
            }
          }
        }
        stage('Generate models') {
          agent {
            docker {
              image "mcr.microsoft.com/dotnet/sdk:8.0"
              args '-v $HOME/.nuget:/.nuget -v $HOME/.dotnet:/.dotnet -e DOTNET_CLI_HOME=/tmp -e XDG_DATA_HOME=/tmp'
              registryUrl 'https://mcr.microsoft.com'
              registryCredentialsId 'artifactory-token-based'
            }
          }
          steps {
            dir("${GENERATOR_FOLDER}") {
              unstash 'jsonSchemas'
              sh 'dotnet run Schema/V1 output'
              stash(name: 'generated', includes: 'output/**')
            }
          }
          post {
            always {
              deleteDir()
              }
            }     
        }
        stage('Unfold artifacts') {
          steps {
            dir("unfold") {
              unstash 'generated'
              sh 'mv output/* .'
              sh 'rm -r output'
              stash(name: 'models', includes: '**')
            }
          }
        }
        stage('Build') {
          environment {
            NUGET_HTTP_CACHE_PATH = "${env.WORKSPACE + '@tmp/cache'}"
            NUGET_CONF = credentials('nuget-config')
          }
          agent {
            docker {
              image "mcr.microsoft.com/dotnet/sdk:8.0"
              args '-v $HOME/.nuget:/.nuget -v $HOME/.dotnet:/.dotnet -e DOTNET_CLI_HOME=/tmp -e XDG_DATA_HOME=/tmp'
              registryUrl 'https://mcr.microsoft.com'
            }
          }
          steps {
            dir("${MODELS_FOLDER}") {
              unstash 'jsonSchemas'
              unstash 'meldingstyper'
              unstash 'models'
              sh 'dotnet restore --configfile ${NUGET_CONF}'
              sh 'dotnet build --no-restore -c Release ${BUILD_SUFFIX}'
              sh 'mv **/Release/*.nupkg .'
              sh 'mv **/Release/*.snupkg .'
              stash(name: 'builtnupkg', includes: '*.nupkg')
              stash(name: 'builtsnupkg', includes: '*.snupkg')
            }
          }
          post {
           always {
             deleteDir()
             }
           }
        }
        stage('Sign package') {
          environment {
            NUGET_HTTP_CACHE_PATH = "${env.WORKSPACE + '@tmp/cache'}"
            CODE_SIGN_CERT = credentials('ks-codesign-combo')
            CODE_SIGN_KEY = credentials('ks-codesign-combo-passwd')
            TIMESTAMP_URL = 'http://timestamp.digicert.com'
          }
          agent {
            docker {
              image "mcr.microsoft.com/dotnet/sdk:8.0"
              args '-v $HOME/.nuget:/.nuget -v $HOME/.dotnet:/.dotnet -e DOTNET_CLI_HOME=/tmp -e XDG_DATA_HOME=/tmp'
              registryUrl 'https://mcr.microsoft.com'
            }
          }
          steps {
            dir("tmpsign") {
              unstash 'builtnupkg'
              sh 'dotnet nuget sign *.nupkg --timestamper ${TIMESTAMP_URL} --certificate-path ${CODE_SIGN_CERT} --certificate-password ${CODE_SIGN_KEY}'
              stash(name: 'signednupkg', includes: '*.nupkg')
            }
          }
        post {
          always {
            deleteDir()
            }
          }
        }
        stage('Push to Artifactory') {
          environment {
            NUGET_HTTP_CACHE_PATH = "${env.WORKSPACE + '@tmp/cache'}"
            NUGET_ACCESS_KEY = credentials('artifactory-token-based')
            NUGET_PUSH_REPO = 'https://artifactory.fiks.ks.no/artifactory/api/nuget/nuget-all'
          }
          agent {
            docker {
              image "mcr.microsoft.com/dotnet/sdk:8.0"
              args '-v $HOME/.nuget:/.nuget -v $HOME/.dotnet:/.dotnet -e DOTNET_CLI_HOME=/tmp -e XDG_DATA_HOME=/tmp'
              registryUrl 'https://mcr.microsoft.com'
            }
          }
          steps {
            dir("tmppush") {
              unstash 'signednupkg'
              unstash 'builtsnupkg'
              sh 'dotnet nuget push *.nupkg -k ${NUGET_ACCESS_KEY} -s ${NUGET_PUSH_REPO}'
            }
          }
          post {
            always {
              deleteDir()
              }
            }                    
        }
        stage('Push to nuget.org') {
          when {
            anyOf {
              expression { params.isRelease }
              expression { params.pushToNugetOrg }
            }
          }
          environment {
            NUGET_HTTP_CACHE_PATH = "${env.WORKSPACE + '@tmp/cache'}"
            NUGET_ACCESS_KEY = credentials('ks-nuget-api-key')
            NUGET_PUSH_REPO = 'https://api.nuget.org/v3/index.json'
          }
          agent {
            docker {
              image "mcr.microsoft.com/dotnet/sdk:8.0"
              args '-v $HOME/.nuget:/.nuget -v $HOME/.dotnet:/.dotnet -e DOTNET_CLI_HOME=/tmp -e XDG_DATA_HOME=/tmp'
              registryUrl 'https://mcr.microsoft.com'
            }
          }
          steps {
            dir("tmpnuget") {
              unstash 'signednupkg'
              unstash 'builtsnupkg'
              sh 'dotnet nuget push *.nupkg -k ${NUGET_ACCESS_KEY} -s ${NUGET_PUSH_REPO}'
            }
          }
          post {
           always {
             deleteDir()
             }
           }         
        }
        stage('Set next version and push') {
          when {
            allOf {
              expression { params.isRelease }
              expression { return env.NEXT_VERSION }
              expression { return env.CURRENT_VERSION }
            }
          }
          steps {
            dir("${MODELS_FOLDER}") {
              gitCheckout(env.BRANCH_NAME)
              gitTag(isRelease, env.CURRENT_VERSION)
              prepareDotNetNoBuild(env.NEXT_VERSION)
              script {
                currentBuild.description = "${env.user} released version ${env.CURRENT_VERSION}"
              }
            }
          }
          post {
            success {
              gitPush()
              createGithubRelease env.REPO_NAME, params.reviewer, params.releaseNotes, env.CURRENT_VERSION, env.user
            }
          }
        }
    } 
  post {
    always {
      deleteDir()
      }
    }
}

def findVersionPrefix() {
  
    def files = findCsprojFiles()
    
    def versions = files.collect {
      echo("Checking ${it}")
      return extractVersion(readFile(file: it.getPath().trim(), encoding: 'UTF-8'))
    }.findAll {
      return it != null && ! it.trim().isEmpty()
    }
    echo "Found ${versions.size()} versions"
    if(versions.size() > 0 && versions[0] != "") {
      def currentVersion = versions[0]
      echo "Version: ${currentVersion}"
      return currentVersion
    } else {
      throw new Exception("No versionPrefix fond in csproj files")
    }
}

@NonCPS
def extractVersion(xml) {
  def debom = { data ->
    if(data?.length() > 0 && data[0] == '\uFEFF') return data.drop(1) else return data
  }
  def xmlData = debom.call(xml)
  def Project = new XmlSlurper().parseText(xmlData)
  def version = Project['PropertyGroup']['VersionPrefix'].text().trim()
  echo("Found version ${version}")
  return version
}

def findCsprojFiles() {
  def files = findFiles(glob: '**/*.csproj').findAll {
    return it.getName().toUpperCase().contains("TEST") == false && it.getName().toUpperCase().contains("EXAMPLE") == false
  }
  if(files.size() == 0) {
    throw new Exception("No csproj files found")
  }
  echo("Found ${files.size()} csproj files")
  return files
}

def incrementVersion(versionString) {
    def p = versionPattern()
    def m = p.matcher(versionString)
    if(m.find()) {
        def major = m.group(1) as Integer
        def minor = m.group(2) as Integer
        def patch = m.group(3) as Integer
        return "${major}.${minor}.${++patch}"
    } else {
        return null
    }
}

def versionPattern() {
  println('version pattern')
  return java.util.regex.Pattern.compile("^(\\d+)\\.(\\d+)\\.(\\d+)(.*)?")
}

def getTimestamp() {
  println('timestamp')
  return java.time.OffsetDateTime.now().format(java.time.format.DateTimeFormatter.ofPattern("yyyyMMddHHmmssSSS"))
}