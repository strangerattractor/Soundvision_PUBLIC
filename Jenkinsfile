pipeline {
   agent any
   environment {
     APP_NAME = "SoundVision"
     RUBY = "C:\\Ruby25-x64\\bin\\u3d"
     MXBUILD = "\"C:\\Program Files (x86)\\Microsoft Visual Studio\\2017\\Community\\MSBuild\\15.0\\Bin\\MSBuild.exe\""
   }
   stages {
      stage('Build'){
         steps{

            echo 'Generate Build Number File'
            writeFile file: 'UnityProject/Assets/Resources/buildNumber.txt', text: "${BUILD_NUMBER}"

            echo 'Building Unity Project'
            bat "${RUBY} run -u 2018.4.3f1 -r -- -batchmode -nographics -quit -projectPath '${workspace}\\UnityProject' -executeMethod AppBuilder.Build"

            echo 'Stash unity build'
            stash includes: 'bin/**/*', name: 'unity build'
         }
      }
      stage('Test'){
         steps{
            echo 'Unit Test'
         }
      }
      stage('Setup')
      {
         steps{
            echo 'Build Installer'
            unstash 'unity build'

            bat "${MXBUILD} .\\setup\\setup.sln /property:Configuration=Release"

            stash includes: "${APP_NAME}.msi", name: 'installer build'
         }
      }
      stage('Publish')
      {
         steps{
            echo 'publish Build'
            unstash 'installer build'

            cifsPublisher(publishers: [[configName: 'Cylvester Share', transfers: [[cleanRemote: false, excludes: '', flatten: false, makeEmptyDirs: false, noDefaultExcludes: false, patternSeparator: '[, ]+', remoteDirectory: 'Build${BUILD_NUMBER}', remoteDirectorySDF: false, removePrefix: '', sourceFiles: 'SoundVision.msi']], usePromotionTimestamp: false, useWorkspaceInPromotion: false, verbose: false]])

         }
      }
   }
}
