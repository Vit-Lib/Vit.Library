set -e

# export versionSuffix='.1234.preview'
# bash 22.add-suffix-to-app-version.bash


#---------------------------------------------------------------------
# args
args_="

export versionSuffix='  '

# "

# remove spaces
versionSuffix=${versionSuffix// /}

#----------------------------------------------
# basePath
if [ -z "$basePath" ]; then basePath=$PWD/../../..; fi



#----------------------------------------------
echo "#1 get appVersion"
# get csproj file with appVersion tag, if not exist get file with pack or publish tag
csprojPath=$(find ${basePath} -name *.csproj -exec grep '<appVersion>' -l {} \; | head -n 1);
if [ ! -f "$csprojPath" ]; then csprojPath=$(find ${basePath} -name *.csproj -exec grep '<pack>\|<publish>' -l {} \; | head -n 1);  fi
if [ -f "$csprojPath" ]; then export appVersion=`grep '<Version>' "$csprojPath" | grep -oE '\>(.*)\<' | tr -d '<>/'`;  fi
echo "appVersion from csproj: $appVersion"



export nextAppVersion="${appVersion}${versionSuffix}"
echo "nextAppVersion: $nextAppVersion"


#----------------------------------------------
echo "#2 change app version from [$appVersion] to [$nextAppVersion]" 
sed -i 's/'"$appVersion"'/'"$nextAppVersion"'/g'  `find ${basePath} -name *.csproj -exec grep '<pack>\|<publish>' -l {} \;`

