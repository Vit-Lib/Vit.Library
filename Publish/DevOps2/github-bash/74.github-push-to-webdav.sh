set -e


#---------------------------------------------------------------------
# args
args_="

export basePath=/root/temp/svn

export APPNAME=Vit.Library
export appVersion=2.2.14

export WebDav_BaseUrl="https://nextcloud.xxx.com/remote.php/dav/files/release/releaseFile/ki_jenkins"
export WebDav_User="username:pwd"

# "





#----------------------------------------------
echo "github-push release file to WebDav"
bash $basePath/Publish/DevOps2/release-bash/78.push-releaseFiles-to-webdav.bash;

 
 
