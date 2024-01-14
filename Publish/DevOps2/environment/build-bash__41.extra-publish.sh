set -e


#---------------------------------------------------------------------
# args
args_="

export basePath=/root/temp/svn
export NUGET_PATH=$basePath/Publish/release/.nuget

# "

if [ ! $NUGET_PATH ]; then NUGET_PATH=$basePath/Publish/release/.nuget; fi


#---------------------------------------------------------------------
echo '#41.StressTest-publish.sh  -> #1 nothing to do'
 



